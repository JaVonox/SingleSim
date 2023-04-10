using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Gameplay : MonoBehaviour
{
    public static float scanSpeed = 0.05f; //How fast scanning occurs
    public static float scanProg = -1; //-1 for not started. locked to 0-1
    public static List<(float x, float y)> scanCoords = new List<(float x, float y)>(); //Stores available positions of scan coordinates
    public static bool scanSpotsAreAvailable = false; //Bool says if there are available scan coordinates on screen
    public static int numberOfScansSpots = 5; //how many scan spots can appear at once
    public static (float xBound, float yBound) bounds; //Stores the boundaries of the scanSet
    public static bool isBoundsSet = false;
    private float secsSinceLastUpdate = 0;

    public static Alien activeAlien = new Alien(); //The current alien stats loaded by the scanner
    public static List<Alien> storedAliens = new List<Alien>(); //The previously completed alien scans placed in storage
    
    public static bool scannerConsolePopupEnabled = false; //Checks if the scanner console pop up should be enabled when loaded
    public static (double x, double y) UIcoordinates; //Stores the coordinates for the last completed signal for the UI popup to handle
    public static string UItext;
    public static int currentTextPos = -1;
    public static float textTime = 0;

    public GameObject scannerObject;
    public List<Sprite> scannerSpriteStates;

    // Start is called before the first frame update
    void Start()
    {
        SetScannerState("idle");
    }

    // Update is called once per frame
    void Update()
    {
        secsSinceLastUpdate += Time.deltaTime;

        if (secsSinceLastUpdate >= 0.2f)
        {
            if(scanProg == 0)
            {
                SetScannerState("scanning");
            }

            if (scanProg != -1 && scanProg < 1)
            {
                scanProg += scanSpeed;
            }
            else if(scanProg >= 1)
            {
                scanProg = -1; //Reset scanner

                for(int i = 0;i < numberOfScansSpots;i++) //Generate a new scan spot inside the bounds
                {
                    scanCoords.Add((Random.Range(10, bounds.xBound), Random.Range(10, bounds.yBound))); //Spawn in a new scanspot at random coordinates dictated by the boundaries of the map
                }
                SetScannerState("finished");
                scanSpotsAreAvailable = true;

            }

            if (currentTextPos != -1) //Setting UI update text
            {
                if(currentTextPos == 0)
                {
                    SetScannerState("idle");
                }
                currentTextPos += 4 ;
                if (currentTextPos >= UItext.Length)
                {
                    currentTextPos = UItext.Length;
                }
            }

            //MUST BE LAST IN QUEUE
            secsSinceLastUpdate = 0;
        }
    }

    public static void AddNewAlien() //Starts a new scan - triggered by pressing one of the scan spots on the map
    {

    }
    
    public void SetScannerState(string state)
    {
        switch(state)
        {
            case "idle":
                scannerObject.GetComponent<SpriteRenderer>().sprite = scannerSpriteStates[0];
                break;
            case "scanning":
                scannerObject.GetComponent<SpriteRenderer>().sprite = scannerSpriteStates[1];
                break;
            case "finished":
                scannerObject.GetComponent<SpriteRenderer>().sprite = scannerSpriteStates[2];
                break;
            default:
                Debug.LogError("Invalid scanner state");
                break;
        }
    }
}

public class Alien //The alien generated when a scanspot is selected. Information is decoded using the signal decoder
{
    public (float x, float y) coordinates; //Stores the coordinates for the alien signal
}
