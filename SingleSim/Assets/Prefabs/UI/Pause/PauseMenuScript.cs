using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
enum PauseState
{
    Default,
    Options,
    ExitDialog
}
public class PauseMenuScript : MonoBehaviour
{
    public GameObject optionsPanel;

    //sensitivity items
    public InputField sensitivityInput;
    public Slider sensitivitySlider;

    //resolution items
    public TMPro.TMP_Dropdown resolutionDropdown;
    public Toggle fullscreen;

    public Button submitChanges;

    public Button optionsButton;
    public Button exitGame;
    public Button saveGame;

    private PauseState currentState;

    private const float minSensitivity = 0.1f;
    private const float maxSensitivity = 2.0f;
    private void SwitchState(PauseState newState)
    {
        currentState = newState;
        switch(newState)
        {
            case PauseState.Default:
                optionsPanel.SetActive(false);
                break;
            case PauseState.Options:
                sensitivitySlider.value = Movement.sensitivity;
                sensitivityInput.text = Movement.sensitivity.ToString("F2");
                optionsPanel.SetActive(true);
                break;
            case PauseState.ExitDialog:
                optionsPanel.SetActive(false);
                break;
            default:
                Debug.LogError("Invalid pause menu state");
                break;
        }
    }

    private void SensitivitySliderHandler(float newValue)
    {
        sensitivityInput.text = newValue.ToString("F2");
    }
    private void SensitivityInputHandler(string newValue)
    {
        float processedNewValue = minSensitivity;

        if (float.TryParse(newValue, out processedNewValue))
        {
            if (processedNewValue < minSensitivity) { processedNewValue = minSensitivity; sensitivityInput.text = processedNewValue.ToString("F2"); }
            else if (processedNewValue > maxSensitivity) { processedNewValue = maxSensitivity; sensitivityInput.text = processedNewValue.ToString("F2"); }
            sensitivitySlider.value = processedNewValue;
        }
        else
        {
            sensitivitySlider.value = minSensitivity;
            sensitivityInput.text = minSensitivity.ToString("F2");
        }
    }
    private void SubmitChanges() //For when a user attempts to submit their options changes
    {
        Movement.sensitivity = float.Parse(sensitivitySlider.value.ToString("F2")); //Set sensitivity, clamped to 2dp

        (int width, int height) splitRes = screenTextRef[resolutionDropdown.options[resolutionDropdown.value].text]; //Get the currently selected resolution and split into width and height
        Screen.SetResolution(splitRes.width, splitRes.height, fullscreen.isOn); //Set new screen resolution
        SwitchState(PauseState.Default);
    }

    Dictionary<string, (int width, int height)> screenTextRef = new Dictionary<string, (int width, int height)>();
    void Start()
    {
        SwitchState(PauseState.Default);
        optionsButton.onClick.AddListener(() => SwitchState(PauseState.Options));

        sensitivitySlider.minValue = minSensitivity;
        sensitivitySlider.maxValue = maxSensitivity;
        sensitivitySlider.onValueChanged.AddListener((newValue) => SensitivitySliderHandler(newValue));
        sensitivityInput.onEndEdit.AddListener((newValue) => SensitivityInputHandler(newValue));

        List<string> resolutionStrings = new List<string>();
        string selectedRes = "";

        foreach((int width, int height) res in Movement.supportedResolutions)
        {
            resolutionStrings.Add(res.width + "x" + res.height);
            screenTextRef.Add(res.width + "x" + res.height, (res.width, res.height));
        }

        selectedRes = Screen.width + "x" + Screen.height;

        resolutionDropdown.options.RemoveAll(x=>true);

        if (!resolutionStrings.Contains(selectedRes)) 
        {
            selectedRes = "<color=#B80E20>" + selectedRes + "</color>"; //Add the colour to the resolution string name
            resolutionStrings.Add(selectedRes);
            screenTextRef.Add(selectedRes, (Screen.width, Screen.height));
        } //if the resolution isnt in the supported list, add to options.

        resolutionDropdown.AddOptions(resolutionStrings);

        resolutionDropdown.value = resolutionDropdown.options.FindIndex(x => x.text == selectedRes);
        submitChanges.onClick.AddListener(() => SubmitChanges());

        fullscreen.isOn = Screen.fullScreen;
    }

    void Update()
    {
        
    }
}
