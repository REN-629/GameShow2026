// HoldType：アイテムをどの持ち位置で表示するかを決める分類
// 例：
// OneHand = ライターや鍵などの片手持ち
// TwoHand = バールや棒などの両手持ち
// Heavy   = 箱などの重い物
// Inspect = 写真やメモなど、近くで見る物
public enum HoldType
{
    OneHand,
    TwoHand,
    Heavy,
    Inspect
}
