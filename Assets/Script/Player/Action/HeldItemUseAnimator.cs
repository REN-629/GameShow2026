// 使用時演出：アイテム側のUse設定に従って位置・回転オフセットを一時的に加える
using System.Collections;
using UnityEngine;

public class HeldItemUseAnimator : MonoBehaviour
{
    public Vector3 CurrentPositionOffset { get; private set; }
    public Vector3 CurrentRotationOffset { get; private set; }

    private Coroutine currentCoroutine;

    public void PlayUseAnimation(HoldPoseData pose)
    {
        if (pose == null)
            return;

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(UseAnimation(pose));
    }

    IEnumerator UseAnimation(HoldPoseData pose)
    {
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
        currentCoroutine = null;
    }
}
