//必要重量を満たしている間だけドアを開け、外れたら閉じる
using UnityEngine;

public class PressurePlatePuzzle_DoorOnly : MonoBehaviour
{
    [Header("対象")]
    public RoomPuzzleTarget puzzleTarget;

    [Header("必要重量")]
    public float requiredWeight = 10f;

    [Header("現在重量")]
    public float currentWeight = 0f;

    [Header("状態")]
    public bool isPressed = false;

    [Header("デバッグ")]
    public bool debugLog = true;

    public void SetCurrentWeight(float weight)
    {
        currentWeight = weight;
        UpdatePressedState();
    }

    public void AddWeight(float weight)
    {
        currentWeight += weight;
        UpdatePressedState();
    }

    public void RemoveWeight(float weight)
    {
        currentWeight -= weight;

        if (currentWeight < 0f)
            currentWeight = 0f;

        UpdatePressedState();
    }

    void UpdatePressedState()
    {
        bool nextPressed = currentWeight >= requiredWeight;

        if (isPressed == nextPressed)
            return;

        isPressed = nextPressed;

        if (debugLog)
        {
            Debug.Log(
                name
                + " 重量スイッチ: "
                + (isPressed ? "ON" : "OFF")
                + " / "
                + currentWeight
                + " / "
                + requiredWeight
            );
        }

        if (puzzleTarget != null)
        {
            puzzleTarget.SetDoorOpen(isPressed);
        }
    }
}
