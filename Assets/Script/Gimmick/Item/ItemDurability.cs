// アイテム耐久：アイテム自身の耐久値
using UnityEngine;

public class ItemDurability : MonoBehaviour
{
    [Header("耐久値")]
    public int durability = 10;

    public void Damage(int amount)
    {
        durability -= amount;

        Debug.Log(name + " の耐久が " + amount + " 減少");

        if (durability <= 0)
        {
            Break();
        }
    }

    void Break()
    {
        Debug.Log(name + " が壊れた");
        Destroy(gameObject);
    }
}
