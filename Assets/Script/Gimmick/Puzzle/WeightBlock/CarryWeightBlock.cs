using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class CarryWeightBlock : MonoBehaviour
{
    [Header("重量")]
    public float weight = 50f;

    [Header("持ち運び")]
    public bool canCarry = true;
    public bool canRotateWhileHeld = true;
    public float rotateSpeed = 4f;

    [Header("置く/投げる")]
    public float dropForwardOffset = 1f;
    public float dropUpOffset = 0.2f;
    public float releaseForce = 2f;

    [Header("状態")]
    public bool isHeld = false;

    private Rigidbody rb;
    private Collider[] colliders;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>(true);
    }

    public float GetWeight()
    {
        return weight;
    }

    public void Hold(Transform holdPoint)
    {
        if (!canCarry || holdPoint == null)
            return;

        isHeld = true;

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        SetTriggerColliders(true);
        ForceHoldTransform(holdPoint);
    }

    public void ForceHoldTransform(Transform holdPoint)
    {
        if (!isHeld || holdPoint == null)
            return;

        transform.position = holdPoint.position;
        transform.rotation = holdPoint.rotation;
    }

    public void AddRotation(Vector3 rotation)
    {
        if (!isHeld || !canRotateWhileHeld)
            return;

        transform.Rotate(rotation, Space.Self);
    }

    public void Release(Vector3 position, Quaternion rotation, Vector3 force)
    {
        isHeld = false;

        transform.position = position;
        transform.rotation = rotation;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        SetTriggerColliders(false);

        rb.AddForce(force, ForceMode.VelocityChange);
    }

    void SetTriggerColliders(bool isTrigger)
    {
        if (colliders == null)
            return;

        foreach (Collider col in colliders)
        {
            if (col != null)
                col.isTrigger = isTrigger;
        }
    }
}
