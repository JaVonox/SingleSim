using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private const float moveSpeed = 5f;
    public GameObject playerChar;
    public CharacterController playerMovement;
    public float sensitivity = 0.5f;
    float rotateX = 0f;
    private const float gravity = -9.81f; 
    public Vector3 vel; //Velocity of gravity
    private const float gDistance = 0.4f;
    public LayerMask groundMask;
    bool isGrounded;

    public GameObject groundCollider; //Checks for ground below the player

    public GameObject camera;
    public BoxCollider interactionCollider; //When intersecting with interactible objects collider, it will open the appropriate UI element
    public LayerMask interactablesMask; //Ensures interactables only collide with eachother

    public Dictionary<BoxCollider, string> interactables = new Dictionary<BoxCollider, string>(); //Colliders and their associated ids
    void Update()
    {
        MoveChar();

        //Debug.DrawRay(camera.transform.position, camera.transform.TransformDirection(Vector3.forward) * 1.5f,Color.green);

        if (Input.GetButtonDown("Fire1")) //After the player presses E (the interaction button) send a raycast to detect any colliders infront of them
        {
            RaycastHit rHit;
            if (Physics.Raycast(camera.transform.position, camera.transform.TransformDirection(Vector3.forward), out rHit, 1.5f, (1 << 9)))
            {
                Debug.Log(rHit.collider);
            }
        }
    }

    private Vector3 neutralVec = new Vector3(1, 0, 1);
    void MoveChar() //Handles char movement - Using https://www.youtube.com/watch?v=_QajrabyTJc code
    {
        isGrounded = Physics.CheckSphere(groundCollider.transform.position, gDistance, groundMask);   
        //Mouse movement
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        rotateX -= mouseY;
        rotateX = Mathf.Clamp(rotateX, -80f, 80f);

        transform.localRotation = Quaternion.Euler(rotateX, 0f, 0f);
        playerChar.transform.Rotate(Vector3.up * mouseX);

        //Player movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        move.y = 0; //Prevent upwards or downwards movement using mouse pos
        playerMovement.Move(move * moveSpeed * Time.deltaTime);

        //Gravity Calculations
        if(isGrounded && vel.y < 0)
        {
            vel.y = -2f;
        }
        vel.y += gravity * Time.deltaTime;
        playerMovement.Move(vel * Time.deltaTime);
    }

}
