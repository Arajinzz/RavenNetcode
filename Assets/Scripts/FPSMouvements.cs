using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSMouvements : MonoBehaviour
{

    [SerializeField]
    float Speed = 12f;

    [SerializeField]
    float Gravity = -9.81f;

    [SerializeField]
    float JumpHeight = 3f;

    [SerializeField]
    float GroundDistance = 0.4f;

    [SerializeField]
    GameObject PlayerCamera;

    [SerializeField]
    Transform PlayerBody;

    public Transform GroundCheck;
    public LayerMask GroundMask;

    private CharacterController Controller;
    private InputManager inputManager;
    public InputManager.Key keyPressed;

    private Vector3 Velocity;
    private bool bGrounded;

    // Start is called before the first frame update
    void Start()
    {
        Controller = GetComponent<CharacterController>();
        inputManager = GetComponent<InputManager>();

        if (inputManager)
        {
            PlayerCamera.SetActive(true);
        }
    }

    private void FixedUpdate()
    {
        bGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistance, GroundMask);

        if (bGrounded && Velocity.y < 0)
        {
            Velocity.y = -2f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //HandleInput();
        HandleMouvement();
        Jump();

        // Falling
        Velocity.y += Gravity * Time.deltaTime;
        Controller.Move(Velocity * Time.deltaTime);
    }

    private void Jump()
    {
        if (InputManager.CompareKey(keyPressed, InputManager.Key.SPACE) && bGrounded)
        {
            Velocity.y = Mathf.Sqrt(JumpHeight * -2f * Gravity);
        }
    }

    public void HandleMouvement()
    {
        float x = 0;
        float z = 0;

        if (InputManager.CompareKey(keyPressed, InputManager.Key.W))
        {
            z += 1;
        }

        if (InputManager.CompareKey(keyPressed, InputManager.Key.S))
        {
            z += -1;
        }

        if (InputManager.CompareKey(keyPressed, InputManager.Key.A))
        {
            x += -1;
        }

        if (InputManager.CompareKey(keyPressed, InputManager.Key.D))
        {
            x += 1;
        }

        Vector3 move = transform.right * x + transform.forward * z;
        Controller.Move(move * Speed * Time.deltaTime);
    }

    public void RotatePlayer(float mouseX)
    {
        PlayerBody.Rotate(Vector3.up * mouseX);
    }

    void HandleInput()
    {
        if (!inputManager)
            return;

        //if (inputManager.isKeyPressed(InputManager.Key.W) ||
        //    inputManager.isKeyPressed(InputManager.Key.S) ||
        //    inputManager.isKeyPressed(InputManager.Key.A) ||
        //    inputManager.isKeyPressed(InputManager.Key.D))
        //{
        //    HandleMouvement();
        //}

        //if (inputManager.isKeyPressed(InputManager.Key.SPACE))
        //{
        //    Jump();
        //}
    }

    public void SetKeyPressed(InputManager.Key keyPressed)
    {
        this.keyPressed = keyPressed;
    }
}
