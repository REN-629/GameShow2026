// HoldPoint管理：持ち方タイプごとに使うHoldPointを返す
using UnityEngine;

public class HoldPointManager : MonoBehaviour
{
    public Transform oneHandHoldPoint;
    public Transform twoHandHoldPoint;
    public Transform heavyHoldPoint;
    public Transform inspectHoldPoint;

    public Transform GetHoldPoint(HoldType holdType)
    {
        switch (holdType)
        {
            case HoldType.TwoHand:
                return twoHandHoldPoint != null ? twoHandHoldPoint : oneHandHoldPoint;

            case HoldType.Heavy:
                if (heavyHoldPoint != null) return heavyHoldPoint;
                if (twoHandHoldPoint != null) return twoHandHoldPoint;
                return oneHandHoldPoint;

            case HoldType.Inspect:
                return inspectHoldPoint != null ? inspectHoldPoint : oneHandHoldPoint;

            case HoldType.OneHand:
            default:
                return oneHandHoldPoint;
        }
    }
}
