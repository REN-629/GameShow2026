//選択中アイテムを手に表示する
//重要
//投げる時は ReleaseHeldItemForThrow() を使う
//投げる時に SetStoredState() を呼ばないようにするため
using UnityEngine;

public class HeldItemController : MonoBehaviour
{
    public Inventory inventory;
    public HoldPointManager holdPointManager;

    [Header("壁貫通防止")]
    public bool preventHeldItemClipping = true;
    public float clipCheckRadius = 0.2f;
    public float wallPadding = 0.05f;
    public LayerMask wallMask;


    [Header("使用時演出")]
    public HeldItemUseAnimator useAnimator;

    private PickupItem currentHeldItem;

    public bool IsRotatingItem { get; private set; }

    void Update()
    {
        UpdateHeldItem();
        HandleUse();
        HandleRotation();
    }

    void LateUpdate()
    {
        if (currentHeldItem == null || holdPointManager == null)
            return;

        Transform holdPoint = holdPointManager.GetHoldPoint(currentHeldItem.holdType);

        if (holdPoint == null)
            return;

        Vector3 usePos = Vector3.zero;
        Vector3 useRot = Vector3.zero;

        if (useAnimator != null)
        {
            usePos = useAnimator.CurrentPositionOffset;
            useRot = useAnimator.CurrentRotationOffset;
        }

        //不要
        //currentHeldItem.ForceHoldTransform(holdPoint, usePos, useRot);

        Vector3 desiredPos =
    holdPoint.position + holdPoint.TransformDirection(usePos);

        Vector3 safePos =
            GetSafeHoldPosition(desiredPos);

        Transform itemTransform =
            currentHeldItem.transform;

        Vector3 originalPos =
            itemTransform.position;

        itemTransform.position = safePos;

        currentHeldItem.ForceHoldTransform(
            holdPoint,
            usePos,
            useRot
        );

        itemTransform.position = safePos;
    }

    Vector3 GetSafeHoldPosition(Vector3 desiredPosition)
    {
        if (!preventHeldItemClipping)
            return desiredPosition;

        Camera cam = Camera.main;

        if (cam == null)
            return desiredPosition;

        Vector3 start = cam.transform.position;
        Vector3 dir = desiredPosition - start;

        float distance = dir.magnitude;

        if (distance <= 0.01f)
            return desiredPosition;

        dir.Normalize();

        if (Physics.SphereCast(
            start,
            clipCheckRadius,
            dir,
            out RaycastHit hit,
            distance,
            wallMask,
            QueryTriggerInteraction.Ignore))
        {
            return hit.point - dir * wallPadding;
        }

        return desiredPosition;
    }

    void UpdateHeldItem()
    {
        if (inventory == null || holdPointManager == null)
            return;

        PickupItem selectedItem = inventory.GetSelectedItem();

        if (currentHeldItem == selectedItem)
            return;

        //前に持っていたアイテムは収納状態へ
        //ただし投げる時は ReleaseHeldItemForThrow() で先に null にするため、ここは走らない
        if (currentHeldItem != null)
        {
            currentHeldItem.SetStoredState();
        }

        if (useAnimator != null)
        {
            useAnimator.ForceStop();
        }

        currentHeldItem = selectedItem;

        if (currentHeldItem != null)
        {
            currentHeldItem.SetHeldState();

            Transform holdPoint = holdPointManager.GetHoldPoint(currentHeldItem.holdType);

            if (holdPoint != null)
            {
                currentHeldItem.ForceHoldTransform(holdPoint);
            }
        }
    }

    void HandleUse()
    {
        if (currentHeldItem == null)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        if (!currentHeldItem.canUseWithLeftClick)
            return;

        if (useAnimator != null && useAnimator.IsPlaying)
            return;

        currentHeldItem.Use();

        if (useAnimator != null)
        {
            useAnimator.PlayUseAnimation(currentHeldItem.holdPose);
        }
    }

    void HandleRotation()
    {
        IsRotatingItem = false;

        if (currentHeldItem == null)
            return;

        if (!currentHeldItem.canRotate)
            return;

        if (useAnimator != null && useAnimator.IsPlaying)
            return;

        if (Input.GetMouseButton(1))
        {
            IsRotatingItem = true;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            currentHeldItem.AddHoldRotation(new Vector3(
                mouseY * currentHeldItem.rotateSpeed,
                -mouseX * currentHeldItem.rotateSpeed,
                0f
            ));
        }
    }

    public PickupItem GetCurrentHeldItem()
    {
        return currentHeldItem;
    }

    public void ClearHeldReference()
    {
        currentHeldItem = null;
        IsRotatingItem = false;

        if (useAnimator != null)
        {
            useAnimator.ForceStop();
        }
    }

    public void ReleaseHeldItemForThrow(PickupItem item)
    {
        if (currentHeldItem == item)
        {
            currentHeldItem = null;
        }

        IsRotatingItem = false;

        if (useAnimator != null)
        {
            useAnimator.ForceStop();
        }
    }


}
