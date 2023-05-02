using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Movement : MonoBehaviour
{
    // Start is called before the first frame update

    public bool playerMovementLocked = false;
    public bool isInMenu = false;
    public GameObject loadedUIElement;

    private const float moveSpeed = 5f;
    public GameObject playerChar;
    public CharacterController playerMovement;
    public static float sensitivity = 0.5f;
    private float rotateX = 0f;

    private const float gravity = -9.81f; 
    private Vector3 vel; //Velocity of gravity
    private const float gDistance = 0.4f;
    public LayerMask groundMask;
    private bool isGrounded;
    public GameObject groundCollider; //Checks for ground below the player

    public GameObject camera;

    public List<Collider> interactables = new List<Collider>(); //Colliders with their indexes representing their UI ids
    public List<GameObject> uiPrefabs = new List<GameObject>(); //prefabs with their associated index IDs

    public GameObject optionsMenu;

    public static List<(int width, int height)> supportedResolutions = new List<(int width, int height)>()
    {(1920,1080),(1600,900),(1366,768),(1280,1024),(1280,720),(1024,768),(600,400)};
    void Start()
    {
        (int width, int height)[] playerResolutions = Screen.resolutions.OrderBy(x=>x.width).Reverse().Select(x=>(x.width,x.height)).ToArray();

        bool hasSetResolution = false;

        foreach((int width, int height) resolution in supportedResolutions)
        {
            if(playerResolutions.Contains(resolution))
            {
                Debug.Log("Setting resolution " + resolution.width + "x" + resolution.height);
                Screen.SetResolution(resolution.width, resolution.height, true);
                hasSetResolution = true;
                break;
            }
        }

        if(!hasSetResolution)
        {
            Debug.LogError("unrecognised resolution");
            supportedResolutions.Insert(0, (Screen.currentResolution.width, Screen.currentResolution.height)); //Adds resolution to list at 0th position
        }    
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        //Debug.DrawRay(camera.transform.position, camera.transform.TransformDirection(Vector3.forward) * 1.5f,Color.green);

        if (Input.GetButtonDown("Fire1") && !playerMovementLocked) //After the player presses E (the interaction button) send a raycast to detect any colliders infront of them
        {
            RaycastHit rHit;
            if (Physics.Raycast(camera.transform.position, camera.transform.TransformDirection(Vector3.forward), out rHit, 1.5f, (1 << 9))) //(1 << 9) sets the layer to only check interactables layer
            {
                //Debug.Log(interactables.IndexOf(rHit.collider));

                int loadIndex = interactables.IndexOf(rHit.collider); //the index to load

                if (loadIndex != -1)
                {
                    ToggleMenuState();
                    loadedUIElement = Instantiate(uiPrefabs[loadIndex],camera.transform);
                    loadedUIElement.GetComponent<UIDestroy>().objectDestroyMethod += ToggleMenuState; //makes it so when the UI element is destroyed movement is reenabled
                }
                else
                {
                    Debug.LogError("Invalid Interactable");
                }
            }
        }

        if(!playerMovementLocked)
        {
            if (Input.GetButtonDown("Cancel")) //Open option menu
            {
                ToggleMenuState();
                loadedUIElement = Instantiate(optionsMenu, camera.transform);
                loadedUIElement.GetComponent<UIDestroy>().objectDestroyMethod += ToggleMenuState; //makes it so when the UI element is destroyed movement is reenabled
            }
            else
            {
                MoveChar();
            }
        }
    }

    private void ToggleMenuState() //Simple function to lock player in place while in a menu
    {
        isInMenu = !isInMenu;
        playerMovementLocked = !playerMovementLocked;
        if(Cursor.lockState == CursorLockMode.Locked) { Cursor.lockState = CursorLockMode.Confined; }
        else { Cursor.lockState = CursorLockMode.Locked; }
        Cursor.visible = !Cursor.visible;
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
