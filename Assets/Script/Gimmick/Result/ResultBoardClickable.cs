using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ResultBoardClickable : MonoBehaviour
{
    [Header("必須設定")]

    [Tooltip("移動させる看板全体のルートを指定してください。")]
    public Transform moveTarget;

    [Tooltip("看板を持ってくる位置の空オブジェクト。看板の子にはしないでください。")]
    public Transform focusPoint;

    [Header("移動")]
    [Min(0f)]
    public float moveDuration = 0.35f;

    public AnimationCurve moveCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public bool useUnscaledTime = true;
    public bool clickAgainToReturn = true;

    [Header("状態")]
    public bool isFocused;
    public bool isMoving;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Coroutine moveRoutine;

    void Awake()
    {
        if (moveTarget == null)
            moveTarget = transform;

        Collider col = GetComponent<Collider>();

        if (col != null)
            col.isTrigger = false;
    }

    void Start()
    {
        originalPosition = moveTarget.position;
        originalRotation = moveTarget.rotation;

        ValidateReferences();
    }

    void ValidateReferences()
    {
        if (focusPoint == null)
        {
            Debug.LogError(
                $"{name}: Focus Pointが設定されていません。",
                this
            );
            return;
        }

        if (focusPoint == moveTarget ||
            focusPoint.IsChildOf(moveTarget))
        {
            Debug.LogError(
                $"{name}: Focus Pointを看板の子にしてはいけません。" +
                " Main Cameraかシーン直下に置いてください。",
                this
            );
        }
    }

    public void ToggleFocus()
    {
        if (isMoving)
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

        if (!CanUseFocusPoint())
            return;

        // 移動開始時点のワールド座標を固定。
        // 移動中にFocusPointが動いても追従しない。
        Vector3 targetPosition = focusPoint.position;
        Quaternion targetRotation = focusPoint.rotation;

        isFocused = true;

        if (ResultCursorController.Instance != null)
        {
            ResultCursorController.Instance
                .RegisterFocusedBoard(this);
        }

        StartMovement(targetPosition, targetRotation);
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

        StartMovement(originalPosition, originalRotation);
    }

    bool CanUseFocusPoint()
    {
        if (moveTarget == null)
        {
            Debug.LogError(
                $"{name}: Move Targetがありません。",
                this
            );
            return false;
        }

        if (focusPoint == null)
        {
            Debug.LogError(
                $"{name}: Focus Pointがありません。",
                this
            );
            return false;
        }

        if (focusPoint == moveTarget ||
            focusPoint.IsChildOf(moveTarget))
        {
            Debug.LogError(
                $"{name}: Focus PointがMove Targetの子になっています。" +
                " 看板とは無関係な場所へ移動してください。",
                this
            );
            return false;
        }

        return true;
    }

    void StartMovement(
        Vector3 targetPosition,
        Quaternion targetRotation)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(
            MoveRoutine(targetPosition, targetRotation)
        );
    }

    IEnumerator MoveRoutine(
        Vector3 targetPosition,
        Quaternion targetRotation)
    {
        isMoving = true;

        Vector3 startPosition = moveTarget.position;
        Quaternion startRotation = moveTarget.rotation;

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

            float t = Mathf.Clamp01(
                timer / moveDuration
            );

            float eased =
                moveCurve != null
                ? moveCurve.Evaluate(t)
                : t;

            moveTarget.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                eased
            );

            moveTarget.rotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                eased
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
