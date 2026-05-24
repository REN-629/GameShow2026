// DoorController：下にスライドして開く扉
//
// パズルクリア時にRoomPuzzleStateからOpen()を呼ばれる。
// 破壊する場合はAttributeDurability/FractureThisと併用可能。

using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("動かす扉本体")]
    public Transform doorBody;

    [Header("開く設定")]
    public float slideDownDistance = 3f;
    public float openSpeed = 2f;

    [Header("開いた後にColliderを切る")]
    public bool disableCollidersWhenOpen = true;

    private Vector3 closedLocalPosition;
    private Vector3 openLocalPosition;
    private bool isOpen = false;
    private Coroutine openRoutine;

    public bool IsOpen => isOpen;

    void Start()
    {
        if (doorBody == null)
            doorBody = transform;

        closedLocalPosition = doorBody.localPosition;
        openLocalPosition = closedLocalPosition + Vector3.down * slideDownDistance;
    }

    public void Open()
    {
        if (isOpen)
            return;

        isOpen = true;

        if (openRoutine != null)
            StopCoroutine(openRoutine);

        openRoutine = StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        while (Vector3.Distance(doorBody.localPosition, openLocalPosition) > 0.01f)
        {
            doorBody.localPosition =
                Vector3.MoveTowards(
                    doorBody.localPosition,
                    openLocalPosition,
                    openSpeed * Time.deltaTime
                );

            yield return null;
        }

        doorBody.localPosition = openLocalPosition;

        if (disableCollidersWhenOpen)
            DisableColliders();
    }

    void DisableColliders()
    {
        Collider[] colliders = doorBody.GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            if (col != null)
                col.enabled = false;
        }
    }
}
