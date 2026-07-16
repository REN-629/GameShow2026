using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ResultBoardClickable : MonoBehaviour
{
    public Transform moveTarget;
    public Transform focusPoint;

    [Min(0f)]
    public float moveDuration = 0.35f;

    public AnimationCurve moveCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public bool useUnscaledTime = true;
    public bool clickAgainToReturn = true;

    [SerializeField] private bool isFocused;
    [SerializeField] private bool isMoving;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Coroutine moveRoutine;

    void Awake()
    {
        if (moveTarget == null)
            moveTarget = transform;

        isFocused = false;
        isMoving = false;

        originalPosition = moveTarget.position;
        originalRotation = moveTarget.rotation;
    }

    public void ToggleFocus()
    {
        if (isMoving)
            return;

        if (ResultCursorController.Instance != null &&
            !ResultCursorController.Instance.CanFocus(this))
        {
            return;
        }

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
        if (isFocused || isMoving || focusPoint == null)
            return;

        if (focusPoint == moveTarget ||
            focusPoint.IsChildOf(moveTarget))
        {
            return;
        }

        isFocused = true;

        if (ResultCursorController.Instance != null)
            ResultCursorController.Instance.RegisterFocusedBoard(this);

        StartMovement(focusPoint.position, focusPoint.rotation);
    }

    public void ReturnToOriginal()
    {
        if (!isFocused || isMoving)
            return;

        isFocused = false;

        if (ResultCursorController.Instance != null)
            ResultCursorController.Instance.UnregisterFocusedBoard(this);

        StartMovement(originalPosition, originalRotation);
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

            moveTarget.SetPositionAndRotation(
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    eased
                ),
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    eased
                )
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
