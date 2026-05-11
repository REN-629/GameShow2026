// HoldPointManager：HoldTypeに応じて使うHoldPointを返す
//
// Main Cameraの子にHoldPointを作る例：
// Main Camera
// ├ HoldPoint_OneHand
// ├ HoldPoint_TwoHand
// ├ HoldPoint_Heavy
// └ HoldPoint_Inspect

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
