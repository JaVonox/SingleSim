using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    public static Alien activeAlien; //The current alien stats loaded by the scanner into the decoder. when decoded it is loaded into storedAliens
    public static List<Alien> storedAliens = new List<Alien>(); //The previously completed alien scans placed in storage

    public static bool scannerConsolePopupEnabled = false; //Checks if the scanner console pop up should be enabled when loaded
    public static (double x, double y) UIcoordinates; //Stores the coordinates for the last completed signal for the UI popup to handle
    public static string UItext;
    public static int currentTextPos = -1;
    public static float textTime = 0;

    public GameObject scannerObject;
    public List<Sprite> scannerSpriteStates;

    public List<Sprite> alienSpritesToLoad; //Non static field to import the sprites and load them into the static loaded
    public static List<Sprite> alienSprites;

    // Start is called before the first frame update
    void Start()
    {
        //Load alien sprites into static space to allow access from methods
        alienSprites = alienSpritesToLoad.GetRange(0,alienSpritesToLoad.Count);
        alienSpritesToLoad.Clear();

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
            else if (currentTextPos != -1) //Setting UI update text
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

            if(activeAlien != null && activeAlien.decoderProgress >= 0)
            {
                if (activeAlien.decoderProgress > 1)
                {
                    activeAlien.decoderProgress = 1;
                }
                else
                {
                    activeAlien.decoderProgress += activeAlien.baseDecodeSpeed;
                }
            }
            //MUST BE LAST IN QUEUE
            secsSinceLastUpdate = 0;
        }
    }

    public static void AddNewAlien() //Starts a new scan - triggered by pressing one of the scan spots on the map
    {
        activeAlien = new Alien(ReturnImage);
    }
    public static Sprite ReturnImage(int imageID)
    {
        return alienSprites[imageID];
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
    public int imageID; //The image ID 
    public System.Func<int,Sprite> retImageMethod;
    public float decoderProgress = -1;
    public float baseDecodeSpeed = 0;
    public Alien(System.Func<int,Sprite> spriteMethod)
    {
        imageID = Random.Range(0, Gameplay.alienSprites.Count - 1); //Load a random image to represent the alien
        //baseDecodeSpeed = Random.Range(0.001f, 0.01f); //Set random speed for decoding the signal
        baseDecodeSpeed = 0.1f;
        retImageMethod = spriteMethod; //Attach the method that returns the sprite
    }

    public Alien(Alien copy)
    {
        imageID = copy.imageID;
        retImageMethod = copy.retImageMethod;
        decoderProgress = copy.decoderProgress;
        baseDecodeSpeed = copy.baseDecodeSpeed;
    }
    public void BeginDecode()
    {
        decoderProgress = 0;
    }
    public Sprite ReturnImage()
    {
        return retImageMethod(imageID);
    }
}
