using UnityEngine;

public class CarryObjectHoldPointSet : MonoBehaviour
{
    public Transform defaultHoldPoint;
    public Transform oneHandHoldPoint;
    public Transform twoHandHoldPoint;
    public Transform heavyHoldPoint;

    public Transform GetHoldPoint(CarryObjectHoldType holdType)
    {
        switch (holdType)
        {
            case CarryObjectHoldType.OneHand:
                return oneHandHoldPoint != null ? oneHandHoldPoint : defaultHoldPoint;

            case CarryObjectHoldType.TwoHand:
                return twoHandHoldPoint != null ? twoHandHoldPoint : defaultHoldPoint;

            case CarryObjectHoldType.Heavy:
                return heavyHoldPoint != null ? heavyHoldPoint : defaultHoldPoint;
        }

        return defaultHoldPoint;
    }
}
