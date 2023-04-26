using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class Gameplay : MonoBehaviour
{
    public static float scanSpeed = 0.05f; //How fast scanning occurs
    public static float decoderSpeedMultiplier = 1.0f;
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
    public static string ScanUItext;
    public static int currentScanTextPos = -1;
    public static float textTime = 0;

    public static float textSpeed = 4;

    public GameObject scannerObject;
    public List<Sprite> scannerSpriteStates;
    public GameObject decoderObject;
    public List<Sprite> decoderSpriteStates;

    public List<Sprite> alienSpritesToLoad; //Non static field to import the sprites and load them into the static loaded
    public static List<Sprite> alienSprites;

    public static int credits = 0;

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
            else if (currentScanTextPos != -1) //Setting UI update text
            {
                if(currentScanTextPos == 0)
                {
                    SetScannerState("idle");
                }
                currentScanTextPos += Mathf.FloorToInt(textSpeed);
                if (currentScanTextPos > ScanUItext.Length)
                {
                    currentScanTextPos = ScanUItext.Length;
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
                    SetDecoderState("decoding");
                    activeAlien.decoderProgress += activeAlien.baseDecodeSpeed;
                }

                if(activeAlien.decoderProgress >= 1)
                {
                    SetDecoderState("finished");
                    if(activeAlien.decodeTextProg == -1 ) { activeAlien.decodeTextProg = 1; }
                    else
                    {
                        activeAlien.decodeTextProg += Mathf.FloorToInt(textSpeed);

                        if (activeAlien.decodeTextProg > activeAlien.decodeTextMessage.Length)
                        {
                            activeAlien.decodeTextProg = activeAlien.decodeTextMessage.Length;
                        }
                    }
                }
            }
            else
            {
                SetDecoderState("idle");
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

    public void SetDecoderState(string state)
    {
        switch (state)
        {
            case "idle":
                decoderObject.GetComponent<SpriteRenderer>().sprite = decoderSpriteStates[0];
                break;
            case "decoding":
                decoderObject.GetComponent<SpriteRenderer>().sprite = decoderSpriteStates[1];
                break;
            case "finished":
                decoderObject.GetComponent<SpriteRenderer>().sprite = decoderSpriteStates[2];
                break;
            default:
                Debug.LogError("Invalid decoder state");
                break;
        }
    }
    public static void MatchAliens(Alien sender1, Alien sender2)
    {
        storedAliens.Remove(sender1);
        storedAliens.Remove(sender2);

        credits += 10; //Calculate credits using matrix method in the future
    }

    public static List<(string name, float baseValue, float incrementValue, int upgradeLevel, int upgradeCost)> shopItems = new List<(string name, float baseValue, float incrementValue, int upgradeLevel, int upgradeCost)>() //Upgrade level is from 0 to 9
    {
        ("Text Render Speed",4.0f,1.0f,0,50),
        ("Scanner Efficiency",0.05f,0.025f,0,20),
        ("Decoder Efficiency",1.0f,0.1f,0,20)
    };
    public static void UpgradeVariable(string varName)
    {
        int activeIndex = shopItems.IndexOf(shopItems.Where(x => x.name == varName).First()); //Find the position in the list of shop items

        if (shopItems[activeIndex].upgradeLevel < 9)
        {
            shopItems[activeIndex] = (shopItems[activeIndex].name,shopItems[activeIndex].baseValue, shopItems[activeIndex].incrementValue, shopItems[activeIndex].upgradeLevel + 1, shopItems[activeIndex].upgradeCost);
            credits -= shopItems[activeIndex].upgradeCost;
            switch (varName) //Select variable. It'd be nice to do this with a referenced variable in the shop items list but apparently c# doesnt enjoy that
            {
                case "Text Render Speed":
                    textSpeed = shopItems[activeIndex].baseValue + (shopItems[activeIndex].incrementValue * shopItems[activeIndex].upgradeLevel);
                    break;
                case "Scanner Efficiency":
                    scanSpeed = shopItems[activeIndex].baseValue + (shopItems[activeIndex].incrementValue * shopItems[activeIndex].upgradeLevel);
                    break;
                case "Decoder Efficiency":
                    decoderSpeedMultiplier = shopItems[activeIndex].baseValue + (shopItems[activeIndex].incrementValue * shopItems[activeIndex].upgradeLevel);
                    break;
                default:
                    Debug.LogError("Invalid shop");
                    break;
            }
        }
        else
        {

        }

    }
    public static float GetItemStatistic(string varName, string statName)
    {
        int activeIndex = shopItems.IndexOf(shopItems.Where(x => x.name == varName).First()); //Find the position in the list of shop items

        switch(statName)
        {
            case "Level":
                return (float)shopItems[activeIndex].upgradeLevel;
            case "UpgradeCost":
                return (float)shopItems[activeIndex].upgradeCost;
            default:
                Debug.LogError("Invalid item statistic name");
                return -1.0f;
        }

    }

}

public class Alien //The alien generated when a scanspot is selected. Information is decoded using the signal decoder
{
    public int imageID; //The image ID 
    public System.Func<int,Sprite> retImageMethod;
    public float decoderProgress = -1;
    public float baseDecodeSpeed = 0;
    public int decodeTextProg = -1;
    public string decodeTextMessage;

    public AlienStats selfParams; //The aliens own stats
    public AlienStats preferenceParams; //The preferred stats of an alien

    public string signalName;
    public Alien(System.Func<int,Sprite> spriteMethod)
    {
        imageID = Random.Range(0, Gameplay.alienSprites.Count); //Load a random image to represent the alien
        baseDecodeSpeed = Random.Range(0.001f, 0.01f) * Gameplay.decoderSpeedMultiplier; //Set random speed for decoding the signal
        retImageMethod = spriteMethod; //Attach the method that returns the sprite

        selfParams = new AlienStats(this);
        preferenceParams = new AlienStats(ref selfParams);

        decodeTextMessage = GenerateText();

        signalName = System.DateTime.Now.ToString("dd/MM/yy HH:mm:ss");
    }

    public Alien(Alien copy)
    {
        imageID = copy.imageID;
        retImageMethod = copy.retImageMethod;
        decoderProgress = copy.decoderProgress;
        baseDecodeSpeed = copy.baseDecodeSpeed;
        decodeTextProg = copy.decodeTextProg;
        decodeTextMessage = copy.decodeTextMessage;

        selfParams = copy.selfParams;
        preferenceParams = copy.preferenceParams;
        signalName = copy.signalName;
    }
    public void BeginDecode()
    {
        decoderProgress = 0;
    }
    public Sprite ReturnImage()
    {
        return retImageMethod(imageID);
    }
    public string GenerateText()
    {
        return "Hi, im just a simple " + (selfParams.age) + " " + (selfParams.body) 
            + " looking for a " + (preferenceParams.body == BodyType.NoPref ? "like minded person " : preferenceParams.body + " ") + "to spend some alone time with. " +
            "I currently " + (selfParams.job == OccupationType.unemployed ? "am in search of a job" : "work as a " + selfParams.job) +
            ". Ideally, id like to meet a " + (preferenceParams.age == AgeType.NoPref ? "" : preferenceParams.age + " " ) +
            (preferenceParams.job == OccupationType.NoPref ? "loving partner" : (preferenceParams.job == OccupationType.unemployed ? "with a lot of free time" : preferenceParams.job +"")) +
            " interested in " + (selfParams.relationshipGoal == GoalsType.NoPref ? "seeing how far things go." : "a " + selfParams.relationshipGoal + ".");
    }
}

//Enums for various alien typings - used for both an aliens own profile and their preferences
//No pref is always the last value for two reasons - to stop it being generated as an alien value and also to dictate what the enum length is
public enum BodyType
{
    humanoid,
    automaton,
    cephalopod,
    insectoid,
    NoPref
}
public enum AgeType
{
    adult,
    senior,
    immortal,
    NoPref
}

public enum OccupationType
{
    unemployed,
    labourer,
    engineer,
    soldier,
    NoPref
}

public enum GoalsType
{
    fling,
    relationship,
    partnership,
    marriage,
    deathbond,
    NoPref
}
public class AlienStats
{
    public BodyType body;
    public AgeType age;
    public OccupationType job;
    public GoalsType relationshipGoal;
    public AlienStats(Alien self) //Constructor for generating a new aliens preferences
    {
        body = (BodyType)(Random.Range(0, (int)(BodyType.NoPref)));
        age = (AgeType)(Random.Range(0, (int)(AgeType.NoPref)));
        job = (OccupationType)(Random.Range(0, (int)(OccupationType.NoPref)));
        relationshipGoal = (GoalsType)(Random.Range(0, (int)(GoalsType.NoPref) + 1)); //goal type may include no pref
    }

    public AlienStats(ref AlienStats self) //Constructor for generating an aliens prefrences
    {   
        byte randomGen = (byte)Random.Range(0,256); //make a random byte - each 1 represents a possible change in values and each 0 a same value. this gives a 50/50 chance of wanting to keep the same value
        //Properties are ordered in right to left, with relationship goal excluded as it will always be the same

        relationshipGoal = self.relationshipGoal;

        if ((randomGen & (byte)1) == (byte)1) { body =(BodyType)(Random.Range(0, (int)(BodyType.NoPref) + 1)); }
        else { body = self.body; }

        if ((randomGen & (byte)2) == (byte)2) { age = (AgeType)(Random.Range(0, (int)(AgeType.NoPref) + 1)); }
        else { age = self.age; }

        if ((randomGen & (byte)4) == (byte)4) { job = (OccupationType)(Random.Range(0, (int)(OccupationType.NoPref) + 1)); }
        else { job = self.job; }


    }
}