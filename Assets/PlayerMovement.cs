using UnityEngine;

public class PlayerCameraRotation : MonoBehaviour
{
    [Header("Camera Settings")]
    private float x;
    private float y;
    public float sensitivity = -1f;
    private Vector3 rotate;

    [Header("Movement Settings")]
    public float speed = 6.0f; // Ground movement speed
    public float airSpeedMultiplier = 0.8f; // 20% slower in air (80% of ground speed)
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float friction = 10.0f; // Friction strength when grounded

    [Header("Wall Detection")]
    public bool isTouchingWall; // Is the player touching a wall?
    public Vector3 wallNormal; // Normal of the wall being touched

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleCameraRotation();
        HandleMovement();
    }

    void HandleCameraRotation()
    {
        y = Input.GetAxis("Mouse X");
        x = Input.GetAxis("Mouse Y");
        rotate = new Vector3(x, y * sensitivity, 0);
        transform.eulerAngles -= rotate;
    }

    void HandleMovement()
    {
        // Calculate movement speed based on whether the player is grounded
        float currentSpeed = controller.isGrounded ? speed : speed * airSpeedMultiplier;

        // Horizontal movement (always applied, even in air)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 horizontalMovement = new Vector3(moveX, 0, moveZ);
        horizontalMovement = transform.TransformDirection(horizontalMovement); // Move relative to player's rotation
        horizontalMovement *= currentSpeed;

        // Apply horizontal movement
        moveDirection.x = horizontalMovement.x;
        moveDirection.z = horizontalMovement.z;

        // Apply friction when grounded
        if (controller.isGrounded)
        {
            // Reduce horizontal velocity over time (simulate friction)
            if (Mathf.Abs(moveX) < 0.1f && Mathf.Abs(moveZ) < 0.1f) // Only apply friction if not actively moving
            {
                moveDirection.x = Mathf.Lerp(moveDirection.x, 0, friction * Time.deltaTime);
                moveDirection.z = Mathf.Lerp(moveDirection.z, 0, friction * Time.deltaTime);
            }

            // Reset vertical velocity when grounded
            if (moveDirection.y < 0)
            {
                moveDirection.y = -2f; // Small force to keep player grounded
            }

            // Jumping
            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }
        }

        // Apply gravity
        moveDirection.y -= gravity * Time.deltaTime;

        // Move the player
        controller.Move(moveDirection * Time.deltaTime);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check if the collision is with a wall
        if (hit.normal.y < 0.1f) // If the normal is mostly horizontal
        {
            isTouchingWall = true;
            wallNormal = hit.normal; // Store the wall normal
        }
        else
        {
            isTouchingWall = false;
        }
    }
}