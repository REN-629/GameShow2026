using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerWeightProxy : MonoBehaviour
{
    public float weight = 70f;

    public float GetWeight()
    {
        return weight;
    }
}
