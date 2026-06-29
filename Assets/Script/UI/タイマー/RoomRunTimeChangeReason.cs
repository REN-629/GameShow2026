using UnityEngine;

public class RoomRunTimeChangeReason : MonoBehaviour
{
    public string reason = "ダメージ";
    public float amount = -5f;

    public void Apply()
    {
        if (RoomRunTimer.RunInstance == null)
            return;

        if (amount >= 0f)
            RoomRunTimer.RunInstance.AddTime(amount, reason);
        else
            RoomRunTimer.RunInstance.RemoveTime(Mathf.Abs(amount), reason);
    }

    public void Add(float value)
    {
        if (RoomRunTimer.RunInstance != null)
            RoomRunTimer.RunInstance.AddTime(value, reason);
    }

    public void Remove(float value)
    {
        if (RoomRunTimer.RunInstance != null)
            RoomRunTimer.RunInstance.RemoveTime(value, reason);
    }
}
