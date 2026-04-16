// 投擲処理：G長押しでチャージし離した時にチャージ量で投げる
using UnityEngine;

public class ItemThrower : MonoBehaviour
{
    public Inventory inventory;
    public HeldItemController heldItemController;
    public Camera cam;

    public KeyCode throwKey = KeyCode.G;

    [Header("投擲設定")]
    public float minThrowForce = 4f;
    public float maxThrowForce = 15f;
    public float maxChargeTime = 2f;
    public float dropDistance = 1.2f;

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
            if (heldItemController != null && heldItemController.GetCurrentHeldItem() != null)
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
            return;

        float chargeRate = maxChargeTime > 0f ? currentCharge / maxChargeTime : 0f;
        float finalForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeRate);

        heldItemController.ClearHeldReference();
        inventory.RemoveSelectedItem();

        Vector3 spawnPos = cam.transform.position + cam.transform.forward * dropDistance;
        Quaternion spawnRot = Quaternion.identity;
        Vector3 force = cam.transform.forward * finalForce;

        item.ThrowToWorld(spawnPos, spawnRot, force);
        Debug.Log("投擲: " + item.itemData.itemName + " / force=" + finalForce);
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
