using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ResultBoardClickable : MonoBehaviour
{
    [Header("移動対象")]
    public Transform moveTarget;

    [Header("カメラ基準で手前に持ってくる")]
    public bool useCameraRelativeTarget = true;
    public Camera targetCamera;
    public Vector3 cameraRelativePosition = new Vector3(0f, 0f, 1.25f);
    public Vector3 cameraRelativeEulerAngles = new Vector3(0f, 180f, 0f);

    [Header("固定移動先")]
    public Vector3 focusedPosition;
    public Vector3 focusedEulerAngles;

    [Header("移動時間")]
    public float moveDuration = 0.35f;
    public AnimationCurve moveCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("時間")]
    public bool useUnscaledTime = true;

    [Header("クリック設定")]
    public bool clickAgainToReturn = true;

    [Header("状態")]
    public bool isFocused = false;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Coroutine moveRoutine;

    void Awake()
    {
        if (moveTarget == null)
            moveTarget = transform;

        originalPosition = moveTarget.position;
        originalRotation = moveTarget.rotation;

        Collider col = GetComponent<Collider>();

        if (col != null)
            col.isTrigger = false;
    }

    public void ToggleFocus()
    {
        if (ResultCursorController.Instance != null &&
            !ResultCursorController.Instance.CanFocus(this))
            return;

        if (isFocused)
        {
            if (clickAgainToReturn)
                ReturnToOriginal();

            return;
        }

        MoveToFocus();
    }

    public void MoveToFocus()
    {
        if (isFocused)
            return;

        isFocused = true;

        if (ResultCursorController.Instance != null)
            ResultCursorController.Instance.RegisterFocusedBoard(this);

        GetFocusPose(out Vector3 targetPos, out Quaternion targetRot);
        StartMove(targetPos, targetRot);
    }

    public void ReturnToOriginal()
    {
        if (!isFocused)
            return;

        isFocused = false;
        StartMove(originalPosition, originalRotation);

        if (ResultCursorController.Instance != null)
            ResultCursorController.Instance.UnregisterFocusedBoard(this);
    }

    void GetFocusPose(out Vector3 pos, out Quaternion rot)
    {
        if (useCameraRelativeTarget)
        {
            Camera cam = targetCamera != null ? targetCamera : Camera.main;

            if (cam != null)
            {
                Transform ct = cam.transform;

                pos =
                    ct.position +
                    ct.right * cameraRelativePosition.x +
                    ct.up * cameraRelativePosition.y +
                    ct.forward * cameraRelativePosition.z;

                rot =
                    ct.rotation *
                    Quaternion.Euler(cameraRelativeEulerAngles);

                return;
            }
        }

        pos = focusedPosition;
        rot = Quaternion.Euler(focusedEulerAngles);
    }

    void StartMove(Vector3 targetPos, Quaternion targetRot)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine =
            StartCoroutine(MoveRoutine(targetPos, targetRot));
    }

    IEnumerator MoveRoutine(Vector3 targetPos, Quaternion targetRot)
    {
        Vector3 startPos = moveTarget.position;
        Quaternion startRot = moveTarget.rotation;
        float timer = 0f;

        while (timer < moveDuration)
        {
            float delta =
                useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            timer += delta;

            float t =
                moveDuration > 0f
                ? Mathf.Clamp01(timer / moveDuration)
                : 1f;

            float eased =
                moveCurve != null
                ? moveCurve.Evaluate(t)
                : t;

            moveTarget.position =
                Vector3.LerpUnclamped(startPos, targetPos, eased);

            moveTarget.rotation =
                Quaternion.SlerpUnclamped(startRot, targetRot, eased);

            yield return null;
        }

        moveTarget.position = targetPos;
        moveTarget.rotation = targetRot;
        moveRoutine = null;
    }
}
