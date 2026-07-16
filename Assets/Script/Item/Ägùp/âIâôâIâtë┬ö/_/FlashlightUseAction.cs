// FlashlightUseAction：懐中電灯専用
//
// 物理耐久：AttributeDurability
// エネルギー耐久：ToggleUseAction の remainingUseSeconds
//
// ON中だけSpotLightなどを有効化する。
// 投げてもON状態は維持される。

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
}
