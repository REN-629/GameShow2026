using UnityEngine;

public class CollectPuzzleManager : MonoBehaviour
{
    public GameObject fragmentPrefab;

    public int minCount = 3;
    public int maxCount = 7;

    public RoomPuzzleTarget puzzleTarget;

    public AudioClip[] collectSEClips;

    [Range(0f, 1f)]
    public float collectSEVolume = 1f;

    private int targetCount;
    private int currentCount;
    private bool completed;
    private bool generated;

    void Start()
    {
        GenerateFragments();
    }

    public void GenerateFragments()
    {
        if (generated)
            return;

        CollectSpawnArea[] areas = GetComponentsInChildren<CollectSpawnArea>(true);

        if (areas == null || areas.Length == 0)
            return;

        if (fragmentPrefab == null)
            return;

        targetCount = Random.Range(minCount, maxCount + 1);
        currentCount = 0;
        completed = false;
        generated = true;

        for (int i = 0; i < targetCount; i++)
        {
            CollectSpawnArea area = areas[Random.Range(0, areas.Length)];

            GameObject obj = Instantiate(fragmentPrefab, area.GetRandomPosition(), Quaternion.identity, transform);

            CollectFragment fragment = obj.GetComponent<CollectFragment>();

            if (fragment != null)
                fragment.manager = this;
        }
    }

    public void BindUI()
    {
        if (completed)
            return;

        if (CollectPuzzleImageUI.Instance != null)
            CollectPuzzleImageUI.Instance.Bind(this);
    }

    public void UnbindUI()
    {
        if (CollectPuzzleImageUI.Instance != null)
            CollectPuzzleImageUI.Instance.Unbind(this);
    }

    public void CollectFragment(CollectFragment fragment)
    {
        if (completed)
            return;

        currentCount++;

        if (collectSEClips != null && collectSEClips.Length > 0)
            RandomAudioPlayer.PlayRandom(collectSEClips, transform.position, collectSEVolume);

        if (CollectPuzzleImageUI.Instance != null)
            CollectPuzzleImageUI.Instance.Refresh(this);

        if (currentCount >= targetCount)
            CompletePuzzle();
    }

    public void CompletePuzzle()
    {
        if (completed)
            return;

        completed = true;

        if (puzzleTarget != null)
        {
            puzzleTarget.solveMethod = PuzzleSolveMethod.Normal;
            puzzleTarget.SetDoorOpen(true);
        }

        if (CollectPuzzleImageUI.Instance != null)
            CollectPuzzleImageUI.Instance.ForceHide(this);
    }

    public void AbandonPuzzle()
    {
        if (CollectPuzzleImageUI.Instance != null)
            CollectPuzzleImageUI.Instance.ForceHide(this);
    }

    public int GetCurrentCount()
    {
        return currentCount;
    }

    public int GetTargetCount()
    {
        return targetCount;
    }

    public int GetRemainingCount()
    {
        return Mathf.Max(0, targetCount - currentCount);
    }

    public bool IsCompleted()
    {
        return completed;
    }
}
