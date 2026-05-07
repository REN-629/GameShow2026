//•às‚âƒWƒƒƒ“ƒv‚É‡‚í‚¹‚ÄŽ‚Á‚Ä‚éItem‚Æ‚©‚ð­‚µ—h‚ç‚·
using UnityEngine;

public class HoldPointSway : MonoBehaviour
{
    public SC_CharacterController playerController;

    [Header("•às—h‚ê")]
    public float walkAmount = 0.03f;
    public float walkSpeed = 8f;

    [Header("ƒ_ƒbƒVƒ…—h‚ê")]
    public float dashAmount = 0.06f;
    public float dashSpeed = 12f;

    [Header("ƒWƒƒƒ“ƒv/—Ž‰º—h‚ê")]
    public float verticalAmount = 0.04f;

    [Header("’x‚ê")]
    public float smooth = 8f;

    private Vector3 defaultLocalPosition;
    private Quaternion defaultLocalRotation;

    private Vector3 targetPosition;
    private Vector3 currentPosition;

    private Vector3 targetRotation;
    private Vector3 currentRotation;

    private float timer;

    void Start()
    {
        defaultLocalPosition = transform.localPosition;
        defaultLocalRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        if (playerController == null)
            return;

        Vector3 move = playerController.moveDirection;

        bool isMoving =
          Mathf.Abs(move.x) > 0.1f ||
          Mathf.Abs(move.z) > 0.1f;

        float amount = playerController.isDashing ? dashAmount : walkAmount;
        float speed = playerController.isDashing ? dashSpeed : walkSpeed;

        if (isMoving)
        {
            timer += Time.deltaTime * speed;

            float x = Mathf.Sin(timer) * amount;
            float y = Mathf.Abs(Mathf.Cos(timer)) * amount;

            targetPosition = new Vector3(x, y, 0f);
            targetRotation = new Vector3(y * 30f, x * 20f, -x * 30f);
        }
        else
        {
            timer = 0f;
            targetPosition = Vector3.zero;
            targetRotation = Vector3.zero;
        }

        float vertical = Mathf.Clamp(-move.y * 0.01f, -verticalAmount, verticalAmount);
        targetPosition.y += vertical;

        currentPosition = Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime * smooth);
        currentRotation = Vector3.Lerp(currentRotation, targetRotation, Time.deltaTime * smooth);

        transform.localPosition = defaultLocalPosition + currentPosition;
        transform.localRotation = defaultLocalRotation * Quaternion.Euler(currentRotation);
    }
}