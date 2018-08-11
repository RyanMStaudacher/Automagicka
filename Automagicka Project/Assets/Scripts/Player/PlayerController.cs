using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("The camera attached to the player game object")]
    [SerializeField] private GameObject playerCamera;

    [Tooltip("How quickly the player moves along the ground")]
    [SerializeField] private float movementSpeed = 10f;

    [Tooltip("The jump speed of the player")]
    [SerializeField] private float jumpSpeed = 20f;

    [Tooltip("How fast the player can look around")]
    [SerializeField] private float sensitivity = 20f;

    [Tooltip("How fast the player can look around with the joystick")]
    [SerializeField] private float joystickSensitivity = 20f;

    [Tooltip("How much gravity affects the player")]
    [SerializeField] private float gravityMultiplier = 5f;


    private CollisionFlags collisionFlags;
    private CharacterController characterController;
    private Vector3 moveDir = Vector3.zero;
    private float stickToGroundForce = 9.807f;
    private float movementInputX;
    private float movementInputY;
    private float lookInputX;
    private float lookInputY;
    private float verticalRotation;
    private bool isMoving = false;
    private bool hasJumped = false;
    private bool isFalling = false;
    private bool isLooking = false;

	// Use this for initialization
	private void Start ()
    {
        characterController = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	private void Update ()
    {
        GetPlayerInput();
	}

    private void FixedUpdate()
    {
        PlayerMovement();
    }

    private void GetPlayerInput()
    {
        if(Input.GetAxisRaw("Movement X") > 0f || Input.GetAxisRaw("Movement X") < 0f
            || Input.GetAxisRaw("Movement Y") > 0f || Input.GetAxisRaw("Movement Y") < 0f)
        {
            movementInputX = Input.GetAxis("Movement X");
            movementInputY = Input.GetAxis("Movement Y");
            isMoving = true;
        }
        else if(Input.GetAxisRaw("Left Joystick X") > 0f || Input.GetAxisRaw("Left Joystick X") < 0f 
            || Input.GetAxisRaw("Left Joystick Y") > 0f || Input.GetAxisRaw("Left Joystick Y") < 0f)
        {
            movementInputX = Input.GetAxis("Left Joystick X");
            movementInputY = Input.GetAxis("Left Joystick Y");
            isMoving = true;
        }
        else
        {
            movementInputX = 0f;
            movementInputY = 0f;
            isMoving = false;
        }

        if(Input.GetAxisRaw("Mouse X") > 0f || Input.GetAxisRaw("Mouse X") < 0f 
            || Input.GetAxisRaw("Mouse Y") > 0f || Input.GetAxisRaw("Mouse Y") < 0f)
        {
            lookInputX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            lookInputY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
            isLooking = true;
        }
        else if(Input.GetAxisRaw("Right Joystick X") > 0f || Input.GetAxisRaw("Right Joystick X") < 0f 
            || Input.GetAxisRaw("Right Joystick Y") > 0f || Input.GetAxisRaw("Right Joystick Y") < 0f)
        {
            lookInputX = Input.GetAxis("Right Joystick X") * joystickSensitivity * Time.deltaTime;
            lookInputY = Input.GetAxis("Right Joystick Y") * joystickSensitivity * Time.deltaTime;
            isLooking = true;
        }
        else
        {
            isLooking = false;
        }
    }

    private void PlayerMovement()
    {
        if (isLooking)
        {
            this.transform.Rotate(0f, lookInputX, 0f);
            verticalRotation += lookInputY;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        Vector3 desiredMove = new Vector3(movementInputX, 0f, movementInputY);
        desiredMove = Camera.main.transform.TransformDirection(desiredMove);
        desiredMove.y = 0f;

        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo,
                               characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);

        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        moveDir.x = desiredMove.x * movementSpeed;
        moveDir.z = desiredMove.z * movementSpeed;

        if (characterController.isGrounded)
        {
            moveDir.y = -stickToGroundForce;
            hasJumped = false;

            if (Input.GetButtonDown("Jump"))
            {
                moveDir.y = jumpSpeed;
                hasJumped = true;
            }
        }
        else
        {
            moveDir += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
        }

        if (!characterController.isGrounded && hasJumped == false)
        {
            isFalling = true;
        }
        else
        {
            isFalling = false;
        }

        collisionFlags = characterController.Move(moveDir * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (collisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(characterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }
}
