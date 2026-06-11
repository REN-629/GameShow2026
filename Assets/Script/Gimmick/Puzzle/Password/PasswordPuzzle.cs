using UnityEngine;

public class PasswordPuzzle : MonoBehaviour
{
    [Header("ドア開閉ターゲット")]
    public RoomPuzzleTarget puzzleTarget;

    [Header("パスワード設定")]
    public int digitCount = 4;
    public bool randomizeOnStart = true;
    public string password = "0000";

    [Header("端末破壊")]
    public bool canBreakTerminal = true;
    public int terminalHP = 3;

    [Header("状態")]
    public bool solved = false;
    public bool broken = false;

    [Header("デバッグ")]
    public bool debugLog = true;

    void Start()
    {
        if (randomizeOnStart)
            GeneratePassword();

        if (debugLog)
            Debug.Log(name + " Password = " + password);
    }

    public void GeneratePassword()
    {
        digitCount = Mathf.Clamp(digitCount, 1, 8);
        password = "";

        for (int i = 0; i < digitCount; i++)
            password += Random.Range(0, 10).ToString();
    }

    public bool TrySubmit(string input)
    {
        if (solved)
            return true;

        if (string.IsNullOrEmpty(input))
            return false;

        if (input == password)
        {
            Solve(PuzzleSolveMethod.Normal);
            return true;
        }

        if (debugLog)
            Debug.Log(name + " パスワード不一致: " + input);

        return false;
    }

    public void ApplyDamage(int damage)
    {
        if (!canBreakTerminal)
            return;

        if (solved || broken)
            return;

        terminalHP -= Mathf.Max(1, damage);

        if (debugLog)
            Debug.Log(name + " 端末ダメージ: HP=" + terminalHP);

        if (terminalHP <= 0)
            BreakTerminal();
    }

    public void BreakTerminal()
    {
        if (solved || broken)
            return;

        broken = true;
        Solve(PuzzleSolveMethod.Break);
    }

    void Solve(PuzzleSolveMethod method)
    {
        solved = true;

        if (puzzleTarget == null)
            puzzleTarget = GetComponent<RoomPuzzleTarget>();

        if (puzzleTarget != null)
        {
            puzzleTarget.solveMethod = method;
            puzzleTarget.SetDoorOpen(true);
        }
        else
        {
            Debug.LogWarning(name + " RoomPuzzleTarget がありません");
        }

        if (debugLog)
            Debug.Log(name + " パスワードパズル解除: " + method);
    }

    public string GetPassword()
    {
        return password;
    }
}
