//オンオフできるエネルギー系アイテム用の親クラス

using UnityEngine;

public abstract class ToggleUseAction : ItemUseAction
{
    [Header("ON/OFF状態")]
    [SerializeField] protected bool isOn = false;

    [Header("エネルギー耐久値：残り使用秒数")]
    public float maxUseSeconds = 120f;
    public float remainingUseSeconds = 120f;
    public float consumeSecondsPerSecond = 1f;

    [Header("エネルギー切れ設定")]
    public bool turnOffWhenEmpty = true;
    public bool cannotTurnOnWhenEmpty = true;

    [Header("ON/OFF時SE")]
    public AudioClip[] toggleOnSEClips;
    public AudioClip[] toggleOffSEClips;
    public AudioClip[] emptySEClips;

    [Range(0f, 1f)]
    public float toggleSEVolume = 1f;

    public bool IsOn => isOn;

    protected virtual void Start()
    {
        remainingUseSeconds = Mathf.Clamp(remainingUseSeconds, 0f, maxUseSeconds);

        if (isOn && IsEnergyEmpty())
        {
            isOn = false;
        }

        if (isOn)
            OnTurnOn();
        else
            OnTurnOff();
    }

    void Update()
    {
        if (!isOn)
            return;

        remainingUseSeconds -= consumeSecondsPerSecond * Time.deltaTime;

        if (remainingUseSeconds < 0f)
            remainingUseSeconds = 0f;

        if (turnOffWhenEmpty && IsEnergyEmpty())
        {
            SetToggleState(false);
            return;
        }

        OnToggleUpdate();
    }

    public override bool Use(PickupItem item)
    {
        if (!isOn && cannotTurnOnWhenEmpty && IsEnergyEmpty())
        {
            RandomAudioPlayer.PlayRandom(emptySEClips, transform.position, toggleSEVolume);
            Debug.Log(name + " はエネルギー切れで使えない");
            return false;
        }

        SetToggleState(!isOn);

        // 重要：
        // falseを返すことで、左クリックON/OFF時に物理耐久値を減らさない。
        return false;
    }

    protected void SetToggleState(bool newState)
    {
        if (isOn == newState)
            return;

        isOn = newState;

        if (isOn)
        {
            RandomAudioPlayer.PlayRandom(toggleOnSEClips, transform.position, toggleSEVolume);
            OnTurnOn();
        }
        else
        {
            RandomAudioPlayer.PlayRandom(toggleOffSEClips, transform.position, toggleSEVolume);
            OnTurnOff();
        }
    }

    public void ForceTurnOff()
    {
        SetToggleState(false);
    }

    public void AddEnergySeconds(float seconds)
    {
        remainingUseSeconds += seconds;

        if (remainingUseSeconds > maxUseSeconds)
            remainingUseSeconds = maxUseSeconds;
    }

    public void SetEnergySeconds(float seconds)
    {
        remainingUseSeconds = Mathf.Clamp(seconds, 0f, maxUseSeconds);

        if (isOn && IsEnergyEmpty())
            SetToggleState(false);
    }

    public bool IsEnergyEmpty()
    {
        return remainingUseSeconds <= 0f;
    }

    public float GetEnergyRate()
    {
        if (maxUseSeconds <= 0f)
            return 0f;

        return Mathf.Clamp01(remainingUseSeconds / maxUseSeconds);
    }

    public int GetRemainingSecondsRounded()
    {
        return Mathf.CeilToInt(remainingUseSeconds);
    }

    protected abstract void OnTurnOn();
    protected abstract void OnTurnOff();

    protected virtual void OnToggleUpdate()
    {
    }
}
