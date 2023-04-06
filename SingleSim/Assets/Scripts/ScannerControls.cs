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
            int i = 0;
            foreach((float x, float y) posScanSpot in Gameplay.scanCoords)
            {
                Debug.Log("x " + posScanSpot.x + "," + posScanSpot.y);
                GameObject newScan = Instantiate(scanSpot,mapSpotsPanel.transform,false);
                //newScan.transform.SetParent(mapSpotsPanel.transform,false);
                newScan.transform.position = new Vector3(0, 0);
                //newScan.transform.Rotate(new Vector3(-180, 180, 0));
                loadedScanSpots.Add(newScan);
                i++;
                //new Vector3(posScanSpot.x, posScanSpot.y), Quaternion.identity
            }
        }
    }
    void BeginScanMode()
    {
        startScan.interactable = false;
        Gameplay.scanProg = 0;
        Gameplay.scanSpotsAreAvailable = false;
        if(mapSpotsPanel.transform.childCount > 0)
        {
            int childCount = mapSpotsPanel.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                GameObject.Destroy(mapSpotsPanel.transform.GetChild(0)); //Destroy each child
            }
        }
        loadedScanSpots.Clear();
    }
}