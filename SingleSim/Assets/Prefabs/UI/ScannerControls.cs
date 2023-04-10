using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScannerControls : MonoBehaviour
{
    public GameObject scannerHighlight;
    public Slider progSlider;
    public Button startScan;
    public GameObject mapSpotsPanel;
    public GameObject scanSpot;
    public GameObject scannerUploaded; //Console for after the scan has been uploaded
    private List<GameObject> loadedScanSpots = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //scannerUploaded.SetActive(false);
        //Make button activate the scanning - unless it has already started
        if (Gameplay.scanProg != -1) { startScan.interactable = false; }
        startScan.onClick.AddListener(() => BeginScanMode());

        if(Gameplay.isBoundsSet == false)
        {
            RectTransform r = mapSpotsPanel.transform.GetComponent<RectTransform>();
            Gameplay.bounds = (r.rect.width,r.rect.height); //Set the boundaries for where scan locations can appear when scan completes
        }
    }

    // Update is called once per frame
    void Update()
    {
        progSlider.value = Gameplay.scanProg; //Update the value of the scan progress slider

        if(Gameplay.scanSpotsAreAvailable && mapSpotsPanel.transform.childCount == 0) //If there are scan spots to be spawned and none currently on the map
        {
            int i = 0; //spotID counter
            foreach((float x, float y) posScanSpot in Gameplay.scanCoords)
            {
                GameObject newScan = Instantiate(scanSpot,mapSpotsPanel.transform,false);
                Vector3 newPos = new Vector3((-Gameplay.bounds.xBound / 2) + posScanSpot.x, (Gameplay.bounds.yBound / 2) - posScanSpot.y, 0); //Position isnt perfect but its close
                newScan.transform.Translate(newPos);
                newScan.name = "ScanSpot_" + i;
                newScan.GetComponentInChildren<Button>().onClick.AddListener(() => SelectScanSpot(newScan, posScanSpot));
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
        Gameplay.currentTextPos = 1;
        hasFinishedLoadingUIText = false;
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
        Gameplay.UIcoordinates.x = position.x;
        Gameplay.UIcoordinates.y = position.y;

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
        Gameplay.UItext = "Scan Complete \n" +
            "Recieved signal (" + Gameplay.UIcoordinates.x + "," + Gameplay.UIcoordinates.y + ")\n"
            + "Uploading data to decoder console...\n";
    }

    bool hasFinishedLoadingUIText = false; //This will check on the console data side if the UI has loaded. will be reset on unloading and reloading the UI but reset
    void LoadConsoleText()
    {
        if (!hasFinishedLoadingUIText)
        {
            Gameplay.textTime += Time.deltaTime;
            if (Gameplay.textTime > 0.03)
            {
                Gameplay.currentTextPos += 1;
                if (Gameplay.currentTextPos >= Gameplay.UItext.Length)
                {
                    Gameplay.currentTextPos = Gameplay.UItext.Length;
                    hasFinishedLoadingUIText = false;
                }
                Gameplay.textTime = 0;
        }
            scannerUploaded.GetComponentInChildren<Text>().text = Gameplay.UItext.Substring(0, Gameplay.currentTextPos).ToString();

        }
    }
}
