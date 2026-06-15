using UnityEngine;

public class CollectPuzzleManager : MonoBehaviour
{
    [Header("生成")]
    public GameObject fragmentPrefab;

    [Header("個数")]
    public int minCount = 3;
    public int maxCount = 7;

    [Header("ドア")]
    public RoomPuzzleTarget puzzleTarget;

    [Header("UI")]
    public bool showUIOnPlayerEnter = true;

    [Header("取得SE")]
    public AudioClip[] collectSEClips;
    [Range(0f, 1f)]
    public float collectSEVolume = 1f;

    [Header("デバッグ")]
    public bool debugLog = true;

    private int targetCount;
    private int currentCount;
    private bool completed;

    void Start()
    {
        GenerateFragments();
    }

    public void GenerateFragments()
    {
        CollectSpawnArea[] areas =
            GetComponentsInChildren<CollectSpawnArea>(true);

        if (areas == null || areas.Length == 0)
        {
            Debug.LogWarning(name + " CollectSpawnArea がありません");
            return;
        }

        if (fragmentPrefab == null)
        {
            Debug.LogWarning(name + " FragmentPrefab がありません");
            return;
        }

        targetCount = Random.Range(minCount, maxCount + 1);
        currentCount = 0;
        completed = false;

        for (int i = 0; i < targetCount; i++)
        {
            CollectSpawnArea area =
                areas[Random.Range(0, areas.Length)];

            GameObject obj = Instantiate(
                fragmentPrefab,
                area.GetRandomPosition(),
                Quaternion.identity,
                transform
            );

            CollectFragment fragment =
                obj.GetComponent<CollectFragment>();

            if (fragment != null)
                fragment.manager = this;
        }

        if (debugLog)
            Debug.Log(name + " 欠片生成: " + targetCount);
    }

    public void BindUI()
    {
        if (!showUIOnPlayerEnter)
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

        PlayCollectSE(fragment != null ? fragment.transform.position : transform.position);

        if (CollectPuzzleImageUI.Instance != null)
            CollectPuzzleImageUI.Instance.Refresh(this);

        if (debugLog)
            Debug.Log(name + " 欠片取得: " + currentCount + " / " + targetCount);

        if (currentCount >= targetCount)
            CompletePuzzle();
    }

    void CompletePuzzle()
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
            CollectPuzzleImageUI.Instance.Refresh(this);

        if (debugLog)
            Debug.Log(name + " 欠片Puzzleクリア");
    }

    void PlayCollectSE(Vector3 position)
    {
        if (collectSEClips == null || collectSEClips.Length == 0)
            return;

        RandomAudioPlayer.PlayRandom(collectSEClips, position, collectSEVolume);
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
