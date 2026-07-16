using System.Collections;
using UnityEngine;

public class ResultCameraIntroMover : MonoBehaviour
{
    [Header("対象カメラ")]
    public Camera targetCamera;

    [Header("開始位置を現在位置にする")]
    public bool useCurrentAsStart = true;

    [Header("開始位置")]
    public Vector3 startPosition;
    public Vector3 startEulerAngles;

    [Header("移動先")]
    public Vector3 targetPosition = new Vector3(0f, 2f, -6f);
    public Vector3 targetEulerAngles = new Vector3(10f, 0f, 0f);

    [Header("移動時間")]
    [Min(0f)]
    public float moveDuration = 2f;

    [Header("カーブ")]
    public AnimationCurve moveCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("時間")]
    public bool useUnscaledTime;

    public bool IsMoving { get; private set; }

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        if (useCurrentAsStart)
        {
            startPosition = targetCamera.transform.position;
            startEulerAngles = targetCamera.transform.eulerAngles;
        }

        StartCoroutine(MoveRoutine());
    }

    IEnumerator MoveRoutine()
    {
        IsMoving = true;

        Transform cameraTransform = targetCamera.transform;
        Quaternion startRotation =
            Quaternion.Euler(startEulerAngles);
        Quaternion targetRotation =
            Quaternion.Euler(targetEulerAngles);

        if (moveDuration <= 0f)
        {
            cameraTransform.SetPositionAndRotation(
                targetPosition,
                targetRotation
            );

            IsMoving = false;
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
                Mathf.Clamp01(timer / moveDuration);

            float easedTime =
                moveCurve != null
                ? moveCurve.Evaluate(normalizedTime)
                : normalizedTime;

            cameraTransform.position =
                Vector3.LerpUnclamped(
                    startPosition,
                    targetPosition,
                    easedTime
                );

            cameraTransform.rotation =
                Quaternion.SlerpUnclamped(
                    startRotation,
                    targetRotation,
                    easedTime
                );

            yield return null;
        }

        cameraTransform.SetPositionAndRotation(
            targetPosition,
            targetRotation
        );

        IsMoving = false;
    }
}
