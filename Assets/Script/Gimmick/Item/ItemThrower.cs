// 投擲処理：G長押しでチャージし離した時にチャージ量で投げる
// 修正版：投げ出し位置を少し遠めにし、ログを増やして位置確認しやすくする
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
    public float dropDistance = 2.0f;

    [Header("投げ出し位置補正")]
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
            PickupItem item = heldItemController != null ? heldItemController.GetCurrentHeldItem() : null;

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

        float chargeRate = maxChargeTime > 0f ? currentCharge / maxChargeTime : 0f;
        float finalForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeRate);

        Vector3 spawnPos =
            cam.transform.position
            + cam.transform.forward * dropDistance
            + cam.transform.up * verticalOffset;

        Quaternion spawnRot = cam.transform.rotation;
        Vector3 force = cam.transform.forward * finalForce;

        inventory.RemoveSelectedItem();
        heldItemController.ClearHeldReference();

        Debug.Log("投擲開始: " + item.name + " / spawnPos=" + spawnPos + " / force=" + force);

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
