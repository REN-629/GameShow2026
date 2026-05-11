// FlashlightUseAction：懐中電灯専用の使用処理
// 左クリックでON/OFF。
// ON中だけSpotLightなどのライト用GameObjectを有効にする。

using UnityEngine;

public class FlashlightUseAction : ToggleUseAction
{
    [Header("ライト")]
    public GameObject[] lightObjects;

    protected override void OnTurnOn()
    {
        SetLightVisible(true);
    }

    protected override void OnTurnOff()
    {
        SetLightVisible(false);
    }

    void SetLightVisible(bool visible)
    {
        if (lightObjects == null)
            return;

        foreach (GameObject obj in lightObjects)
        {
            if (obj != null)
                obj.SetActive(visible);
        }
    }

    void OnDisable()
    {
        SetLightVisible(false);
    }
}
