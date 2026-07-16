using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ResultBoardClickable : MonoBehaviour
{
    public enum FocusTargetMode
    {
        InspectorWorldPosition,
        CameraRelative,
        FocusPoint
    }

    [Header("移動対象")]
    [Tooltip("看板全体の一番上の親を指定してください。")]
    public Transform moveTarget;

    [Header("移動先の決め方")]
    public FocusTargetMode targetMode =
        FocusTargetMode.InspectorWorldPosition;

    [Header("Inspectorで世界座標を指定")]
    public Vector3 focusedWorldPosition;
    public Vector3 focusedWorldEulerAngles;

    [Header("カメラ基準")]
    public Camera targetCamera;
    public Vector3 cameraRelativePosition =
        new Vector3(0f, 0f, 1.25f);
    public Vector3 cameraRelativeEulerAngles =
        new Vector3(0f, 180f, 0f);

    [Header("指定Transform")]
    public Transform focusPoint;

    [Header("移動")]
    [Min(0f)]
    public float moveDuration = 0.35f;

    public AnimationCurve moveCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("操作")]
    public bool clickAgainToReturn = true;
    public bool useUnscaledTime = true;

    [Header("安全")]
    public float maxAllowedMoveDistance = 50f;

    [Header("カメラ導入中の操作")]
    public ResultCameraIntroMover cameraIntroMover;
    public bool blockWhileCameraIntro = true;

    [Header("状態")]
    public bool isFocused;
    public bool isMoving;

    private Vector3 originalWorldPosition;
    private Quaternion originalWorldRotation;
    private bool originalPoseStored;
    private Coroutine moveRoutine;

    void Awake()
    {
        if (moveTarget == null)
            moveTarget = transform;

        if (targetCamera == null)
            targetCamera = Camera.main;

        Collider col = GetComponent<Collider>();

        if (col != null)
            col.isTrigger = false;
    }

    void Start()
    {
        StoreOriginalPose();
    }

    public void ToggleFocus()
    {
        if (isMoving)
            return;

        if (blockWhileCameraIntro &&
            cameraIntroMover != null &&
            cameraIntroMover.IsMoving)
            return;

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
        if (isFocused || isMoving)
            return;

        if (!originalPoseStored)
            StoreOriginalPose();

        if (!TryGetFocusPose(
            out Vector3 targetPosition,
            out Quaternion targetRotation))
        {
            return;
        }

        float distance =
            Vector3.Distance(
                moveTarget.position,
                targetPosition
            );

        if (maxAllowedMoveDistance > 0f &&
            distance > maxAllowedMoveDistance)
        {
            Debug.LogWarning(
                $"{name}: 移動距離が大きすぎるため中止しました。" +
                $" 距離={distance:F2}",
                this
            );
            return;
        }

        isFocused = true;

        if (ResultCursorController.Instance != null)
        {
            ResultCursorController.Instance
                .RegisterFocusedBoard(this);
        }

        StartMove(targetPosition, targetRotation);
    }

    public void ReturnToOriginal()
    {
        if (!isFocused || isMoving)
            return;

        isFocused = false;

        if (ResultCursorController.Instance != null)
        {
            ResultCursorController.Instance
                .UnregisterFocusedBoard(this);
        }

        StartMove(
            originalWorldPosition,
            originalWorldRotation
        );
    }

    public void SaveCurrentPositionAsOriginal()
    {
        if (isFocused || isMoving)
            return;

        StoreOriginalPose();
    }

    bool TryGetFocusPose(
        out Vector3 targetPosition,
        out Quaternion targetRotation)
    {
        switch (targetMode)
        {
            case FocusTargetMode.InspectorWorldPosition:
                targetPosition = focusedWorldPosition;
                targetRotation =
                    Quaternion.Euler(
                        focusedWorldEulerAngles
                    );
                return true;

            case FocusTargetMode.CameraRelative:
            {
                Camera cam =
                    targetCamera != null
                    ? targetCamera
                    : Camera.main;

                if (cam == null)
                {
                    Debug.LogWarning(
                        $"{name}: Cameraが設定されていません。",
                        this
                    );

                    targetPosition = Vector3.zero;
                    targetRotation = Quaternion.identity;
                    return false;
                }

                Transform cameraTransform = cam.transform;

                targetPosition =
                    cameraTransform.position +
                    cameraTransform.right *
                    cameraRelativePosition.x +
                    cameraTransform.up *
                    cameraRelativePosition.y +
                    cameraTransform.forward *
                    cameraRelativePosition.z;

                targetRotation =
                    cameraTransform.rotation *
                    Quaternion.Euler(
                        cameraRelativeEulerAngles
                    );

                return true;
            }

            case FocusTargetMode.FocusPoint:
                if (focusPoint == null)
                {
                    Debug.LogWarning(
                        $"{name}: Focus Pointが未設定です。",
                        this
                    );

                    targetPosition = Vector3.zero;
                    targetRotation = Quaternion.identity;
                    return false;
                }

                targetPosition = focusPoint.position;
                targetRotation = focusPoint.rotation;
                return true;
        }

        targetPosition = Vector3.zero;
        targetRotation = Quaternion.identity;
        return false;
    }

    void StoreOriginalPose()
    {
        if (moveTarget == null)
            return;

        originalWorldPosition = moveTarget.position;
        originalWorldRotation = moveTarget.rotation;
        originalPoseStored = true;
    }

    void StartMove(
        Vector3 targetPosition,
        Quaternion targetRotation)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine =
            StartCoroutine(
                MoveRoutine(
                    targetPosition,
                    targetRotation
                )
            );
    }

    IEnumerator MoveRoutine(
        Vector3 targetPosition,
        Quaternion targetRotation)
    {
        isMoving = true;

        Vector3 startPosition =
            moveTarget.position;

        Quaternion startRotation =
            moveTarget.rotation;

        if (moveDuration <= 0f)
        {
            moveTarget.SetPositionAndRotation(
                targetPosition,
                targetRotation
            );

            isMoving = false;
            moveRoutine = null;
            yield break;
        }

        float timer = 0f;

        while (timer < moveDuration)
        {
            float deltaTime =
                useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            timer += deltaTime;

            float normalizedTime =
                Mathf.Clamp01(
                    timer / moveDuration
                );

            float easedTime =
                moveCurve != null
                ? moveCurve.Evaluate(normalizedTime)
                : normalizedTime;

            Vector3 nextPosition =
                Vector3.LerpUnclamped(
                    startPosition,
                    targetPosition,
                    easedTime
                );

            Quaternion nextRotation =
                Quaternion.SlerpUnclamped(
                    startRotation,
                    targetRotation,
                    easedTime
                );

            moveTarget.SetPositionAndRotation(
                nextPosition,
                nextRotation
            );

            yield return null;
        }

        moveTarget.SetPositionAndRotation(
            targetPosition,
            targetRotation
        );

        isMoving = false;
        moveRoutine = null;
    }
}
