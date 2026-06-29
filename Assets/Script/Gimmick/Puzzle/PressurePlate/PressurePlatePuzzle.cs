using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(Collider))]
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

    [Header("見た目")]
    public Renderer buttonRenderer;
    public Material normalMaterial;
    public Material clearedMaterial;

    [Header("重量計算")]
    public bool useInventoryWeightData = true;
    public bool fallbackToRigidbodyMass = true;
    public bool validateObjectsEveryFrame = true;
    public float boundsPadding = 0.05f;

    [Header("デバッグ")]
    public bool debugLog = true;
    public bool showWeightEveryFrame = false;

    private bool isPressed = false;
    private Vector3 defaultLocalPosition;
    private Collider triggerCollider;
    private readonly List<GameObject> objectsOnPlate = new List<GameObject>();

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }

    void Start()
    {
        if (plateModel == null)
            plateModel = transform;

        defaultLocalPosition = plateModel.localPosition;
        UpdateButtonMaterial(false);
    }

    void Update()
    {
        if (validateObjectsEveryFrame)
            ValidateObjectsOnPlate();

        float currentWeight = CalculateWeight();

        if (showWeightEveryFrame)
            Debug.Log(name + " 現在重量: " + currentWeight + " / 必要重量: " + requiredWeight);

        bool shouldBePressed = currentWeight >= requiredWeight;
        UpdatePlateVisual(shouldBePressed);

        if (shouldBePressed == isPressed)
            return;

        isPressed = shouldBePressed;
        ApplyDoorState(isPressed);
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
                Debug.Log("重量対象追加: " + target.name + " / weight=" + GetObjectWeight(target));
        }
    }

    void OnTriggerExit(Collider other)
    {
        GameObject target = GetWeightRoot(other);

        if (target == null)
            return;

        RemoveObjectFromPlate(target);
    }

    void ValidateObjectsOnPlate()
    {
        for (int i = objectsOnPlate.Count - 1; i >= 0; i--)
        {
            GameObject obj = objectsOnPlate[i];

            if (!IsValidWeightObject(obj))
            {
                RemoveObjectFromPlateAt(i, obj);
                continue;
            }

            if (!IsObjectInsideTrigger(obj))
                RemoveObjectFromPlateAt(i, obj);
        }
    }

    bool IsValidWeightObject(GameObject obj)
    {
        if (obj == null)
            return false;

        if (!obj.activeInHierarchy)
            return false;

        CarryWeightBlock block = obj.GetComponent<CarryWeightBlock>();

        if (block != null && block.isHeld)
            return false;

        PickupItem item = obj.GetComponent<PickupItem>();

        if (item != null && item.CurrentState != PickupItemState.World)
            return false;

        return true;
    }

    bool IsObjectInsideTrigger(GameObject obj)
    {
        if (triggerCollider == null)
            return true;

        Bounds triggerBounds = triggerCollider.bounds;
        triggerBounds.Expand(boundsPadding);

        Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
        bool hasEnabledCollider = false;

        foreach (Collider col in colliders)
        {
            if (col == null)
                continue;

            if (!col.enabled)
                continue;

            if (col == triggerCollider)
                continue;

            hasEnabledCollider = true;

            if (triggerBounds.Intersects(col.bounds))
                return true;
        }

        if (!hasEnabledCollider)
            return triggerBounds.Contains(obj.transform.position);

        return false;
    }

    void RemoveObjectFromPlate(GameObject target)
    {
        int index = objectsOnPlate.IndexOf(target);

        if (index >= 0)
            RemoveObjectFromPlateAt(index, target);
    }

    void RemoveObjectFromPlateAt(int index, GameObject target)
    {
        objectsOnPlate.RemoveAt(index);

        if (debugLog && target != null)
            Debug.Log("重量対象削除: " + target.name);
    }

    GameObject GetWeightRoot(Collider other)
    {
        if (other == null)
            return null;

        if (other.CompareTag("Player"))
            return other.gameObject;

        CarryWeightBlock carryBlock = other.GetComponentInParent<CarryWeightBlock>();

        if (carryBlock != null)
            return carryBlock.gameObject;

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

            if (!IsValidWeightObject(obj))
            {
                RemoveObjectFromPlateAt(i, obj);
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

        PlayerWeightProxy player =
            obj.GetComponent<PlayerWeightProxy>();

        if (player != null)
            return player.GetWeight();

        if (obj.CompareTag("Player"))
            return playerWeight;

        CarryWeightBlock block = obj.GetComponent<CarryWeightBlock>();

        if (block != null)
            return block.GetWeight();

        PressureWeightSource pressureWeight =
            obj.GetComponent<PressureWeightSource>();

        if (pressureWeight != null)
            return pressureWeight.GetWeight();

        PickupItem pickupItem = obj.GetComponent<PickupItem>();

        if (pickupItem != null)
            return Mathf.Max(0f, pickupItem.itemWeight);

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

            if (TryGetFloatMember(component, "weight", out float weight))
                return weight;

            if (TryGetFloatMember(component, "itemWeight", out float itemWeight))
                return itemWeight;

            if (TryGetFloatMember(component, "weightValue", out float weightValue))
                return weightValue;

            if (TryGetFloatMember(component, "currentWeight", out float currentWeight))
                return currentWeight;
        }

        return -1f;
    }

    bool TryGetFloatMember(Component component, string memberName, out float value)
    {
        value = 0f;
        System.Type type = component.GetType();
        FieldInfo field = type.GetField(memberName, BindingFlags.Public | BindingFlags.Instance);

        if (field != null)
        {
            object fieldValue = field.GetValue(component);

            if (fieldValue is float floatValue)
            {
                value = floatValue;
                return true;
            }

            if (fieldValue is int intValue)
            {
                value = intValue;
                return true;
            }
        }

        PropertyInfo property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);

        if (property != null)
        {
            object propertyValue = property.GetValue(component, null);

            if (propertyValue is float floatValue)
            {
                value = floatValue;
                return true;
            }

            if (propertyValue is int intValue)
            {
                value = intValue;
                return true;
            }
        }

        return false;
    }

    void ApplyDoorState(bool pressed)
    {
        UpdateButtonMaterial(pressed);

        if (debugLog)
            Debug.Log(pressed ? "重量条件達成 → ドア開放" : "重量不足 → ドア閉鎖");

        RoomPuzzleTarget target = GetComponent<RoomPuzzleTarget>();

        if (target == null)
            target = GetComponentInParent<RoomPuzzleTarget>();

        if (target == null)
            target = GetComponentInChildren<RoomPuzzleTarget>(true);

        if (target != null)
        {
            target.SetDoorOpen(pressed);
            return;
        }

        if (RoomRuntimeManager.Instance != null && RoomRuntimeManager.Instance.currentRoom != null)
            RoomRuntimeManager.Instance.currentRoom.SetDoorOpenCondition(pressed, PuzzleSolveMethod.Weight);
    }

    void UpdateButtonMaterial(bool pressed)
    {
        if (buttonRenderer == null)
            return;

        if (pressed)
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

        Vector3 targetPos = pressed ? defaultLocalPosition + pressedOffset : defaultLocalPosition;
        plateModel.localPosition = Vector3.Lerp(plateModel.localPosition, targetPos, Time.deltaTime * pressSmooth);
    }
}
