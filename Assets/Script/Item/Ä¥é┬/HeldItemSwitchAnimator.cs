using System;
using UnityEngine;

public class HeldItemSwitchAnimator : MonoBehaviour
{
    [Header("切り替え時間")]
    public float hideDuration = 0.12f;
    public float showDuration = 0.16f;

    [Header("下げる位置")]
    public Vector3 hiddenPositionOffset = new Vector3(0f, -1.0f, -0.25f);

    [Header("下げる回転")]
    public Vector3 hiddenRotationOffset = new Vector3(25f, 0f, 0f);

    [Header("イージング")]
    public AnimationCurve hideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve showCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public Vector3 CurrentPositionOffset { get; private set; }
    public Vector3 CurrentRotationOffset { get; private set; }

    public bool IsSwitching { get; private set; }

    private Action onMiddleCallback;
    private Coroutine routine;

    public void StartSwitch(Action onMiddle)
    {
        if (routine != null)
            StopCoroutine(routine);

        onMiddleCallback = onMiddle;
        routine = StartCoroutine(SwitchRoutine());
    }

    public void ForceStop()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = null;
        onMiddleCallback = null;
        IsSwitching = false;

        CurrentPositionOffset = Vector3.zero;
        CurrentRotationOffset = Vector3.zero;
    }

    System.Collections.IEnumerator SwitchRoutine()
    {
        IsSwitching = true;

        yield return Animate(
            Vector3.zero,
            hiddenPositionOffset,
            Vector3.zero,
            hiddenRotationOffset,
            hideDuration,
            hideCurve
        );

        onMiddleCallback?.Invoke();

        yield return Animate(
            hiddenPositionOffset,
            Vector3.zero,
            hiddenRotationOffset,
            Vector3.zero,
            showDuration,
            showCurve
        );

        CurrentPositionOffset = Vector3.zero;
        CurrentRotationOffset = Vector3.zero;

        IsSwitching = false;
        routine = null;
        onMiddleCallback = null;
    }

    System.Collections.IEnumerator Animate(
        Vector3 fromPos,
        Vector3 toPos,
        Vector3 fromRot,
        Vector3 toRot,
        float duration,
        AnimationCurve curve
    )
    {
        if (duration <= 0f)
        {
            CurrentPositionOffset = toPos;
            CurrentRotationOffset = toRot;
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);
            float e = curve != null ? curve.Evaluate(t) : t;

            CurrentPositionOffset = Vector3.LerpUnclamped(fromPos, toPos, e);
            CurrentRotationOffset = Vector3.LerpUnclamped(fromRot, toRot, e);

            yield return null;
        }

        CurrentPositionOffset = toPos;
        CurrentRotationOffset = toRot;
    }
}
