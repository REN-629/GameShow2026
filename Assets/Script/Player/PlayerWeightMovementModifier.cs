// PlayerWeightMovementModifier：Inventoryの合計重量でプレイヤーの移動速度を下げる
//
// 付ける場所：Player本体
//
// 必要な参照：
// Player Controller → SC_CharacterController
// Inventory → ItemManagerのInventory
//
// 例：
// 重量0   → speed 100%
// 重量50  → speed 50%
// 重量100 → speed 0%

using UnityEngine;

public class PlayerWeightMovementModifier : MonoBehaviour
{
    [Header("参照")]
    public SC_CharacterController playerController;
    public Inventory inventory;

    [Header("元の速度")]
    public float baseSpeed = 7.5f;
    public float baseDashSpeed = 12f;

    [Header("デバッグ")]
    public bool debugLog = false;

    void Start()
    {
        if (playerController == null)
            playerController = GetComponent<SC_CharacterController>();

        if (playerController != null)
        {
            baseSpeed = playerController.speed;
            baseDashSpeed = playerController.dashSpeed;
        }
    }

    void Update()
    {
        if (playerController == null || inventory == null)
            return;

        float multiplier = inventory.GetMoveSpeedMultiplier();

        playerController.speed = baseSpeed * multiplier;
        playerController.dashSpeed = baseDashSpeed * multiplier;

        if (debugLog)
        {
            Debug.Log(
                "重量: "
                + inventory.GetCurrentWeight()
                + " / "
                + inventory.maxWeight
                + " 速度倍率: "
                + multiplier
            );
        }
    }
}
