// ToggleUseAction：オン/オフできるアイテム用の親クラス
// ライター・懐中電灯・ランタンなどに使う。
// 左クリックでON/OFFを切り替え、ON中だけエネルギーを消費する。

using UnityEngine;

public abstract class ToggleUseAction : ItemUseAction
{
    [Header("ON/OFF状態")]
    [SerializeField] protected bool isOn = false;

    [Header("エネルギー")]
    public float energy = 100f;
    public float maxEnergy = 100f;
    public float energyConsumePerSecond = 1f;
    public bool turnOffWhenEnergyEmpty = true;

    [Header("ON/OFF時SE")]
    public AudioClip[] toggleOnSEClips;
    public AudioClip[] toggleOffSEClips;

    [Range(0f, 1f)]
    public float toggleSEVolume = 1f;

    public bool IsOn => isOn;

    protected virtual void Start()
    {
        SetToggleState(false);
    }

    void Update()
    {
        if (!isOn)
            return;

        energy -= energyConsumePerSecond * Time.deltaTime;

        if (energy < 0f)
            energy = 0f;

        if (turnOffWhenEnergyEmpty && energy <= 0f)
        {
            SetToggleState(false);
            return;
        }

        OnToggleUpdate();
    }

    public override bool Use(PickupItem item)
    {
        if (!isOn && energy <= 0f)
        {
            Debug.Log(name + " はエネルギー切れ");
            return false;
        }

        SetToggleState(!isOn);

        // Toggle系はクリックしただけでは道具耐久を減らさない
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

    protected abstract void OnTurnOn();
    protected abstract void OnTurnOff();

    protected virtual void OnToggleUpdate()
    {
    }

    public void AddEnergy(float amount)
    {
        energy += amount;

        if (energy > maxEnergy)
            energy = maxEnergy;
    }
}
