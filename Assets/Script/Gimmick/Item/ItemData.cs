// アイテムデータ：名前と属性を保持するデータ構造
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemAttribute
{
    public string id;
}

[System.Serializable]
public class ItemData
{
    public string itemName;
    public List<ItemAttribute> attributes = new List<ItemAttribute>();
}
