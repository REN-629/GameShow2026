using UnityEngine;

public class WeightDoorCondition : MonoBehaviour
{
    [Header("ドア開閉ターゲット")]
    public RoomPuzzleTarget target;

    [Header("必要重量")]
    public float requiredWeight = 10f;

    [Header("現在重量")]
    public float currentWeight = 0f;

    [Header("状態")]
    public bool doorOpen = false;

    [Header("毎フレーム状態を再評価")]
    public bool updateEveryFrame = false;

    [Header("デバッグ")]
    public bool debugLog = true;

    void Update()
    {
        if (updateEveryFrame)
            Evaluate();
    }

    public void SetCurrentWeight(float weight)
    {
        currentWeight = Mathf.Max(0f, weight);
        Evaluate();
    }

    public void AddWeight(float weight)
    {
        currentWeight += Mathf.Max(0f, weight);
        Evaluate();
    }

    public void RemoveWeight(float weight)
    {
        currentWeight -= Mathf.Max(0f, weight);

        if (currentWeight < 0f)
            currentWeight = 0f;

        Evaluate();
    }

    public void Evaluate()
    {
        bool shouldOpen = currentWeight >= requiredWeight;

        if (doorOpen == shouldOpen)
            return;

        doorOpen = shouldOpen;

        if (debugLog)
        {
            Debug.Log(
                name
                + " WeightDoorCondition: "
                + (doorOpen ? "OPEN" : "CLOSE")
                + " / "
                + currentWeight
                + " / "
                + requiredWeight
            );
        }

        if (target != null)
            target.SetDoorOpen(doorOpen);
    }
}
