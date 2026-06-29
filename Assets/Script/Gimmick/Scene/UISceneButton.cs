using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UISceneButton : MonoBehaviour
{
    [Header("遷移先")]
    public string sceneName;

    [Header("遅延")]
    public float delay = 0f;

    [Header("連打防止")]
    public bool disableAfterClick = true;

    [Header("クリック時に表示するObject")]
    public GameObject[] enableOnClick;

    [Header("クリック時に非表示にするObject")]
    public GameObject[] disableOnClick;

    [Header("クリック時に無効化する操作/処理")]
    public MonoBehaviour[] disableBehavioursOnClick;

    [Header("タイマー停止")]
    public bool stopRoomRunTimer = true;

    [Header("デバッグ")]
    public bool debugLog = true;

    private bool clicked;

    public void LoadScene()
    {
        if (disableAfterClick && clicked)
            return;

        clicked = true;

        ApplyObjects();
        DisableBehaviours();

        if (stopRoomRunTimer && RoomRunTimer.RunInstance != null)
            RoomRunTimer.RunInstance.StopTimer();

        if (debugLog)
            Debug.Log(name + " UIシーン遷移: " + sceneName);

        StartCoroutine(LoadSceneRoutine());
    }

    public void LoadScene(string targetSceneName)
    {
        sceneName = targetSceneName;
        LoadScene();
    }

    public void ReloadCurrentScene()
    {
        sceneName = SceneManager.GetActiveScene().name;
        LoadScene();
    }

    public void QuitGame()
    {
        if (disableAfterClick && clicked)
            return;

        clicked = true;

        if (debugLog)
            Debug.Log(name + " ゲーム終了");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator LoadSceneRoutine()
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning(name + " sceneName が未設定");
            yield break;
        }

        SceneManager.LoadScene(sceneName);
    }

    void ApplyObjects()
    {
        if (enableOnClick != null)
        {
            foreach (GameObject obj in enableOnClick)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }

        if (disableOnClick != null)
        {
            foreach (GameObject obj in disableOnClick)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
    }

    void DisableBehaviours()
    {
        if (disableBehavioursOnClick == null)
            return;

        foreach (MonoBehaviour behaviour in disableBehavioursOnClick)
        {
            if (behaviour != null)
                behaviour.enabled = false;
        }
    }
}
