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

    public Transform GroundCheck;
    public LayerMask GroundMask;

    private CharacterController Controller;

    private Vector3 Velocity;
    private bool bGrounded;

    // Start is called before the first frame update
    void Start()
    {
        Controller = GetComponent<CharacterController>();
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
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        Controller.Move(move * Speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && bGrounded)
        {
            Velocity.y = Mathf.Sqrt(JumpHeight * -2f * Gravity);
        }

        Velocity.y += Gravity * Time.deltaTime;
        Controller.Move(Velocity * Time.deltaTime);
    }
}
