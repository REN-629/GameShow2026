using UnityEngine;
//ˆÚ“®‚É‡‚í‚¹‚ÄƒJƒƒ‰‚ð—h‚ç‚·


public class SC_HeadBobber : MonoBehaviour
{
    public float walkingBobbingSpeed = 14f;
    public float walkingBobbingAmount = 0.05f;

    public float dashBobbingSpeed = 20f;
    public float dashBobbingAmount = 0.09f;

    public SC_CharacterController controller;

    private float defaultPosY = 0f;
    private float timer = 0f;

    void Start()
    {
        defaultPosY = transform.localPosition.y;
    }

    void Update()
    {
        bool isMoving =
          Mathf.Abs(controller.moveDirection.x) > 0.1f ||
          Mathf.Abs(controller.moveDirection.z) > 0.1f;

        if (isMoving)
        {
            float currentBobbingSpeed = controller.isDashing ? dashBobbingSpeed : walkingBobbingSpeed;
            float currentBobbingAmount = controller.isDashing ? dashBobbingAmount : walkingBobbingAmount;

            timer += Time.deltaTime * currentBobbingSpeed;

            transform.localPosition = new Vector3(
              transform.localPosition.x,
              defaultPosY + Mathf.Sin(timer) * currentBobbingAmount,
              transform.localPosition.z
            );
        }
        else
        {
            timer = 0f;

            transform.localPosition = new Vector3(
              transform.localPosition.x,
              Mathf.Lerp(transform.localPosition.y, defaultPosY, Time.deltaTime * walkingBobbingSpeed),
              transform.localPosition.z
            );
        }
    }
}

