using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("遷移先")]
    public string nextSceneName;

    [Header("遅延")]
    public float delay = 2f;

    [Header("対象")]
    public string playerTag = "Player";

    [Header("一度だけ")]
    public bool triggerOnce = true;

    [Header("発動時に無効化する操作")]
    public MonoBehaviour[] disableOnTrigger;

    [Header("発動時に表示するObject")]
    public GameObject[] enableOnTrigger;

    [Header("発動時に非表示にするObject")]
    public GameObject[] disableObjectsOnTrigger;

    [Header("遷移前にタイマー停止")]
    public bool stopRoomRunTimer = true;

    [Header("デバッグ")]
    public bool debugLog = true;

    private bool triggered;

    void Reset()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
            col.isTrigger = true;
    }

    void Awake()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
            col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && triggered)
            return;

        if (!other.CompareTag(playerTag))
            return;

        TriggerTransition();
    }

    public void TriggerTransition()
    {
        if (triggerOnce && triggered)
            return;

        triggered = true;

        ApplyObjects();
        DisableControls();

        if (stopRoomRunTimer && RoomRunTimer.RunInstance != null)
            RoomRunTimer.RunInstance.StopTimer();

        if (debugLog)
            Debug.Log(name + " シーン遷移開始: " + nextSceneName);

        StartCoroutine(LoadSceneRoutine());
    }

    IEnumerator LoadSceneRoutine()
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning(name + " nextSceneName が未設定");
            yield break;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    void DisableControls()
    {
        if (disableOnTrigger == null)
            return;

        foreach (MonoBehaviour behaviour in disableOnTrigger)
        {
            if (behaviour != null)
                behaviour.enabled = false;
        }
    }

    void ApplyObjects()
    {
        if (enableOnTrigger != null)
        {
            foreach (GameObject obj in enableOnTrigger)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }

        if (disableObjectsOnTrigger != null)
        {
            foreach (GameObject obj in disableObjectsOnTrigger)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
    }
}
