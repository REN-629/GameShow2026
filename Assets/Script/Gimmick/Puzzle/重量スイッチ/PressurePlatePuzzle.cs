//重量スイッチ
using System.Collections.Generic;
using UnityEngine;

public class PressurePlatePuzzle : MonoBehaviour
{
    [Header("必要重量")]
    public float requiredWeight = 50f;

    [Header("プレイヤー重量")]
    public float playerWeight = 70f;

    [Header("押し込み演出")]
    public Transform plateModel;

    public Vector3 pressedOffset =
        new Vector3(0f, -0.05f, 0f);

    public float pressSmooth = 10f;

    [Header("一度だけクリア")]
    public bool triggerOnlyOnce = true;

    [Header("デバッグ")]
    public bool debugLog = true;

    private bool hasTriggered = false;

    private Vector3 defaultLocalPosition;

    // Trigger内の物体
    private readonly List<Rigidbody> bodies =
        new List<Rigidbody>();

    private bool playerInside = false;

    void Start()
    {
        if (plateModel == null)
            plateModel = transform;

        defaultLocalPosition =
            plateModel.localPosition;
    }

    void Update()
    {
        float currentWeight = CalculateWeight();

        bool isPressed =
            currentWeight >= requiredWeight;

        UpdatePlateVisual(isPressed);

        if (isPressed)
        {
            TriggerPuzzle();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            if (debugLog)
                Debug.Log("プレイヤーが乗った");
        }

        Rigidbody rb = other.attachedRigidbody;

        if (rb != null && !bodies.Contains(rb))
        {
            bodies.Add(rb);

            if (debugLog)
            {
                Debug.Log(
                    "重量追加: "
                    + rb.name
                );
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            if (debugLog)
                Debug.Log("プレイヤーが降りた");
        }

        Rigidbody rb = other.attachedRigidbody;

        if (rb != null && bodies.Contains(rb))
        {
            bodies.Remove(rb);

            if (debugLog)
            {
                Debug.Log(
                    "重量削除: "
                    + rb.name
                );
            }
        }
    }

    float CalculateWeight()
    {
        float total = 0f;

        // プレイヤー
        if (playerInside)
        {
            total += playerWeight;
        }

        // Rigidbody
        for (int i = bodies.Count - 1; i >= 0; i--)
        {
            Rigidbody rb = bodies[i];

            if (rb == null)
            {
                bodies.RemoveAt(i);
                continue;
            }

            total += rb.mass;
        }

        return total;
    }

    void TriggerPuzzle()
    {
        if (triggerOnlyOnce && hasTriggered)
            return;

        hasTriggered = true;

        if (debugLog)
        {
            Debug.Log(
                "重量条件達成 → パズルクリア"
            );
        }

        if (RoomRuntimeManager.Instance != null)
        {
            RoomRuntimeManager.Instance
                .ClearCurrentRoomPuzzle();
        }
        else
        {
            Debug.LogWarning(
                "RoomRuntimeManager がありません"
            );
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