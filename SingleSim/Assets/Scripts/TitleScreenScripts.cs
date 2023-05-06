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
    // Start is called before the first frame update
    void Start()
    {
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

    }

    void StartNewGame()
    {
        startupAudio.Stop();
        continualAudio.Stop();
        SceneManager.LoadScene("Main");
    }

    // Update is called once per frame
    void Update()
    {
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
