using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;
public enum PauseState
{
    Default,
    Options,
    ExitDialog,
    Saving,
}
public class PauseMenuScript : MonoBehaviour
{
    public GameObject optionsPanel;
    public GameObject reallyExitPanel;

    //sidepanel buttons
    public Button optionsButton;
    public Button exitGame;
    public Button saveGame;

    //Really exit buttons
    public Button reallyExit;
    public Button reallyCancel;

    private PauseState currentState;
    private void SwitchState(PauseState newState)
    {
        currentState = newState;
    
        switch(newState)
        {
            case PauseState.Default:
                saveGame.interactable = true;
                optionsButton.interactable = true;
                exitGame.interactable = true;
                optionsPanel.SetActive(false);
                reallyExitPanel.SetActive(false);
                break;
            case PauseState.Options:
                saveGame.interactable = false;
                optionsButton.interactable = false;
                exitGame.interactable = false;
                optionsPanel.GetComponentInChildren<OptionsScript>().SetOptionsDefaults();
                optionsPanel.SetActive(true);
                reallyExitPanel.SetActive(false);
                break;
            case PauseState.ExitDialog:
                saveGame.interactable = false;
                exitGame.interactable = false;
                optionsButton.interactable = false;
                optionsPanel.SetActive(false);
                reallyExitPanel.SetActive(true);
                break;
            case PauseState.Saving:
                saveGame.interactable = false;
                exitGame.interactable = false;
                optionsButton.interactable = false;
                optionsPanel.SetActive(false);
                reallyExitPanel.SetActive(false);
                break;
            default:
                Debug.LogError("Invalid pause menu state");
                break;
        }
    }
    void Start()
    {
        optionsPanel.GetComponent<OptionsScript>().AddPauseMethods(SwitchState); //Add the method for switching state to the options script to allow it to close itself
        SwitchState(PauseState.Default);

        optionsButton.onClick.AddListener(() => SwitchState(PauseState.Options));
        exitGame.onClick.AddListener(() => SwitchState(PauseState.ExitDialog));
        saveGame.onClick.AddListener(()=> SwitchState(PauseState.Saving));

        reallyExit.onClick.AddListener(() => ExitToTitle()); ;
        reallyCancel.onClick.AddListener(() => SwitchState(PauseState.Default));

    }
    private void LoadSaveDialog()
    {

    }
    void ExitToTitle()
    {
        SceneManager.LoadScene("Title");
        Gameplay.ResetGamestate();
    }

    void Update()
    {
        
    }
}
