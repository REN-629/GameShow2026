using UnityEngine;
using UnityEngine.UI;

public class CollectPuzzleIconUI : MonoBehaviour
{
    [Header("半透明アイコン")]
    public Image emptyImage;

    [Header("取得済みアイコン")]
    public Image collectedImage;

    public void SetCollected(bool collected)
    {
        if (emptyImage != null)
            emptyImage.enabled = true;

        if (collectedImage != null)
            collectedImage.enabled = collected;
    }
}
