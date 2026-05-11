// DurabilityBreakMode：耐久が0になった時にどうするか
// DestroyObject  = オブジェクトを消す。木箱や壊れる壁向け
// DisableUseOnly = 使用機能だけ止める。ライターや懐中電灯向け
// KeepAsMaterial = 壊れても物体として残す。素材として使いたい物向け
public enum DurabilityBreakMode
{
    DestroyObject,
    DisableUseOnly,
    KeepAsMaterial
}
