using UnityEngine;
//プレイヤーコントローラー

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
    private CharacterController characterController;
    [HideInInspector]
    public Vector3 moveDirection = Vector3.zero;
    private Vector2 rotation = Vector2.zero;
    [HideInInspector]
    public bool canMove = true;

    //ダッシュ中かどうかを他のスクリプトから参照できるようにする
    [HideInInspector]
    public bool isDashing = false;
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rotation.y = transform.eulerAngles.y;
    }
    void Update()
    {
        if (characterController.isGrounded)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            float inputZ = canMove ? Input.GetAxis("Vertical") : 0f;
            float inputX = canMove ? Input.GetAxis("Horizontal") : 0f;
            bool hasMovementInput = Mathf.Abs(inputZ) > 0.1f || Mathf.Abs(inputX) > 0.1f;
            isDashing = canMove && hasMovementInput && Input.GetKey(dashKey);
            float currentSpeed = isDashing ? dashSpeed : speed;
            moveDirection = (forward * inputZ + right * inputX) * currentSpeed;
            if (Input.GetButton("Jump") && canMove)
            {
                moveDirection.y = jumpSpeed;
            }
        }
        else
        {

            //空中ではダッシュ判定を切る
            isDashing = false;
        }
        moveDirection.y -= gravity * Time.deltaTime;
        characterController.Move(moveDirection * Time.deltaTime);
        if (canMove)
        {
            rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
            rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotation.x, 0, 0);
            transform.eulerAngles = new Vector3(0, rotation.y, 0);
        }
    }
}