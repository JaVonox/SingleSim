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

    //Saving panels
    public GameObject savePanel;
    public InputField saveName;
    public Button createSave;
    public Button cancelSave;
    public Text errorText;

    private PauseState currentState;
    private void SwitchState(PauseState newState)
    {
        currentState = newState;

        createSave.onClick.RemoveAllListeners();
        cancelSave.onClick.RemoveAllListeners();
        saveName.onValueChanged.RemoveAllListeners();
        errorText.text = "";
    
        switch(newState)
        {
            case PauseState.Default:
                saveGame.interactable = true;
                optionsButton.interactable = true;
                exitGame.interactable = true;
                savePanel.SetActive(false);
                optionsPanel.SetActive(false);
                reallyExitPanel.SetActive(false);
                break;
            case PauseState.Options:
                saveGame.interactable = false;
                optionsButton.interactable = false;
                exitGame.interactable = false;
                optionsPanel.GetComponentInChildren<OptionsScript>().SetOptionsDefaults();
                savePanel.SetActive(false);
                optionsPanel.SetActive(true);
                reallyExitPanel.SetActive(false);
                break;
            case PauseState.ExitDialog:
                saveGame.interactable = false;
                exitGame.interactable = false;
                optionsButton.interactable = false;
                savePanel.SetActive(false);
                optionsPanel.SetActive(false);
                reallyExitPanel.SetActive(true);
                break;
            case PauseState.Saving:
                saveGame.interactable = false;
                exitGame.interactable = false;
                optionsButton.interactable = false;
                savePanel.SetActive(true);
                optionsPanel.SetActive(false);
                reallyExitPanel.SetActive(false);
                createSave.interactable = false;
                createSave.onClick.AddListener(() => AttemptSave());
                cancelSave.onClick.AddListener(() => SwitchState(PauseState.Default));
                saveName.onValueChanged.AddListener((x) => UpdateSaveErrorText(x));
                UpdateSaveErrorText("");
                break;
            default:
                Debug.LogError("Invalid pause menu state");
                break;
        }
    }

    public void AttemptSave()
    {
        FileLoading.CreateSave(saveName.text, false);
        SwitchState(PauseState.Default);
    }

    public void UpdateSaveErrorText(string newVal)
    {
        createSave.interactable = false;
        errorText.color = new Color(0.72f, 0.05f, 0.12f, 1);


        if (newVal.Length > 0 && FileLoading.IsValidFilename(newVal))
        {
            errorText.color = new Color(0.12f, 0.72f, 0.05f, 1);
            if (FileLoading.DoesDirExist(newVal))
            {
                errorText.color = Color.white;
                errorText.text = "A save file with this name already exists and will be overwritten";
            }
            else
            {
                errorText.text = "The specified file name is valid";
            }
            createSave.interactable = true;
        }
        else
        {
            errorText.text = "Invalid file name";
        }

        if (!FileLoading.DoesDirExist(null))
        {
            errorText.text += "\nSave file directory not found. A new save file directory will be created.";
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
