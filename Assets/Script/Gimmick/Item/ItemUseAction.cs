// アイテム使用処理の基底クラス
// バールなら BluntUseAction、ライターなら FireUseAction などを作って増やす
using UnityEngine;

public abstract class ItemUseAction : MonoBehaviour
{
    public abstract void Use(PickupItem item);
}
