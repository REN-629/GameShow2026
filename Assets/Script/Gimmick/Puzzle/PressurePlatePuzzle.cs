// PressurePlatePuzzle.cs
//
// 重量式ボタン / 重量スイッチ
//
// 修正版:
// ・重量条件達成時にマテリアル変更
// ・重量不足で元のマテリアルへ戻す
// ・重りを置いている間だけドアを開く
//
// 推奨構成:
//
// PressurePlate
// ├ PlateModel
// │  ├ MeshRenderer
// │  └ BoxCollider               ← Is Trigger OFF / 上に乗る用
// └ WeightTrigger
//    ├ BoxCollider               ← Is Trigger ON / 判定用
//    ├ RoomPuzzleTarget
//    └ PressurePlatePuzzle       ← このスクリプト
//
// PressurePlatePuzzle の:
// Plate Model → PlateModel
// Button Renderer → PlateModel の MeshRenderer

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PressurePlatePuzzle : MonoBehaviour
{
    [Header("必要重量")]
    public float requiredWeight = 50f;

    [Header("プレイヤー重量")]
    public float playerWeight = 70f;

    [Header("押し込み演出")]
    public Transform plateModel;
    public Vector3 pressedOffset = new Vector3(0f, -0.05f, 0f);
    public float pressSmooth = 10f;

    [Header("見た目：マテリアル変更")]
    [Tooltip("ボタンのRenderer")]
    public Renderer buttonRenderer;

    [Tooltip("通常時Material")]
    public Material normalMaterial;

    [Tooltip("押された時Material")]
    public Material clearedMaterial;

    [Header("クリア解除設定")]
    [Tooltip("ONなら、重量が足りなくなった時にパズル未クリアへ戻す")]
    public bool resetWhenWeightRemoved = true;

    [Tooltip("ONなら、必要重量を満たしている間だけクリア扱い")]
    public bool holdToKeepOpen = true;

    [Header("重量計算")]
    public bool useInventoryWeightData = true;
    public bool fallbackToRigidbodyMass = true;

    [Header("デバッグ")]
    public bool debugLog = true;
    public bool showWeightEveryFrame = false;

    private bool isPressed = false;
    private Vector3 defaultLocalPosition;

    private readonly List<GameObject> objectsOnPlate = new List<GameObject>();

    void Start()
    {
        if (plateModel == null)
            plateModel = transform;

        defaultLocalPosition = plateModel.localPosition;

        // 初期マテリアル
        if (buttonRenderer != null && normalMaterial != null)
        {
            buttonRenderer.material = normalMaterial;
        }
    }

    void Update()
    {
        float currentWeight = CalculateWeight();

        if (showWeightEveryFrame)
        {
            Debug.Log(
                name
                + " 現在重量: "
                + currentWeight
                + " / 必要重量: "
                + requiredWeight
            );
        }

        bool shouldBePressed = currentWeight >= requiredWeight;

        UpdatePlateVisual(shouldBePressed);

        if (shouldBePressed != isPressed)
        {
            isPressed = shouldBePressed;
            ApplyPuzzleState(isPressed);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject target = GetWeightRoot(other);

        if (target == null)
            return;

        if (!objectsOnPlate.Contains(target))
        {
            objectsOnPlate.Add(target);

            if (debugLog)
            {
                Debug.Log(
                    "重量対象追加: "
                    + target.name
                    + " / weight="
                    + GetObjectWeight(target)
                );
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        GameObject target = GetWeightRoot(other);

        if (target == null)
            return;

        if (objectsOnPlate.Contains(target))
        {
            objectsOnPlate.Remove(target);

            if (debugLog)
                Debug.Log("重量対象削除: " + target.name);
        }
    }

    GameObject GetWeightRoot(Collider other)
    {
        if (other == null)
            return null;

        if (other.CompareTag("Player"))
            return other.gameObject;

        PickupItem pickupItem = other.GetComponentInParent<PickupItem>();

        if (pickupItem != null)
            return pickupItem.gameObject;

        Rigidbody rb = other.attachedRigidbody;

        if (rb != null)
            return rb.gameObject;

        return other.gameObject;
    }

    float CalculateWeight()
    {
        float total = 0f;

        for (int i = objectsOnPlate.Count - 1; i >= 0; i--)
        {
            GameObject obj = objectsOnPlate[i];

            if (obj == null)
            {
                objectsOnPlate.RemoveAt(i);
                continue;
            }

            total += GetObjectWeight(obj);
        }

        return total;
    }

    float GetObjectWeight(GameObject obj)
    {
        if (obj == null)
            return 0f;

        if (obj.CompareTag("Player"))
            return playerWeight;

        if (useInventoryWeightData)
        {
            float weightFromComponent = TryGetWeightFromComponents(obj);

            if (weightFromComponent >= 0f)
                return weightFromComponent;
        }

        if (fallbackToRigidbodyMass)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();

            if (rb != null)
                return rb.mass;
        }

        return 0f;
    }

    float TryGetWeightFromComponents(GameObject obj)
    {
        Component[] components = obj.GetComponents<Component>();

        foreach (Component component in components)
        {
            if (component == null)
                continue;

            float value;

            if (TryGetFloatMember(component, "weight", out value))
                return value;

            if (TryGetFloatMember(component, "itemWeight", out value))
                return value;

            if (TryGetFloatMember(component, "weightValue", out value))
                return value;

            if (TryGetFloatMember(component, "currentWeight", out value))
                return value;
        }

        return -1f;
    }

    bool TryGetFloatMember(Component component, string memberName, out float value)
    {
        value = 0f;

        System.Type type = component.GetType();

        FieldInfo field = type.GetField(
            memberName,
            BindingFlags.Public | BindingFlags.Instance
        );

        if (field != null)
        {
            object fieldValue = field.GetValue(component);

            if (fieldValue is float)
            {
                value = (float)fieldValue;
                return true;
            }

            if (fieldValue is int)
            {
                value = (int)fieldValue;
                return true;
            }
        }

        PropertyInfo property = type.GetProperty(
            memberName,
            BindingFlags.Public | BindingFlags.Instance
        );

        if (property != null)
        {
            object propertyValue = property.GetValue(component, null);

            if (propertyValue is float)
            {
                value = (float)propertyValue;
                return true;
            }

            if (propertyValue is int)
            {
                value = (int)propertyValue;
                return true;
            }
        }

        return false;
    }

    void ApplyPuzzleState(bool pressed)
    {
        // マテリアル更新
        UpdateButtonMaterial(pressed);

        RoomPuzzleTarget target = GetComponent<RoomPuzzleTarget>();

        if (pressed)
        {
            if (debugLog)
                Debug.Log("重量条件達成 → パズルクリア");

            if (target != null)
                target.ClearTargetRoom();
            else if (RoomRuntimeManager.Instance != null)
                RoomRuntimeManager.Instance.ClearCurrentRoomPuzzle();

            return;
        }

        if (!resetWhenWeightRemoved && !holdToKeepOpen)
            return;

        if (debugLog)
            Debug.Log("重量不足 → パズル未クリアへ戻す");

        if (target != null)
        {
            target.ResetTargetRoom();
            return;
        }

        if (RoomRuntimeManager.Instance != null && RoomRuntimeManager.Instance.currentRoom != null)
        {
            RoomRuntimeManager.Instance.currentRoom.ResetPuzzle();
        }
    }

    void UpdateButtonMaterial(bool cleared)
    {
        if (buttonRenderer == null)
            return;

        if (cleared)
        {
            if (clearedMaterial != null)
                buttonRenderer.material = clearedMaterial;
        }
        else
        {
            if (normalMaterial != null)
                buttonRenderer.material = normalMaterial;
        }
    }

    void UpdatePlateVisual(bool pressed)
    {
        if (plateModel == null)
            return;

        Vector3 targetPos =
            pressed
            ? defaultLocalPosition + pressedOffset
            : defaultLocalPosition;

        plateModel.localPosition =
            Vector3.Lerp(
                plateModel.localPosition,
                targetPos,
                Time.deltaTime * pressSmooth
            );
    }
}
