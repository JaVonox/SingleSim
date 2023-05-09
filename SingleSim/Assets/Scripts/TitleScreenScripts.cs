using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
public class TitleScreenScripts : MonoBehaviour
{
    public Button newGame;
    public Button loadGame;
    public Button options;
    public Button exit;

    public AudioSource startupAudio;
    public AudioSource continualAudio;

    public Camera mainCamera;

    //Breathing variables
    private Vector3 cameraStartVec;
    private Vector3 chestRiseVec;
    private const float chestRiseHeight = 0.2f;
    private bool chestRise = true;
    private float breathTime = 0;

    private float dTime = -1.0f;
    // Start is called before the first frame update
    void Start()
    {
        cameraStartVec = mainCamera.transform.position;
        chestRiseVec = new Vector3(cameraStartVec.x, cameraStartVec.y + chestRiseHeight, cameraStartVec.z);
        startupAudio.Play();
        startupAudio.volume = 0.3f * Movement.volume;
        continualAudio.volume = 0.3f * Movement.volume;

        newGame.onClick.AddListener(() => StartNewGame());
        exit.onClick.AddListener(() => Application.Quit());

        (int width, int height)[] playerResolutions = Screen.resolutions.OrderBy(x => x.width).Reverse().Select(x => (x.width, x.height)).ToArray();

        bool hasSetResolution = false;

        foreach ((int width, int height) resolution in Movement.supportedResolutions)
        {
            if (playerResolutions.Contains(resolution))
            {
                Screen.SetResolution(resolution.width, resolution.height, true);
                hasSetResolution = true;
                break;
            }
        }

        if (!hasSetResolution)
        {
            Debug.LogError("unrecognised resolution");
            Movement.supportedResolutions.Insert(0, (Screen.currentResolution.width, Screen.currentResolution.height)); //Adds resolution to list at 0th position
        }

        Movement.defaultScreenRes = (Screen.width, Screen.height);

        Application.targetFrameRate = 120;

    }

    void StartNewGame()
    {
        startupAudio.Stop();
        continualAudio.Stop();
        Gameplay.Setup();
        SceneManager.LoadScene("Main");
    }

    // Update is called once per frame
    void Update()
    {
        dTime += Time.deltaTime;

        //Need one breath cycle to finish over 5 seconds period?
        if(dTime >= 0.05f)
        {
            breathTime += dTime;
            if(mainCamera.transform.position.y <= cameraStartVec.y && !chestRise)
            {
                mainCamera.transform.position = cameraStartVec;
                breathTime = 0;
                chestRise = true;
            }
            else if(mainCamera.transform.position.y >= chestRiseVec.y && chestRise)
            {
                mainCamera.transform.position = cameraStartVec;
                breathTime = 0;
                chestRise = false;
            }

            if(chestRise)
            {
                mainCamera.transform.position = Vector3.Lerp(cameraStartVec, chestRiseVec, breathTime / 2.5f);
            }
            else
            {
                mainCamera.transform.position = Vector3.Lerp(chestRiseVec, cameraStartVec, breathTime / 2.5f);
            }
            dTime = 0;
        }
        if(Cursor.visible == false) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true; 
        }
        if(startupAudio.isPlaying == false && continualAudio.isPlaying == false)
        {
            continualAudio.Play();
        }
        
    }
}
