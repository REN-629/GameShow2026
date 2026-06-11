using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestDoorStateDisplay : MonoBehaviour
{
    public RoomPuzzleState room;
    public TextMeshProUGUI textTMP;
    public Text text;

    void Update()
    {
        if (room == null)
            return;

        string value = room.doorOpenCondition ? "DOOR OPEN" : "DOOR CLOSED";

        if (textTMP != null)
            textTMP.text = value;

        if (text != null)
            text.text = value;
    }
}
