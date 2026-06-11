using UnityEngine;

public class PasswordTerminalDamageReceiver : MonoBehaviour
{
    [Header("対象パズル")]
    public PasswordPuzzle passwordPuzzle;

    [Header("一回のダメージ量")]
    public int damagePerHit = 1;

    [Header("デバッグ")]
    public bool debugLog = true;

    public void ApplyDamage()
    {
        ApplyDamage(damagePerHit);
    }

    public void ApplyDamage(int damage)
    {
        PasswordPuzzle target = passwordPuzzle;

        if (target == null)
            target = GetComponentInParent<PasswordPuzzle>();

        if (target == null)
        {
            Debug.LogWarning(name + " PasswordPuzzle がありません");
            return;
        }

        target.ApplyDamage(damage);

        if (debugLog)
            Debug.Log(name + " 端末破壊ダメージ: " + damage);
    }
}
