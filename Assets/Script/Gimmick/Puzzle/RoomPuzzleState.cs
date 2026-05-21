//¡‚¢‚é•”‰®‚ÌƒpƒYƒ‹‚Ìó‘Ô
using UnityEngine;

public class RoomPuzzleState : MonoBehaviour
{
    [Header("‚±‚Ì•”‰®‚Ì”à")]
    public DoorController[] roomDoors;

    private bool cleared = false;

    public void ClearPuzzle()
    {
        if (cleared)
            return;

        cleared = true;

        foreach (DoorController door in roomDoors)
        {
            if (door != null)
            {
                door.Open();
            }
        }
    }
}