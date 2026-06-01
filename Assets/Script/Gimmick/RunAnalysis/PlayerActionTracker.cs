using UnityEngine;

public class PlayerActionTracker : MonoBehaviour
{
    [Header("参照")]
    public Transform player;

    [Header("ダッシュ判定")]
    public KeyCode dashKey = KeyCode.LeftShift;

    [Header("ジャンプ判定")]
    public KeyCode jumpKey = KeyCode.Space;

    void Start()
    {
        if (player == null)
            player = transform;
    }

    void Update()
    {
        if (RunActionLogger.Instance == null)
            return;

        RunActionLogger.Instance.TrackPlayerPosition(player);

        if (Input.GetKey(dashKey))
        {
            RunActionLogger.Instance.AddDashTime(Time.deltaTime);
        }

        if (Input.GetKeyDown(jumpKey))
        {
            RunActionLogger.Instance.LogJump();
        }
    }
}
