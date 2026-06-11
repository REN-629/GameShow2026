using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KeyTouchPuzzle : MonoBehaviour
{
    [Header("ドア開閉ターゲット")]
    public RoomPuzzleTarget puzzleTarget;

    [Header("反応対象")]
    public string playerTag = "Player";

    [Header("取得後")]
    public bool destroyOnTouch = true;
    public GameObject destroyTarget;

    [Header("演出")]
    public GameObject pickupEffectPrefab;
    public AudioClip pickupSE;
    [Range(0f, 1f)]
    public float pickupSEVolume = 1f;

    [Header("一度だけ")]
    public bool triggerOnce = true;

    [Header("デバッグ")]
    public bool debugLog = true;

    private bool triggered = false;

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Awake()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
            col.isTrigger = true;

        if (destroyTarget == null)
            destroyTarget = gameObject;

        if (puzzleTarget == null)
            puzzleTarget = GetComponent<RoomPuzzleTarget>();

        if (puzzleTarget == null)
            puzzleTarget = GetComponentInParent<RoomPuzzleTarget>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && triggered)
            return;

        if (!other.CompareTag(playerTag))
            return;

        triggered = true;

        OpenDoor();
        PlayEffect();

        if (debugLog)
            Debug.Log(name + " 鍵取得 → ドア開放");

        if (destroyOnTouch && destroyTarget != null)
            Destroy(destroyTarget);
    }

    void OpenDoor()
    {
        if (puzzleTarget == null)
            puzzleTarget = GetComponentInParent<RoomPuzzleTarget>();

        if (puzzleTarget != null)
        {
            puzzleTarget.solveMethod = PuzzleSolveMethod.Normal;
            puzzleTarget.SetDoorOpen(true);
            return;
        }

        if (RoomRuntimeManager.Instance != null && RoomRuntimeManager.Instance.currentRoom != null)
        {
            RoomRuntimeManager.Instance.currentRoom.SetDoorOpenCondition(true, PuzzleSolveMethod.Normal);
        }
    }

    void PlayEffect()
    {
        Vector3 pos = transform.position;

        if (pickupEffectPrefab != null)
            Instantiate(pickupEffectPrefab, pos, Quaternion.identity);

        if (pickupSE != null)
            AudioSource.PlayClipAtPoint(pickupSE, pos, pickupSEVolume);
    }
}
