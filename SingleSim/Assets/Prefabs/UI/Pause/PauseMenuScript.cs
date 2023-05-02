using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum PauseState
{
    Default,
    Options,
    ExitDialog
}
public class PauseMenuScript : MonoBehaviour
{
    public GameObject optionsPanel;
    public InputField sensitivityInput;
    public Slider sensitivitySlider;
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

        SwitchState(PauseState.Default);
    }
    void Start()
    {
        SwitchState(PauseState.Default);
        optionsButton.onClick.AddListener(() => SwitchState(PauseState.Options));
        sensitivitySlider.minValue = minSensitivity;
        sensitivitySlider.maxValue = maxSensitivity;
        sensitivitySlider.onValueChanged.AddListener((newValue) => SensitivitySliderHandler(newValue));
        sensitivityInput.onEndEdit.AddListener((newValue) => SensitivityInputHandler(newValue));
        submitChanges.onClick.AddListener(() => SubmitChanges());
    }

    void Update()
    {
        
    }
}
