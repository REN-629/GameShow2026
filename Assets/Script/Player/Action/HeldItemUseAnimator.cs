// 使用時演出：使用演出中かどうかを管理し、連続使用を防ぐ
using System.Collections;
using UnityEngine;

public class HeldItemUseAnimator : MonoBehaviour
{
    public Vector3 CurrentPositionOffset { get; private set; }
    public Vector3 CurrentRotationOffset { get; private set; }

    public bool IsPlaying { get; private set; }

    private Coroutine currentCoroutine;

    public bool PlayUseAnimation(HoldPoseData pose)
    {
        if (pose == null)
            return false;

        if (IsPlaying)
            return false;

        currentCoroutine = StartCoroutine(UseAnimation(pose));
        return true;
    }

    IEnumerator UseAnimation(HoldPoseData pose)
    {
        IsPlaying = true;

        float speed = Mathf.Max(0.01f, pose.useSpeed);

        Vector3 targetPos = pose.usePositionOffset;
        Vector3 targetRot = pose.useRotationOffset;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            CurrentPositionOffset = Vector3.Lerp(Vector3.zero, targetPos, t);
            CurrentRotationOffset = Vector3.Lerp(Vector3.zero, targetRot, t);
            yield return null;
        }

        t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            CurrentPositionOffset = Vector3.Lerp(targetPos, Vector3.zero, t);
            CurrentRotationOffset = Vector3.Lerp(targetRot, Vector3.zero, t);
            yield return null;
        }

        CurrentPositionOffset = Vector3.zero;
        CurrentRotationOffset = Vector3.zero;

        IsPlaying = false;
        currentCoroutine = null;
    }

    public void ForceStop()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        CurrentPositionOffset = Vector3.zero;
        CurrentRotationOffset = Vector3.zero;
        IsPlaying = false;
    }
}
