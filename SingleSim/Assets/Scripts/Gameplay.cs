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
    
    public static bool scannerConsolePopupEnabled = false; //Checks if the scanner console pop up should be enabled when loadewd
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        secsSinceLastUpdate += Time.deltaTime;

        if (secsSinceLastUpdate >= 0.2f)
        {
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
                scanSpotsAreAvailable = true;

            }


            //MUST BE LAST IN QUEUE
            secsSinceLastUpdate = 0;
        }
    }

    public static void AddNewAlien() //Starts a new scan - triggered by pressing one of the scan spots on the map
    {

    }
}

public class Alien //The alien generated when a scanspot is selected. Information is decoded using the signal decoder
{

}
