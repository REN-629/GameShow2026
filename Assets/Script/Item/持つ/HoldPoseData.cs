// HoldPoseData：アイテムを手に持った時の見た目位置を決めるデータ
// アイテムごとにInspectorで調整する
using UnityEngine;

[System.Serializable]
public class HoldPoseData
{
    [Header("持った時の位置補正")]
    public Vector3 positionOffset;

    [Header("持った時の回転補正")]
    public Vector3 rotationOffset;

    [Header("表示スケール")]
    public Vector3 scale = Vector3.one;

    [Header("使用時の移動")]
    public Vector3 usePositionOffset;

    [Header("使用時の回転")]
    public Vector3 useRotationOffset;

    [Header("使用演出速度")]
    public float useSpeed = 12f;
}
