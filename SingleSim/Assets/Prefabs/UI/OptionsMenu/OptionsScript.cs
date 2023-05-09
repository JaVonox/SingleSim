using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class OptionsScript : MonoBehaviour
{
    //sensitivity items
    public InputField sensitivityInput;
    public Slider sensitivitySlider;

    //resolution items
    public TMPro.TMP_Dropdown resolutionDropdown;
    public Toggle fullscreen;

    //volume items
    public InputField volumeInput;
    public Slider volumeSlider;
    //options buttons
    public Button submitChanges;
    public Button restoreDefault;
    public Button cancel;

    //Actions
    public System.Action<PauseState> pauseSwitchMethod;
    public System.Action<TitleState> titleSwitchMethod;

    private const float minSensitivity = 0.1f;
    private const float maxSensitivity = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        sensitivitySlider.onValueChanged.AddListener((newValue) => SensitivitySliderHandler(newValue));
        sensitivityInput.onEndEdit.AddListener((newValue) => SensitivityInputHandler(newValue));

        volumeSlider.onValueChanged.AddListener((newValue) => VolumeSliderHandler(newValue));
        volumeInput.onEndEdit.AddListener((newValue) => VolumeInputHandler(newValue)); ;

        submitChanges.onClick.AddListener(() => SubmitChanges());
        restoreDefault.onClick.AddListener(() => RestoreDefaultSettings());
        cancel.onClick.AddListener(() => CancelChanges());
    }

    public void AddPauseMethods(System.Action<PauseState> inhSwitchMethod)
    {
        pauseSwitchMethod = inhSwitchMethod;
    }

    public void AddPauseMethods(System.Action<TitleState> inhSwitchMethod)
    {
        titleSwitchMethod = inhSwitchMethod;
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    Dictionary<string, (int width, int height)> screenTextRef = new Dictionary<string, (int width, int height)>();
    public void SetOptionsDefaults()
    {
        sensitivitySlider.minValue = minSensitivity;
        sensitivitySlider.maxValue = maxSensitivity;

        volumeSlider.minValue = 0;
        volumeSlider.maxValue = 1;

        sensitivitySlider.value = Movement.sensitivity;
        sensitivityInput.text = Movement.sensitivity.ToString("F2");

        volumeSlider.value = Movement.volume;
        volumeInput.text = Movement.volume.ToString("F2");

        List<string> resolutionStrings = new List<string>();
        string selectedRes = "";

        screenTextRef.Clear();

        foreach ((int width, int height) res in Movement.supportedResolutions)
        {
            resolutionStrings.Add(res.width + "x" + res.height);
            screenTextRef.Add(res.width + "x" + res.height, (res.width, res.height));
        }

        selectedRes = Screen.width + "x" + Screen.height;

        resolutionDropdown.options.RemoveAll(x => true);

        if (!resolutionStrings.Contains(selectedRes))
        {
            selectedRes = "<color=#B80E20>" + selectedRes + "</color>"; //Add the colour to the resolution string name
            resolutionStrings.Add(selectedRes);
            screenTextRef.Add(selectedRes, (Screen.width, Screen.height));
        } //if the resolution isnt in the supported list, add to options.

        resolutionDropdown.AddOptions(resolutionStrings);

        resolutionDropdown.value = resolutionDropdown.options.FindIndex(x => x.text == selectedRes);
        fullscreen.isOn = Screen.fullScreen;
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

    private void VolumeSliderHandler(float newValue)
    {
        volumeInput.text = newValue.ToString("F2");
    }
    private void VolumeInputHandler(string newValue)
    {
        float processedNewValue = 0;

        if (float.TryParse(newValue, out processedNewValue))
        {
            if (processedNewValue < 0) { processedNewValue = 0; volumeInput.text = processedNewValue.ToString("F2"); }
            else if (processedNewValue > 1) { processedNewValue = 1; volumeInput.text = processedNewValue.ToString("F2"); }
            volumeSlider.value = processedNewValue;
        }
        else
        {
            volumeSlider.value = 0;
            volumeInput.text = "0";
        }
    }

    private void SubmitChanges() //For when a user attempts to submit their options changes
    {
        Movement.sensitivity = float.Parse(sensitivitySlider.value.ToString("F2")); //Set sensitivity, clamped to 2dp
        Movement.volume = float.Parse(volumeSlider.value.ToString("F2"));

        (int width, int height) splitRes = screenTextRef[resolutionDropdown.options[resolutionDropdown.value].text]; //Get the currently selected resolution and split into width and height
        Screen.SetResolution(splitRes.width, splitRes.height, fullscreen.isOn); //Set new screen resolution

        if (pauseSwitchMethod != null)
        {
            pauseSwitchMethod(PauseState.Default);
        }
        else if(titleSwitchMethod != null)
        {
            titleSwitchMethod(TitleState.Default);
        }
    }

    private void RestoreDefaultSettings()
    {
        if (pauseSwitchMethod != null)
        {
            pauseSwitchMethod(PauseState.Default);
        }
        else if (titleSwitchMethod != null)
        {
            titleSwitchMethod(TitleState.Default);
        }
        Movement.sensitivity = 0.5f;
        Movement.volume = 0.5f;
        Screen.SetResolution(Movement.defaultScreenRes.width, Movement.defaultScreenRes.height, true);
        SetOptionsDefaults();
    }

    private void CancelChanges()
    {
        if (pauseSwitchMethod != null)
        {
            pauseSwitchMethod(PauseState.Default);
        }
        else if (titleSwitchMethod != null)
        {
            titleSwitchMethod(TitleState.Default);
        }
        SetOptionsDefaults();
    }

}
