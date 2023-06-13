using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ScanState
{
    IdleConsole,
    Scanning,
    EndScreen,
    ReplaceScreen,
    Disabled
}
public class ScannerControls : MonoBehaviour
{
    public GameObject scannerHighlight;
    public Slider progSlider;
    public Button startScan;
    public GameObject mapSpotsPanel;
    public GameObject scanSpot;
    public GameObject scannerUploaded; //Console for after the scan has been uploaded
    private List<GameObject> loadedScanSpots = new List<GameObject>();

    public TextMeshProUGUI hundredsText;
    public Button hundredsIncrement;
    public Button hundredsDecrement;

    public TextMeshProUGUI tensText;
    public Button tensIncrement;
    public Button tensDecrement;

    public TextMeshProUGUI onesText;
    public Button onesIncrement;
    public Button onesDecrement;

    public GameObject replacementPanel;
    public TextMeshProUGUI replacementText;
    public Button replaceSignal;
    public Button cancelReplace;

    public GameObject disabledPanel;
    public TextMeshProUGUI disabledText;
    public Button retryDisabled;

    public static ScanState currentState;

    private Dictionary<Button, (TextMeshProUGUI modifiableObject, bool IsPositive)> btnToModifier = new Dictionary<Button, (TextMeshProUGUI modifiableObject, bool IsPositive)>();

    void SwitchState(ScanState newState)
    {
        currentState = newState;

        switch(currentState)
        {
            case ScanState.IdleConsole:
                scannerUploaded.SetActive(true);
                LoadDefaultText();
                replacementPanel.SetActive(false);
                disabledPanel.SetActive(false);
                break;
            case ScanState.Scanning:
                scannerUploaded.SetActive(false);
                replacementPanel.SetActive(false);
                disabledPanel.SetActive(false);
                break;
            case ScanState.EndScreen:
                scannerUploaded.SetActive(true);
                LoadConsoleText();
                replacementPanel.SetActive(false);
                disabledPanel.SetActive(false);
                break;
            case ScanState.ReplaceScreen:
                SetupReplace();
                scannerUploaded.SetActive(false);
                replacementPanel.SetActive(true);
                disabledPanel.SetActive(false);
                startScan.interactable = false;
                break;
            case ScanState.Disabled:
                SetupDisabled();
                scannerUploaded.SetActive(false);
                replacementPanel.SetActive(false);
                disabledPanel.SetActive(true);
                startScan.interactable = false;
                break;
            default:
                Debug.LogError("Invalid scanner state");
                break;
        }
    }

    void SetupReplace()
    {
        replaceSignal.onClick.RemoveAllListeners();
        cancelReplace.onClick.RemoveAllListeners();

        if (Gameplay.storyState == 5 || Gameplay.storyState == 4 || Gameplay.storyState == 2) //If in the tutorial stages where replacement should not be allowed
        {
            replacementText.text = "Signal replacement is currently disabled due to group policy";
            replaceSignal.interactable = false;
        }
        else //If outside the tutorial state
        {
            replacementText.text = "Downloading a second signal will replace the currently loaded signal, are you sure you wish to proceed?";
            replaceSignal.interactable = true;
        }

        replaceSignal.onClick.AddListener(() => SendToDecoder());
        cancelReplace.onClick.AddListener(() => SwitchState(ScanState.Scanning));
    }

    void SetupDisabled()
    {
        Gameplay.scanCoords.Clear();
        loadedScanSpots.Clear();
        Gameplay.scanProg = -1;
        Gameplay.scannerState = "idle";

        if (Gameplay.storyState == 0) { disabledText.text = "Scanner is currently disabled by admin. Message: 'Check my email!'"; }
        else if (Gameplay.storyState == 5) { disabledText.text = "Scanner is currently disabled by admin. Message: 'Match the signals first!'"; }
        Gameplay.scanSpotsAreAvailable = false;
        retryDisabled.onClick.RemoveAllListeners();
        retryDisabled.onClick.AddListener(() => CheckDisabledMode(true));
        SetHzInteractionState(false);
    }

    void CheckDisabledMode(bool isRefresh)
    {
        if(Gameplay.storyState == 5 || Gameplay.storyState == 0)
        {
            SwitchState(ScanState.Disabled);
        }
        else
        {
            if(isRefresh) //If attempting to refresh the state rather than simply check if disabled mode is active
            {
                SwitchState(ScanState.IdleConsole);
                //Delete all scan data to force a switch into idle mode
                Gameplay.scanCoords.Clear();
                loadedScanSpots.Clear();
                Gameplay.scanProg = -1;
                Gameplay.scannerState = "idle";
                Gameplay.scanSpotsAreAvailable = false;
            }
        }
    }
    void SetupDigits()
    {
        //Add in references in dictionary
        btnToModifier.Add(hundredsIncrement, (hundredsText,true));
        hundredsIncrement.onClick.AddListener(() => ChangeDigit(hundredsIncrement));
        btnToModifier.Add(hundredsDecrement, (hundredsText, false));
        hundredsDecrement.onClick.AddListener(() => ChangeDigit(hundredsDecrement));
        btnToModifier.Add(tensIncrement, (tensText, true));
        tensIncrement.onClick.AddListener(() => ChangeDigit(tensIncrement));
        btnToModifier.Add(tensDecrement, (tensText, false));
        tensDecrement.onClick.AddListener(() => ChangeDigit(tensDecrement));
        btnToModifier.Add(onesIncrement, (onesText, true));
        onesIncrement.onClick.AddListener(() => ChangeDigit(onesIncrement));
        btnToModifier.Add(onesDecrement, (onesText, false));
        onesDecrement.onClick.AddListener(() => ChangeDigit(onesDecrement));

        string[] hzString = ConvertHzArray(Gameplay.lastLoadedHz);
        hundredsText.text = hzString[0].ToString();
        tensText.text = hzString[1].ToString();
        onesText.text = hzString[2].ToString();
    }

    void ChangeDigit(Button sender)
    {
        int newDigitVal = int.Parse(btnToModifier[sender].modifiableObject.text);

        if(btnToModifier[sender].IsPositive) //If an increment button is pressed
        {
            newDigitVal = newDigitVal == 9 ? newDigitVal = 0 : newDigitVal += 1;
        }
        else
        {
            newDigitVal = newDigitVal == 0 ? newDigitVal = 9 : newDigitVal -= 1;
        }

        btnToModifier[sender].modifiableObject.text = newDigitVal.ToString();
    }
    void Start()
    {
        SetupDigits();
        //Make button activate the scanning - unless it has already started
        if (Gameplay.scanProg != -1) { startScan.interactable = false; SetHzInteractionState(false); }
        else { startScan.interactable = true; SetHzInteractionState(true); }
        startScan.onClick.AddListener(() => BeginScanMode());

        if(Gameplay.isBoundsSet == false)
        {
            RectTransform r = mapSpotsPanel.transform.GetComponent<RectTransform>();
            Gameplay.bounds = (r.rect.width,r.rect.height); //Set the boundaries for where scan locations can appear when scan completes
        }

        SwitchState(currentState);

    }

    void LoadDefaultText()
    {
        scannerUploaded.GetComponentInChildren<Text>().text = "Scanner console active\n" +
        "Current expected signal reading range: +/- " + (Gameplay.signalReadingRange + Random.Range(0.001f, 0.8f)).ToString() + "MHz \n" +
        "Press 'Perform Scan' to begin scanning for signals.";
    }
    void Update()
    {
        CheckDisabledMode(false);
        if (Input.mouseScrollDelta.y != 0) { ScrollHandler(Input.mouseScrollDelta.y); }
        progSlider.value = Gameplay.scanProg; //Update the value of the scan progress slider
        Gameplay.lastLoadedHz = int.Parse(hundredsText.text) * 100 + int.Parse(tensText.text) * 10 + int.Parse(onesText.text); //Constantly update hz value stored

        if (Gameplay.scanSpotsAreAvailable && mapSpotsPanel.transform.childCount == 0) //If there are scan spots to be spawned and none currently on the map
        {
            int i = 0; //spotID counter
            foreach(Scanspot posScanSpot in Gameplay.scanCoords)
            {
                GameObject newScan = Instantiate(scanSpot,mapSpotsPanel.transform,false);
                Button scanInteract = newScan.GetComponentInChildren<Button>();

                Vector3 newPos = new Vector3((-Gameplay.bounds.xBound / 2) + posScanSpot.x, (Gameplay.bounds.yBound / 2) - posScanSpot.y, 0); //Position isnt perfect but its close
                newScan.transform.Translate(newPos);
                newScan.name = "ScanSpot_" + i;

                scanInteract.onClick.RemoveAllListeners();
                scanInteract.onClick.AddListener(() => SelectScanSpot(newScan, posScanSpot.GetCoordsTuple()));

                loadedScanSpots.Add(newScan);
                SetupScanSpotGameobject();
                i++;
            }
        }

        if(Gameplay.scanProg == -1 && loadedScanSpots.Count > 0) //When the scan finishes, reenable all buttons
        {
            SetHzInteractionState(true);
            if (loadedScanSpots[0].GetComponentInChildren<Button>().interactable == false)
            {
                SetupScanSpotGameobject();
            }
        }


        if (currentState == ScanState.EndScreen)
        {
            LoadConsoleText();
        }

        if(startScan.interactable == false && Gameplay.scanProg == -1) { startScan.interactable = true; }
    }

    string[] ConvertHzArray(int freq)
    {
        string hzString = freq.ToString();
        string[] outStr;

        if (freq >= 100)
        {
            outStr = new string[]{ hzString[0].ToString(), hzString[1].ToString(),hzString[2].ToString()};
        }
        else if (freq >= 10)
        {
            outStr = new string[] { "0", hzString[0].ToString(), hzString[1].ToString() };
        }
        else if (freq >= 1)
        {
            outStr = new string[] { "0", "0", hzString[0].ToString() };
        }
        else
        {
            outStr = new string[] { "0","0","0"};
        }

        return outStr;
    }
    void ScrollHandler(float yIn)
    {
        if (scrollWheelEnabled)
        {
            int cVal = int.Parse(hundredsText.text) * 100 + int.Parse(tensText.text) * 10 + int.Parse(onesText.text);
            int updatedFreq = Mathf.Clamp(cVal + Mathf.CeilToInt(yIn), 0, 999);
            string[] hzString = ConvertHzArray(updatedFreq);

            hundredsText.text = hzString[0].ToString();
            tensText.text = hzString[1].ToString();
            onesText.text = hzString[2].ToString();
        }

    }

    void SetupScanSpotGameobject()
    {
        int currentFreq = int.Parse(Gameplay.lastSentHz.ToString());
        int i = 0;
        foreach (GameObject scanspot in loadedScanSpots)
        {
            Button scanInteract = scanspot.GetComponentInChildren<Button>();

            int offset = Mathf.Abs(Gameplay.scanCoords[i].freq - currentFreq); //The abs distance from the frequence of the scanspot

            if (Gameplay.scanProg != -1) //If a scan is in progress, remove the interaction until it is done
            {
                scanInteract.interactable = false;
                scanInteract.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            }
            else
            {
                if (offset > Gameplay.signalReadingRange) //If the signal is outside the range atwhich signal reading can occur
                {
                    float offsetRange = (float)offset / 200.0f;
                    scanInteract.gameObject.GetComponent<Image>().color = Color.Lerp(new Color(0.12f, 0.72f, 0.05f, 1), new Color(0.72f, 0.05f, 0.12f, 1), offsetRange);
                    scanInteract.interactable = false;
                }
                else
                {
                    scanInteract.gameObject.GetComponent<Image>().color = new Color(0.12f, 0.72f, 0.05f, 1);
                    scanInteract.interactable = true;
                }
            }
            i++;
        }
    }
    void BeginScanMode()
    {
        SwitchState(ScanState.Scanning);
        startScan.interactable = false;
        SetHzInteractionState(false);
        Gameplay.scanProg = 0;
        Gameplay.currentScanTextPos = -1;
        Gameplay.lastSentHz = Gameplay.lastLoadedHz; //Load in the last hz recorded to be sent as the scan

        if(mapSpotsPanel.transform.childCount > 0) //Unload all scan spots
        {
            foreach (Transform child in mapSpotsPanel.transform)
            {
                Destroy(child.gameObject);
            }
        }
        loadedScanSpots.Clear();
    }

    bool scrollWheelEnabled = true; //If scroll wheel can be used to change the hz values
    void SetHzInteractionState(bool isOn)
    {
        hundredsIncrement.interactable = isOn;
        tensIncrement.interactable = isOn;
        onesIncrement.interactable = isOn;

        hundredsDecrement.interactable = isOn;
        tensDecrement.interactable = isOn;
        onesDecrement.interactable = isOn;
        scrollWheelEnabled = isOn;

        if(!isOn)
        {
            hundredsText.color = Color.grey;
            tensText.color = Color.grey;
            onesText.color = Color.grey;
        }
        else
        {
            hundredsText.color = Color.white;
            tensText.color = Color.white;
            onesText.color = Color.white;
        }
    }
    void SelectScanSpot(GameObject selectedScanSpot, (float x, float y) position) //When a spot is selected
    {
        //Convert numbers to coordinates, giving them some obfuscated digits in the process 
        //This is fine to store regardless of if the data is to be replaced. 
        Gameplay.UIcoordinates.x = double.Parse(position.x.ToString() + Random.Range(0,10000).ToString());
        Gameplay.UIcoordinates.y = double.Parse(position.y.ToString() + Random.Range(0,10000).ToString());

        if (Gameplay.activeAlien != null)
        {
            SwitchState(ScanState.ReplaceScreen); //Load the replace dialog box
        }
        else
        {
            SendToDecoder();
        }
    }

    void SendToDecoder()
    {
        StoreConsoleText();
        Gameplay.scanProg = -1;
        Gameplay.scanSpotsAreAvailable = false;
        Gameplay.scanCoords.Clear();

        if (mapSpotsPanel.transform.childCount > 0) //Unload all scan spots
        {
            foreach (Transform child in mapSpotsPanel.transform)
            {
                Destroy(child.gameObject);
            }
        }
        loadedScanSpots.Clear();

        SwitchState(ScanState.EndScreen);

        if (Gameplay.storyState == 1) { Gameplay.storyState = 2; Gameplay.tutorialStateUpdateNeeded = true; }
        else if (Gameplay.storyState == 3) { Gameplay.storyState = 4; Gameplay.tutorialStateUpdateNeeded = true; }

        Gameplay.AddNewAlien();

    }
    void StoreConsoleText()
    {
        //Scan Complete
        //Uploaded target signal to decoder console
        Gameplay.scanUIText = "Scan Complete \n" +
            "Downloaded signal (" + Gameplay.UIcoordinates.x + " , " + Gameplay.UIcoordinates.y + ")\n"
            + "Data uploaded to decoder console...\n\n\n"
            + "Raw stream:\n\n"
            + GenerateRawData()
            + "\nPress 'Perform Scan' to scan for new signals";

        Gameplay.currentScanTextPos = 0;
    }
    string GenerateRawData()
    {
        string rawData = "";
        
        for(int i = 0;i <= 255;i++)
        {
            rawData += (char)(Random.Range(33, 126));
        }
        rawData += "\n\n (Cont. " + Random.Range(200, 4000).ToString() + " bytes)";
        return rawData;
    }
    void LoadConsoleText()
    {
        scannerUploaded.GetComponentInChildren<Text>().text = Gameplay.scanUIText.Substring(0, Gameplay.currentScanTextPos).ToString();
    }
}
