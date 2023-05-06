using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

    private Dictionary<Button, (TextMeshProUGUI modifiableObject, bool IsPositive)> btnToModifier = new Dictionary<Button, (TextMeshProUGUI modifiableObject, bool IsPositive)>();

    void AddDigitsListeners()
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
        AddDigitsListeners();
        //Make button activate the scanning - unless it has already started
        if (Gameplay.scanProg != -1) { startScan.interactable = false; }
        startScan.onClick.AddListener(() => BeginScanMode());

        if(Gameplay.isBoundsSet == false)
        {
            RectTransform r = mapSpotsPanel.transform.GetComponent<RectTransform>();
            Gameplay.bounds = (r.rect.width,r.rect.height); //Set the boundaries for where scan locations can appear when scan completes
        }

        if (Gameplay.scanProg == -1 && !Gameplay.scanSpotsAreAvailable)
        {
            scannerUploaded.GetComponentInChildren<Text>().text = "Scanner console active\n" +
                "Press 'Perform Scan' to begin scanning for signals";
            scannerUploaded.SetActive(true);
        }
        else
        {
            scannerUploaded.SetActive(false);
        }

        }
    void Update()
    {
        progSlider.value = Gameplay.scanProg; //Update the value of the scan progress slider

        if(Gameplay.scanSpotsAreAvailable && mapSpotsPanel.transform.childCount == 0) //If there are scan spots to be spawned and none currently on the map
        {
            int i = 0; //spotID counter
            foreach(Scanspot posScanSpot in Gameplay.scanCoords)
            {
                GameObject newScan = Instantiate(scanSpot,mapSpotsPanel.transform,false);
                Vector3 newPos = new Vector3((-Gameplay.bounds.xBound / 2) + posScanSpot.x, (Gameplay.bounds.yBound / 2) - posScanSpot.y, 0); //Position isnt perfect but its close
                newScan.transform.Translate(newPos);
                newScan.name = "ScanSpot_" + i;
                newScan.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
                newScan.GetComponentInChildren<Button>().onClick.AddListener(() => SelectScanSpot(newScan, posScanSpot.GetCoordsTuple()));
                loadedScanSpots.Add(newScan);
                i++;
            }
        }


        if (Gameplay.scannerConsolePopupEnabled == true)
        {
            if(scannerUploaded.activeSelf == false)
            {
                scannerUploaded.SetActive(true);
            }
            LoadConsoleText();
        }

        if(startScan.interactable == false && Gameplay.scanProg == -1) { startScan.interactable = true; }
    }
    void BeginScanMode()
    {
        if(scannerUploaded.activeSelf == true) { scannerUploaded.SetActive(false); Gameplay.scannerConsolePopupEnabled = false; }
        startScan.interactable = false;
        Gameplay.scanProg = 0;
        Gameplay.currentScanTextPos = -1;
        Gameplay.textTime = 0;
        Gameplay.scanSpotsAreAvailable = false;
        Gameplay.scanCoords.Clear();
        if(mapSpotsPanel.transform.childCount > 0) //Unload all scan spots
        {
            foreach (Transform child in mapSpotsPanel.transform)
            {
                Destroy(child.gameObject);
            }
        }
        loadedScanSpots.Clear();
    }
    void SelectScanSpot(GameObject selectedScanSpot, (float x, float y) position) //When a spot is selected
    {
        //Convert numbers to coordinates, giving them some obfuscated digits in the process 
        Gameplay.UIcoordinates.x = double.Parse(position.x.ToString() + Random.Range(0,10000).ToString());
        Gameplay.UIcoordinates.y = double.Parse(position.y.ToString() + Random.Range(0,10000).ToString());
        StoreConsoleText();

        Gameplay.scannerConsolePopupEnabled = true;
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

        scannerUploaded.SetActive(true);
        LoadConsoleText();

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
