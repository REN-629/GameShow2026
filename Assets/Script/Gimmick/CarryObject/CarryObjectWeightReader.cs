using UnityEngine;

public static class CarryObjectWeightReader
{
    public static bool TryGetWeight(GameObject obj, out float weight)
    {
        weight = 0f;

        if (obj == null)
            return false;

        CarryObject carryObject = obj.GetComponent<CarryObject>();

        if (carryObject != null)
        {
            if (carryObject.isHeld)
                return false;

            weight = Mathf.Max(0f, carryObject.GetWeight());
            return true;
        }

        return false;
    }
}
