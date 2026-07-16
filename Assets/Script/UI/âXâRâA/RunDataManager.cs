//シーンを跨いで結果を保持するための親オブジェクト
//DontDestroyOnLoadでリザルトシーンまで残る
using UnityEngine;

public class RunDataManager : MonoBehaviour
{
    public static RunDataManager Instance { get; private set; }

    [Header("重複したRunDataManagerを削除する")]
    public bool destroyDuplicate = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (destroyDuplicate)
            {
                Destroy(gameObject);
                return;
            }
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ResetRunData()
    {
        if (RunScoreManager.Instance != null)
            RunScoreManager.Instance.ResetRunScore();

        if (RunActionLogger.Instance != null)
            RunActionLogger.Instance.ResetLog();
    }
}
