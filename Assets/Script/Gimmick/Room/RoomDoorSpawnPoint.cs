using UnityEngine;

public class RoomDoorSpawnPoint : MonoBehaviour
{
    [Header("このスポーンポイントで扉を生成する")]
    public bool spawnDoor = true;

    [Header("接続済みなのでドア生成禁止")]
    public bool blockedByConnection = false;

    [Header("生成済み扉")]
    public DoorController spawnedDoor;

    public bool CanSpawnDoor()
    {
        if (!spawnDoor)
            return false;

        if (blockedByConnection)
            return false;

        return true;
    }

    public void BlockDoorByConnection()
    {
        blockedByConnection = true;
        spawnDoor = false;

        if (spawnedDoor != null)
        {
            Destroy(spawnedDoor.gameObject);
            spawnedDoor = null;
        }
    }
}
