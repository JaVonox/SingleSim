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
    private List<GameObject> loadedScanSpots = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
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
            foreach((float x, float y) posScanSpot in Gameplay.scanCoords)
            {
                GameObject newScan = Instantiate(scanSpot,mapSpotsPanel.transform,false);
                Vector3 newPos = new Vector3((-Gameplay.bounds.xBound / 2) + posScanSpot.x, (Gameplay.bounds.yBound / 2) - posScanSpot.y, 0); //Position isnt perfect but its close
                newScan.transform.Translate(newPos);
                loadedScanSpots.Add(newScan);
            }
        }

        if(startScan.interactable == false && Gameplay.scanProg == -1) { startScan.interactable = true; }
    }
    void BeginScanMode()
    {
        startScan.interactable = false;
        Gameplay.scanProg = 0;
        Gameplay.scanSpotsAreAvailable = false;
        Gameplay.scanCoords.Clear();
        if(mapSpotsPanel.transform.childCount > 0)
        {
            foreach (Transform child in mapSpotsPanel.transform)
            {
                Destroy(child.gameObject);
            }
        }
        loadedScanSpots.Clear();
    }
}
