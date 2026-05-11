// ItemThrower：G長押しでチャージして、離したらアイテムを投げる
//
// 重要：
// 投げる時はInventoryから外して、HeldItemControllerの参照も外し、最後にThrowToWorld()する。
// 絶対に投げる時に SetStoredState() を呼ばない。

using UnityEngine;

public class ItemThrower : MonoBehaviour
{
    [Header("参照")]
    public Inventory inventory;
    public HeldItemController heldItemController;
    public Camera cam;

    [Header("入力")]
    public KeyCode throwKey = KeyCode.G;

    [Header("投げる力")]
    public float minThrowForce = 4f;
    public float maxThrowForce = 15f;
    public float maxChargeTime = 2f;

    [Header("投げ始める位置")]
    public float dropDistance = 2.0f;
    public float verticalOffset = -0.2f;

    private float currentCharge = 0f;
    private bool isCharging = false;

    void Update()
    {
        HandleThrowCharge();
    }

    void HandleThrowCharge()
    {
        if (Input.GetKeyDown(throwKey))
        {
            PickupItem item =
                heldItemController != null
                ? heldItemController.GetCurrentHeldItem()
                : null;

            if (item != null && item.canThrow)
            {
                isCharging = true;
                currentCharge = 0f;
            }
        }

        if (isCharging && Input.GetKey(throwKey))
        {
            currentCharge += Time.deltaTime;
            currentCharge = Mathf.Clamp(currentCharge, 0f, maxChargeTime);
        }

        if (isCharging && Input.GetKeyUp(throwKey))
        {
            ThrowSelectedItem();

            isCharging = false;
            currentCharge = 0f;
        }
    }

    void ThrowSelectedItem()
    {
        if (inventory == null || heldItemController == null || cam == null)
            return;

        PickupItem item = heldItemController.GetCurrentHeldItem();

        if (item == null)
        {
            Debug.LogWarning("投げるアイテムがありません");
            return;
        }

        if (!item.canThrow)
        {
            Debug.LogWarning(item.name + " は投げられません");
            return;
        }

        float chargeRate =
            maxChargeTime > 0f
            ? currentCharge / maxChargeTime
            : 0f;

        float finalForce =
            Mathf.Lerp(minThrowForce, maxThrowForce, chargeRate);

        Vector3 spawnPos =
            cam.transform.position
            + cam.transform.forward * dropDistance
            + cam.transform.up * verticalOffset;

        Quaternion spawnRot = cam.transform.rotation;
        Vector3 force = cam.transform.forward * finalForce;

        // 1. インベントリから外す
        inventory.RemoveSelectedItem();

        // 2. 手持ち参照を外す
        heldItemController.ReleaseHeldItemForThrow(item);

        // 3. ワールドに出して投げる
        item.ThrowToWorld(spawnPos, spawnRot, force);
    }

    public float GetChargeRate()
    {
        if (!isCharging || maxChargeTime <= 0f)
            return 0f;

        return currentCharge / maxChargeTime;
    }

    public bool IsCharging()
    {
        return isCharging;
    }
}
