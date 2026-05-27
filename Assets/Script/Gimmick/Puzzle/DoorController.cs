// DoorController.cs
//
// 扉を下にスライドして開く / 元の位置へ戻して閉じるスクリプト
//
// 役割:
// ・Open() で下にスライドして開く
// ・Close() で元の位置に戻って閉じる
//
// 重量ボタンのような
// 「押している間だけ開く」仕組みに対応するため、Close()を追加した版。

using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("動かす扉本体")]
    public Transform doorBody;

    [Header("開く設定")]
    public float slideDownDistance = 3f;
    public float moveSpeed = 2f;

    [Header("Collider設定")]
    [Tooltip("開いた時に扉のColliderを切る")]
    public bool disableCollidersWhenOpen = true;

    [Tooltip("閉じた時に扉のColliderを戻す")]
    public bool enableCollidersWhenClosed = true;

    [Header("デバッグ")]
    public bool debugLog = true;

    private Vector3 closedLocalPosition;
    private Vector3 openLocalPosition;

    private bool isOpen = false;
    private Coroutine moveRoutine;

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

        if (debugLog)
            Debug.Log(name + " を開きます");

        StartMove(openLocalPosition, true);
    }

    public void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (debugLog)
            Debug.Log(name + " を閉じます");

        // 閉じ始める時点でColliderを戻す
        if (enableCollidersWhenClosed)
            SetDoorColliders(true);

        StartMove(closedLocalPosition, false);
    }

    void StartMove(Vector3 targetLocalPosition, bool opening)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveRoutine(targetLocalPosition, opening));
    }

    IEnumerator MoveRoutine(Vector3 targetLocalPosition, bool opening)
    {
        while (Vector3.Distance(doorBody.localPosition, targetLocalPosition) > 0.01f)
        {
            doorBody.localPosition =
                Vector3.MoveTowards(
                    doorBody.localPosition,
                    targetLocalPosition,
                    moveSpeed * Time.deltaTime
                );

            yield return null;
        }

        doorBody.localPosition = targetLocalPosition;

        if (opening && disableCollidersWhenOpen)
            SetDoorColliders(false);
    }

    void SetDoorColliders(bool enabled)
    {
        if (doorBody == null)
            return;

        Collider[] colliders = doorBody.GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            if (col != null)
                col.enabled = enabled;
        }
    }
}
