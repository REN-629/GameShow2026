using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class CarryObject : MonoBehaviour
{
    public CarryObjectType objectType = CarryObjectType.Generic;
    public float weight = 50f;

    public bool canStoreInInventory = false;
    public bool canCarry = true;
    public CarryObjectHoldType holdType = CarryObjectHoldType.Default;

    public Vector3 heldPositionOffset;
    public Vector3 heldRotationOffset;

    public bool useCustomHoldPoint = false;
    public Transform customHoldPoint;

    public bool canRotateWhileHeld = true;
    public float rotateSpeed = 4f;

    public bool canRelease = true;
    public float releaseForwardOffset = 1f;
    public float releaseUpOffset = 0.2f;
    public float releaseForce = 2f;

    public bool canThrow = true;
    public float throwForceMultiplier = 1f;

    public bool makeKinematicWhileHeld = true;
    public bool disableGravityWhileHeld = true;
    public bool makeCollidersTriggerWhileHeld = true;

    public bool isHeld = false;

    Rigidbody rb;
    Collider[] colliders;

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
        if (!canCarry)
            return;

        Transform targetHoldPoint = GetActualHoldPoint(holdPoint);

        if (targetHoldPoint == null)
            return;

        isHeld = true;

        if (rb != null)
        {
            if (makeKinematicWhileHeld)
                rb.isKinematic = true;

            if (disableGravityWhileHeld)
                rb.useGravity = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (makeCollidersTriggerWhileHeld)
            SetCollidersTrigger(true);

        ForceHoldTransform(targetHoldPoint);
    }

    public void ForceHoldTransform(Transform holdPoint)
    {
        if (!isHeld)
            return;

        Transform targetHoldPoint = GetActualHoldPoint(holdPoint);

        if (targetHoldPoint == null)
            return;

        transform.position =
            targetHoldPoint.position +
            targetHoldPoint.TransformDirection(heldPositionOffset);

        transform.rotation =
            targetHoldPoint.rotation *
            Quaternion.Euler(heldRotationOffset);
    }

    Transform GetActualHoldPoint(Transform fallback)
    {
        if (useCustomHoldPoint && customHoldPoint != null)
            return customHoldPoint;

        return fallback;
    }

    public void AddRotation(Vector3 rotation)
    {
        if (!isHeld || !canRotateWhileHeld)
            return;

        transform.Rotate(rotation, Space.Self);
    }

    public void Release(Vector3 position, Quaternion rotation, Vector3 force)
    {
        if (!canRelease)
            return;

        isHeld = false;

        transform.position = position;
        transform.rotation = rotation;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (makeCollidersTriggerWhileHeld)
            SetCollidersTrigger(false);

        if (rb != null)
            rb.AddForce(force, ForceMode.VelocityChange);
    }

    public void ReleaseFrom(Transform origin)
    {
        Vector3 forward = transform.forward;
        Quaternion rotation = transform.rotation;
        Vector3 position = transform.position;

        if (origin != null)
        {
            forward = origin.forward;
            rotation = origin.rotation;
            position =
                origin.position +
                forward.normalized * releaseForwardOffset +
                Vector3.up * releaseUpOffset;
        }

        Release(position, rotation, forward.normalized * releaseForce);
    }

    void SetCollidersTrigger(bool value)
    {
        if (colliders == null)
            return;

        foreach (Collider col in colliders)
        {
            if (col != null)
                col.isTrigger = value;
        }
    }
}
