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
    public float moveDuration = 2f;

    [Header("カーブ")]
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("時間")]
    public bool useUnscaledTime = false;

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
        Transform cam = targetCamera.transform;

        Quaternion startRot = Quaternion.Euler(startEulerAngles);
        Quaternion targetRot = Quaternion.Euler(targetEulerAngles);

        float timer = 0f;

        while (timer < moveDuration)
        {
            float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            timer += delta;

            float t = moveDuration > 0f ? Mathf.Clamp01(timer / moveDuration) : 1f;
            float e = moveCurve != null ? moveCurve.Evaluate(t) : t;

            cam.position = Vector3.LerpUnclamped(startPosition, targetPosition, e);
            cam.rotation = Quaternion.SlerpUnclamped(startRot, targetRot, e);

            yield return null;
        }

        cam.position = targetPosition;
        cam.rotation = targetRot;
    }
}
