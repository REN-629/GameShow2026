// 耐久が0になった時の挙動
public enum DurabilityBreakMode
{
    DestroyObject,   // オブジェクト自体を破壊して消す
    DisableUseOnly,  // 使用機能だけ停止する。ライターや懐中電灯向け
    KeepAsMaterial   // 物としては残す。素材・投擲物として使える
}
