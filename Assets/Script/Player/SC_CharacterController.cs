using UnityEngine;
// プレイヤーコントローラー：移動、ダッシュ、ジャンプ、視点、アニメーション連携
[RequireComponent(typeof(CharacterController))]
public class SC_CharacterController : MonoBehaviour
{
    public float speed = 7.5f;
    public float dashSpeed = 12.0f;
    public KeyCode dashKey = KeyCode.LeftShift;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    public Animator animator;
    public HeldItemController heldItemController;
    private CharacterController characterController;
    [HideInInspector]
    public Vector3 moveDirection = Vector3.zero;
    private Vector2 rotation = Vector2.zero;
    [HideInInspector]
    public bool canMove = true;
    [HideInInspector]
    public bool isDashing = false;
    private float inputX;
    private float inputZ;
    // アニメ用の安定化接地判定
    private bool stableGrounded;
    private float groundedRememberTime = 0.1f;
    private float groundedTimer = 0f;
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rotation.y = transform.eulerAngles.y;
    }
    void Update()
    {
        HandleMovement();
        HandleLook();
        HandleAnimation();
    }
    void HandleMovement()
    {
        bool rawGrounded = characterController.isGrounded;
        inputZ = canMove ? Input.GetAxisRaw("Vertical") : 0f;
        inputX = canMove ? Input.GetAxisRaw("Horizontal") : 0f;
        if (Mathf.Abs(inputZ) < 0.1f) inputZ = 0f;
        if (Mathf.Abs(inputX) < 0.1f) inputX = 0f;
        if (rawGrounded)
        {
            groundedTimer = groundedRememberTime;
            stableGrounded = true;
        }
        else
        {
            groundedTimer -= Time.deltaTime;
            if (groundedTimer <= 0f)
            {
                stableGrounded = false;
            }
        }
        if (rawGrounded)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            bool hasMovementInput = Mathf.Abs(inputZ) > 0.1f || Mathf.Abs(inputX) > 0.1f;
            isDashing = canMove && hasMovementInput && Input.GetKey(dashKey);
            float currentSpeed = isDashing ? dashSpeed : speed;
            moveDirection = (forward * inputZ + right * inputX) * currentSpeed;
            // 接地中は少し下向きにして接地を安定させる
            moveDirection.y = -2f;
            if (Input.GetButtonDown("Jump") && canMove)
            {
                moveDirection.y = jumpSpeed;
                if (animator != null)
                {
                    animator.SetTrigger("Jump");
                }
                stableGrounded = false;
                groundedTimer = 0f;
            }
        }
        else
        {
            isDashing = false;
            moveDirection.y -= gravity * Time.deltaTime;
        }
        characterController.Move(moveDirection * Time.deltaTime);
    }
    void HandleLook()
    {
        bool isRotatingItem = heldItemController != null && heldItemController.IsRotatingItem;
        if (canMove && !isRotatingItem)
        {
            rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
            rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
            transform.eulerAngles = new Vector3(0, rotation.y, 0);
        }
    }
    void HandleAnimation()
    {
        if (animator == null)
            return;
        float moveMultiplier = isDashing ? 2f : 1f;
        float animX = inputX * moveMultiplier;
        float animZ = inputZ * moveMultiplier;
        animator.SetFloat("MoveX", animX, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveZ", animZ, 0.1f, Time.deltaTime);
        animator.SetBool("IsGrounded", stableGrounded);
    }
}