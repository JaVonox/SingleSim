using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class Gameplay : MonoBehaviour
{

    public static readonly string gameVersion = "Prerelease";


    public static float scanSpeed = 0.05f; //How fast scanning occurs
    public static float decoderSpeedMultiplier = 1.0f;
    public static float scanProg = -1; //-1 for not started. locked to 0-1
    public static List<Scanspot> scanCoords = new List<Scanspot>(); //Stores available positions of scan coordinates
    public static bool scanSpotsAreAvailable = false; //Bool says if there are available scan coordinates on screen
    public static int numberOfScansSpots = 3; //how many scan spots can appear at once
    public static (float xBound, float yBound) bounds; //Stores the boundaries of the scanSet
    public static bool isBoundsSet = false;
    private float secsSinceLastUpdate = 0;

    public static Alien activeAlien; //The current alien stats loaded by the scanner into the decoder. when decoded it is loaded into storedAliens
    public static List<Alien> storedAliens = new List<Alien>(); //The previously completed alien scans placed in storage

    public static (double x, double y) UIcoordinates; //Stores the coordinates for the last completed signal for the UI popup to handle
    public static string scanUIText;
    public static int currentScanTextPos = -1;

    public static float textSpeed = 4;
    public static float textDisplayChance = 1; //Chance that the text will properly display. currently dummied, used to be 0.925f

    public static float signalReadingRange = 30; //Range at which a signal can be loaded into the decoder

    public GameObject scannerObject;
    public List<Sprite> scannerSpriteStates;
    public GameObject decoderObject;
    public List<Sprite> decoderSpriteStates;

    public List<Sprite> humanoidSpritesToLoad; //Non static field to import the sprites and load them into the static loaded
    public List<Sprite> robotSpritesToLoad; 
    public List<Sprite> cephSpritesToLoad; 
    public List<Sprite> insectSpritesToLoad; 
    public static List<Sprite> humanSprites;
    public static List<Sprite> robotSprites;
    public static List<Sprite> cephSprites;
    public static List<Sprite> insectSprites;

    public static int credits = 0;
    public static int lifetimeCredits = 0;
    public static byte tutorialState = 0;//state 0 is before first scan, state 1 is after scan, state 2 is after first decode, state 3 is after second scan, state 4 is after second decode, state 5 is after match
    public static bool tutorialStateUpdateNeeded = false; //Set to true when tutorial state changes - requesting an update of tutorial assets + emails
    private float emailDtTime = 0.0f;
    public static int lastLoadedHz = 255;
    public static int lastSentHz = 255;

    public static Dictionary<System.Type, EnumMatrix> prefComparisons = new Dictionary<System.Type, EnumMatrix>(); //Matrices for the preferences vs actual grid

    public static List<(BodyType type, string unprocessedContents, Dictionary<System.Type,string> noPrefReplacements, string selfUnemployedReplacement, string prefUnemployedReplacement)> LoadedMessages = new List<(BodyType type, string unprocessedContents, Dictionary<System.Type, string> noPrefReplacements, string selfUnemployedReplacement, string prefUnemployedReplacement)>(); //Messages sorted by body type and details filled out as appropriate
    
    public static string prevSaveName = ""; //Stores the name of the save - updates if a save is made or if the save is loaded
    public static void ResetGamestate()
    {
        scanProg = -1;
        scanCoords.Clear();
        scanSpotsAreAvailable = false;
        bounds = (0,0);
        isBoundsSet = false;
        activeAlien = null;
        storedAliens.Clear();
        UIcoordinates = (0, 0);
        scanUIText = "";
        currentScanTextPos = -1;
        //Loaded sprites does not need to be reset
        credits = 0;
        lifetimeCredits = 0;
        tutorialState = 0; //state 0 is before first scan, state 1 is after scan, state 2 is after first decode, state 3 is after second decode, state 4 is after match
        tutorialStateUpdateNeeded = true;
        lastLoadedHz = 255;
        lastSentHz = 255;
        prevSaveName = "";
        //Pref Comparisons does not need to be reset
        //Loaded messages does not need to be reset
        ScannerControls.currentState = ScanState.IdleConsole;

        for (int i = 0; i < shopItems.Count; i++) //Reset shop items
        {
            shopItems[i] = (shopItems[i].name, shopItems[i].baseValue, shopItems[i].incrementValue, 0, shopItems[i].upgradeCost);
        }

        ReloadShopVars();


        isSetup = false;
        scannerState = "idle";
        decoderState = "idle";

        AudioHandler.Setup();
        LaptopHandler.emails.Clear();

    }
    public static void HandleSaveLoad(string path)
    {
        ResetGamestate();
        FileLoading.LoadSave(path);

        //Append image methods
        if(activeAlien != null) { activeAlien.retImageMethod = ReturnImage; }
        if(storedAliens.Count > 0)
        {
            for(int i = 0;i < storedAliens.Count;i++)
            {
                storedAliens[i].retImageMethod = ReturnImage;
            }
        }
        //Set scanner/decoder monitor screens


    }
    // Start is called before the first frame update
    void Start()
    {

        prefComparisons[typeof(BodyType)] = new EnumMatrix(typeof(BodyType));
        prefComparisons[typeof(AgeType)] = new EnumMatrix(typeof(AgeType));
        prefComparisons[typeof(OccupationType)] = new EnumMatrix(typeof(OccupationType));
        prefComparisons[typeof(GoalsType)] = new EnumMatrix(typeof(GoalsType));

        LoadedMessages = FileLoading.GetMessages();
        //Load alien sprites into static space to allow access from methods
        humanSprites = humanoidSpritesToLoad.GetRange(0, humanoidSpritesToLoad.Count);
        robotSprites = robotSpritesToLoad.GetRange(0, robotSpritesToLoad.Count);
        cephSprites = cephSpritesToLoad.GetRange(0, cephSpritesToLoad.Count);
        insectSprites = insectSpritesToLoad.GetRange(0, insectSpritesToLoad.Count);

        humanoidSpritesToLoad.Clear();
        robotSpritesToLoad.Clear();
        cephSpritesToLoad.Clear();
        insectSpritesToLoad.Clear();

        if (!PlayerPrefs.HasKey("sensitivity")) { PlayerPrefs.SetFloat("sensitivity", 0.5f); }
        if (!PlayerPrefs.HasKey("volume")) { PlayerPrefs.SetFloat("volume", 1.0f); }

        Movement.volume = PlayerPrefs.GetFloat("volume");
        Movement.sensitivity = PlayerPrefs.GetFloat("sensitivity");

        Setup();
    }

    public static bool isSetup = false;
    public static void Setup()
    {
        if(prevSaveName == "") { ResetGamestate(); }
        Movement.EnterMovementState();
        isSetup = true;
    }
    // Update is called once per frame

    void HandleTutorialStateChange()
    {
        tutorialStateUpdateNeeded = false;
        switch (tutorialState)
        {
            case 0:
                LaptopHandler.emailQueue.Enqueue(new Email("Y.Hans441@Internal.yc",
                    "Welcome to the department!",
                    "Dr. Wester! It's been far too long since we've last talked!\n\nAs department head, I'd like to formally welcome you to the Signals dept. I'm sure I dont have to explain to you what the purpose of our"+
                    " work here is, you've worked here longer than I have after all, nevertheless, it is my responsibility to instruct new transfers on how to work the machinery - so we'll run through a quick tutorial," +
                    " I'll be remoting in and watching your progress.\n\n" +
                    "Assuming installations did their job correctly, left of the server rack should be your scanner console - we use this to download client signals. Press the" +
                    " 'perform scan' button to load some signals onto the console, you should see some immediately appear on the system. Most likely, you'll find these signals are outside the frequency catchment range, so you'll have" +
                    " to keep scanning at different frequencies until you find the right one. You'll know when you're getting close when a signal starts to get greener. When the signal enters your search range you can click it to load it into the decoder."
                    ));
                break;
            case 1:
                LaptopHandler.emailQueue.Enqueue(new Email("Y.Hans441@Internal.yc",
                    "Using the decoder",
                    "Alright, the signal now should be loaded into the decoder.\n\nYou can find the decoder to the left of the scanner. We use this machine to process the raw signal data into client profiles. This part is easy but it takes" +
                    " a little bit of time. Simply press the 'Begin Decoding' button and wait as the system processes the signal.\n\nAfter some time it'll print the profile data and allow you to upload it into your personal signal database."
                    ));
                break;
            case 2:
                LaptopHandler.emailQueue.Enqueue(new Email("Y.Hans441@Internal.yc",
                    "Finding a match",
                    "Customer satisfaction dept. says we arent allowed to judge clients even in private emails anymore, so i'll bite my tongue and not say that this guy seems a little desperate. Next all we need to do is find him a partner.\n\n"+
                    " All this entails is loading up the scanner again and searching for a new signal - same as before."
                    ));
                break;
            case 3:
                LaptopHandler.emailQueue.Enqueue(new Email("Y.Hans441@Internal.yc",
                    "Decoding the match signal",
                    "Okay, now just boot up the decoder again and let it decode the new signal.\n\nAlso, you probably noticed by now but you can see any signals you've downloaded in the profiles screen. We'll get back to that in a moment but for now just"+
                    " wait for the decoder to finish running and upload the signal like before.\n\n"
                    +"If you're feeling like doing something while you wait, you can start searching for another signal in the meantime. I've disabled signal replacing for now though so you wont be able to load in a new signal until this one is done."
                    ));
                break;
            case 4:
                LaptopHandler.emailQueue.Enqueue(new Email("Y.Hans441@Internal.yc",
                    "Matching",
                    "So now we get to the crux of our job - matching profiles. These two weird aliens are looking for the love of their lives and its our job to get them set up on a hot date.\n\n"+
                    "In the profiles menu you can find the two signals we downloaded, loading one up will show you the message details again. " +
                    "highlighted in <color=#e7d112>yellow</color> you can see how the alien has described themselves, whereas in <color=#A60EB8>purple</color> you can see what theyre looking for.\n"+
                    "Its our job to compare these different profiles and match the ones we think fit the clients needs best. You'll be rewarded on how compatable the two clients are, so make sure you look through the details carefully "+
                    "before matching two profiles up.\n\nThese two profiles seem like a pretty perfect match, so we dont need to search for new signals. Try matching them now."
                    ));
                break;
            case 5:
                LaptopHandler.emailQueue.Enqueue(new Email("Y.Hans441@Internal.yc",
                    "Thats all there is to it!",
                    "Nice job! The 'Hot date night' department should have recieved the profiles and will most likely be setting up a meetup shortly - our part of the process is done!\n\n"+
                    "When you matched the two profiles, a screen should have appeared telling you how you did and how many credits you were awarded for the task. Credits can be spent in the credit shop on your laptop to"+
                    " purchase upgrades for your devices, so it'd be wise to look through the stock carefully before wasting your credits.\n\n"+
                    "Anyway, thats everything done, you've learned everything I can teach you - just start downloading more signals and finding matches!\n\n"+
                    "I'll be watching your performance, but I have to say it's pretty odd having my old boss as an subordinate. Why did you transfer from head of frontend development anyway? Regardless, it'll be nice having you on the team, doctor!"
                    ));
                break;
            default:
                Debug.LogError("invalid tutorial state message");
                break;
        }
    }
    void Update()
    {
        if(isSetup) //When in setup state, update all non-static variables on first frame update
        {
            secsSinceLastUpdate = 0;
            LaptopConsole.FirstConsoleLoad();
            SetScannerState(scannerState);
            SetDecoderState(decoderState);
            isSetup = false;
        }

        if (tutorialState == 4 && ScannerControls.currentState != ScanState.Disabled) //If in the matching state of the tutorial, disable the scanner
        {
            ScannerControls.currentState = ScanState.Disabled;
            SetScannerState("idle");
        }
        else if(ScannerControls.currentState == ScanState.Disabled && tutorialState == 5)
        {
            ScannerControls.currentState = ScanState.IdleConsole;
            SetScannerState("idle");
        }

        float dt = Time.deltaTime;
        secsSinceLastUpdate += dt;
        emailDtTime += dt;

        if (tutorialStateUpdateNeeded) { HandleTutorialStateChange(); }

        if (emailDtTime > 2.5f) //Emails check for updates every 5 seconds
        {
            LaptopHandler.HandleEmailQueue(); //Adds from email queue
            emailDtTime = 0;
        }

        if (secsSinceLastUpdate >= 0.2f)
        {
            if (ScannerControls.currentState != ScanState.Disabled)
            {
                if (scanProg == 0)
                {
                    SetScannerState("scanning");
                }

                if (scanProg != -1 && scanProg < 1)
                {
                    scanProg += scanSpeed;
                }
                else if (scanProg >= 1)
                {
                    scanProg = -1; //Reset scanner

                    if (!scanSpotsAreAvailable) //If there are not already available scanspots, generate new ones
                    {
                        for (int i = 0; i < numberOfScansSpots; i++) //Generate a new scan spot inside the bounds
                        {
                            scanCoords.Add(new Scanspot(Random.Range(10, bounds.xBound), Random.Range(10, bounds.yBound), Random.Range(50, 950))); //Spawn in a new scanspot at random coordinates dictated by the boundaries of the map
                        }

                    }
                    SetScannerState("finished");
                    scanSpotsAreAvailable = true;

                }
                else if (currentScanTextPos != -1) //Setting UI update text
                {
                    if (currentScanTextPos == 0)
                    {
                        SetScannerState("idle");
                    }
                    currentScanTextPos += Mathf.FloorToInt(textSpeed);
                    if (currentScanTextPos > scanUIText.Length)
                    {
                        currentScanTextPos = scanUIText.Length;
                    }
                }
            }
            else //If in disabled state
            {
                SetScannerState("idle");
            }

            if (activeAlien != null && activeAlien.decoderProgress >= 0)
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

                if (activeAlien.decoderProgress >= 1)
                {
                    SetDecoderState("finished");
                    //tutorial state is updated for each decode
                    if (activeAlien.decodeTextProg == -1) { activeAlien.decodeTextProg = 1; }
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
        if (tutorialState == 1) { activeAlien = new Alien(ReturnImage, true); }
        else if (tutorialState == 3) { activeAlien = new Alien(ReturnImage, false); }
        else { activeAlien = new Alien(ReturnImage); }
    }
    public static Sprite ReturnImage(int imageID,BodyType bodyType)
    {
        switch(bodyType)
        {
            case BodyType.humanoid:
                return humanSprites[imageID];
            case BodyType.automaton:
                return robotSprites[imageID];
            case BodyType.cephalopod:
                return cephSprites[imageID];
            case BodyType.insectoid:
                return insectSprites[imageID];
            default:
                Debug.LogError("Invalid sprite type");
                return humanSprites[0];
        }
    }

    public static string scannerState = "idle";
    public static string decoderState = "idle";
    public void SetScannerState(string state)
    {
        scannerState = state;

        switch (state)
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

        AudioHandler.CallNewAudioState();
    }
    public void SetDecoderState(string state)
    {
        decoderState = state;

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

        AudioHandler.CallNewAudioState();
    }
    public static void MatchAliens(Alien sender1, Alien sender2)
    {
        int score = GetCreditScore(sender1, sender2);
        credits += score;
        lifetimeCredits += score;
            
        if(tutorialState == 4) { tutorialState = 5; tutorialStateUpdateNeeded = true; } //exit tutorial state after finishing first match
        storedAliens.Remove(sender1);
        storedAliens.Remove(sender2);
    }

    public static int GetCreditScore(Alien sender1,Alien sender2)
    {
        int creditsToAppend = 0;
        //For each of the two aliens, get 15 * the multiplier and average between the two
        creditsToAppend += Mathf.RoundToInt(GetAverageComparisonMultiplier(typeof(BodyType),sender1,sender2) * 20.0f);
        creditsToAppend += Mathf.RoundToInt(GetAverageComparisonMultiplier(typeof(AgeType), sender1, sender2) * 10.0f);
        creditsToAppend += Mathf.RoundToInt(GetAverageComparisonMultiplier(typeof(OccupationType), sender1, sender2) * 10.0f);
        creditsToAppend += Mathf.RoundToInt(GetAverageComparisonMultiplier(typeof(GoalsType), sender1, sender2) * 20.0f);

        return creditsToAppend;
    }

    public static float GetAverageComparisonMultiplier(System.Type enumType, Alien sender1, Alien sender2)
    {
        if(enumType == typeof(BodyType))
        {
            return (
            (GetPrefComparisonMultiplier(typeof(BodyType), sender1.preferenceParams.body.ToString(), sender2.selfParams.body.ToString())) +
            (GetPrefComparisonMultiplier(typeof(BodyType), sender2.preferenceParams.body.ToString(), sender1.selfParams.body.ToString()))
            ) / 2.0f;
        }
        else if(enumType == typeof(AgeType))
        {
            return (
            (GetPrefComparisonMultiplier(typeof(AgeType), sender1.preferenceParams.age.ToString(), sender2.selfParams.age.ToString())) +
            (GetPrefComparisonMultiplier(typeof(AgeType), sender2.preferenceParams.age.ToString(), sender1.selfParams.age.ToString()))
            ) / 2.0f;
        }
        else if (enumType == typeof(OccupationType))
        {
            return (
            (GetPrefComparisonMultiplier(typeof(OccupationType), sender1.preferenceParams.job.ToString(), sender2.selfParams.job.ToString())) +
            (GetPrefComparisonMultiplier(typeof(OccupationType), sender2.preferenceParams.job.ToString(), sender1.selfParams.job.ToString()))
            ) / 2.0f;
        }
        else if (enumType == typeof(GoalsType))
        {
            return (
            (GetPrefComparisonMultiplier(typeof(GoalsType), sender1.preferenceParams.relationshipGoal.ToString(), sender2.selfParams.relationshipGoal.ToString())) +
            (GetPrefComparisonMultiplier(typeof(GoalsType), sender2.preferenceParams.relationshipGoal.ToString(), sender1.selfParams.relationshipGoal.ToString()))
            ) / 2.0f;
        }
        else
        {
            Debug.LogError("Invalid enum in average comparison");
            return -1.0f;
        }
    }

    public static List<(string name, float baseValue, float incrementValue, int upgradeLevel, int upgradeCost)> shopItems = new List<(string name, float baseValue, float incrementValue, int upgradeLevel, int upgradeCost)>() //Upgrade level is from 0 to 9
    {
        ("Text Render Speed",4.0f,1.0f,0,50),
        ("Scanner Speed",0.05f,0.025f,0,40),
        ("Decoder Speed",1.0f,0.2f,0,40),
        ("Frequency Range",30.0f,5.0f,0,60),
        ("Radar Strength",3.0f,0.5f,0,60)
    };

    public static void ReloadShopVars()
    {
        textSpeed = shopItems[0].baseValue + (shopItems[0].incrementValue * shopItems[0].upgradeLevel);
        scanSpeed = shopItems[1].baseValue + (shopItems[1].incrementValue * shopItems[1].upgradeLevel);
        decoderSpeedMultiplier = shopItems[2].baseValue + (shopItems[2].incrementValue * shopItems[2].upgradeLevel);
        signalReadingRange = Mathf.FloorToInt(shopItems[3].baseValue + (shopItems[3].incrementValue * shopItems[3].upgradeLevel));
        numberOfScansSpots = Mathf.FloorToInt(shopItems[4].baseValue + (shopItems[4].incrementValue * shopItems[4].upgradeLevel));
    }
    public static void UpgradeVariable(string varName)
    {
        int activeIndex = shopItems.IndexOf(shopItems.Where(x => x.name == varName).First()); //Find the position in the list of shop items

        if (shopItems[activeIndex].upgradeLevel < 9)
        {
            shopItems[activeIndex] = (shopItems[activeIndex].name, shopItems[activeIndex].baseValue, shopItems[activeIndex].incrementValue, shopItems[activeIndex].upgradeLevel + 1, shopItems[activeIndex].upgradeCost);
            credits -= shopItems[activeIndex].upgradeCost;
        }
        else
        {

        }

        ReloadShopVars();

    }
    public static float GetItemStatistic(string varName, string statName)
    {
        int activeIndex = shopItems.IndexOf(shopItems.Where(x => x.name == varName).First()); //Find the position in the list of shop items

        switch (statName)
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
    public static float GetPrefComparisonMultiplier(System.Type enumType, string expectedValue, string actualValue)
    {
        return prefComparisons[enumType].GetComparisonValue(expectedValue, actualValue);
    }

}

public class Scanspot
{
    public float x;
    public float y;
    public int freq; //The hz frequency of the signal

    public Scanspot(float nx, float ny, int nhz)
    {
        x = nx;
        y = ny;
        freq = nhz;
    }

    public (float x, float y) GetCoordsTuple()
    {
        return (x, y);
    }

}
public class Alien //The alien generated when a scanspot is selected. Information is decoded using the signal decoder
{
    public int imageID; //The image ID 
    public System.Func<int, BodyType,Sprite> retImageMethod;
    public double decoderProgress = -1;
    public double baseDecodeSpeed = 0;
    public int decodeTextProg = -1;
    public string decodeTextMessage;

    public AlienStats selfParams; //The aliens own stats
    public AlienStats preferenceParams; //The preferred stats of an alien

    public string signalName;

    public Alien(int newImageID,double newDecodeProg,double newBaseDecodeSpeed,int newTextProg,string newMessage,AlienStats self,AlienStats pref, string SN) //For loading
    {
        imageID = newImageID;
        decoderProgress = newDecodeProg;
        baseDecodeSpeed = newBaseDecodeSpeed;
        decodeTextProg = newTextProg;
        decodeTextMessage = newMessage;
        selfParams = self;
        preferenceParams = pref;
        signalName = SN;
        //method to be updated by start
    }
    public Alien(System.Func<int, BodyType, Sprite> spriteMethod)
    {
        baseDecodeSpeed = ((double)(Random.Range(2.0f, 6.0f)) / 2000.0) * Gameplay.decoderSpeedMultiplier; //Set random speed for decoding the signal
        retImageMethod = spriteMethod; //Attach the method that returns the sprite

        selfParams = new AlienStats(this);

        //Load a random image to represent the alien
        if (selfParams.body == BodyType.humanoid) { imageID = Random.Range(0, Gameplay.humanSprites.Count); }
        else if (selfParams.body == BodyType.automaton) { imageID = Random.Range(0, Gameplay.robotSprites.Count); }
        else if (selfParams.body == BodyType.cephalopod) { imageID = Random.Range(0, Gameplay.cephSprites.Count); }
        else if (selfParams.body == BodyType.insectoid) { imageID = Random.Range(0, Gameplay.insectSprites.Count); }

        preferenceParams = new AlienStats(ref selfParams);

        decodeTextMessage = GenerateText(0);

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

    public Alien(System.Func<int, BodyType, Sprite> spriteMethod, bool tutorialAlienOne) //Generates the first two aliens a user will recieve, based on their tutorial state
    {
        baseDecodeSpeed = 0.005 * Gameplay.decoderSpeedMultiplier; //Preset speed

        retImageMethod = spriteMethod; //Attach the method that returns the sprite

        if (tutorialAlienOne) //First alien during tutorial
        {
            selfParams = new AlienStats(BodyType.humanoid,AgeType.adult,OccupationType.engineer,GoalsType.relationship);
            preferenceParams = new AlienStats(BodyType.humanoid, AgeType.adult, OccupationType.NoPref, GoalsType.relationship);
            imageID = 1;
            decodeTextMessage = GenerateText(1);
        }
        else //Second alien during tutorial
        {
            selfParams = new AlienStats(BodyType.humanoid, AgeType.adult, OccupationType.unemployed, GoalsType.fling);
            preferenceParams = new AlienStats(BodyType.humanoid, AgeType.NoPref, OccupationType.unemployed, GoalsType.fling);
            imageID = 0;
            decodeTextMessage = GenerateText(2);
        }

        signalName = System.DateTime.Now.ToString("dd/MM/yy HH:mm:ss");
    }
    public void BeginDecode()
    {
        decoderProgress = 0;
    }
    public Sprite ReturnImage()
    {
        return retImageMethod(imageID,selfParams.body);
    }
    public string GenerateText(int preset)
    {
        List<(BodyType type, string unprocessedContents, Dictionary<System.Type, string> noPrefReplacements, string selfUnemployedReplacement, string prefUnemployedReplacement)> possibleMessages = Gameplay.LoadedMessages.Where(n => n.type == selfParams.body).ToList(); //Get all body type specific messages
        int messageID = 0;
        if (preset == 0) { messageID = Random.Range(0, possibleMessages.Count); }
        else if(preset == 1) { messageID = 1; }
        else if(preset == 2) { messageID = 2; }

        string preObMessage = ProcessText(possibleMessages[messageID].unprocessedContents,possibleMessages[messageID].noPrefReplacements,possibleMessages[messageID].selfUnemployedReplacement,possibleMessages[messageID].prefUnemployedReplacement);
        string newMessage = "";

        foreach(char x in preObMessage)
        {
            if(x == ' ' || Random.Range(0.01f,1.0f) < Gameplay.textDisplayChance)
            {
                newMessage += x;
            }
            else
            {
                newMessage += ".";
            }
        }
        return newMessage;
    }

    public string GetAlienText() //Gets aliens own text to the current text progress - highlighting syntax in the progress.
    {
        string txt = decodeTextMessage.Substring(0, decodeTextProg).ToString();
        txt = txt.Replace("", "<color=#e7d112>");
        txt = txt.Replace("`", "<color=#A60EB8>");
        txt = txt.Replace("", "</color>");

        return txt;
    }

    private static readonly List<string> replacementStrings = new List<string>() { "[self_body]","[pref_body]","[self_age]","[pref_age]","[self_job]","[pref_job]","[pref_goal]"}; 
    public string ProcessText(string preprocessedText, Dictionary<System.Type, string> noPrefReplacements, string selfUnemployedReplacement, string prefUnemployedReplacement)
    {
        string editedText = preprocessedText;
        Dictionary<string, string> replacementWords = new Dictionary<string, string>();

        //string selfColorHex = "#e7d112";  character indicator
        //string prefColorHex = "#A60EB8"; ` character indicator
        //  character end

        //Add replacements for strings
        replacementWords.Add("[self_body]", "" + selfParams.body.ToString() + "");
        replacementWords.Add("[pref_body]", "`" + (preferenceParams.body == BodyType.NoPref ? noPrefReplacements[typeof(BodyType)] : preferenceParams.body.ToString()) + "");
        replacementWords.Add("[self_age]", "" + selfParams.age.ToString() + "");
        replacementWords.Add("[pref_age]", "`" +(preferenceParams.age == AgeType.NoPref ? noPrefReplacements[typeof(AgeType)] : preferenceParams.age.ToString()) + "");
        replacementWords.Add("[self_job]", "" + (selfParams.job == OccupationType.unemployed ? selfUnemployedReplacement : selfParams.job.ToString()) + "");
        replacementWords.Add("[pref_job]", "`" +(preferenceParams.job == OccupationType.unemployed ? prefUnemployedReplacement : (preferenceParams.job == OccupationType.NoPref ? noPrefReplacements[typeof(OccupationType)] : preferenceParams.job.ToString()))+ "");
        replacementWords.Add("[pref_goal]", "" + (preferenceParams.relationshipGoal == GoalsType.NoPref ? noPrefReplacements[typeof(GoalsType)] : preferenceParams.relationshipGoal.ToString()) + "");
        //pref goal should always match self goal so theres no need for it
        foreach(string match in replacementStrings) //iterate through match words and replace with the appropriate replacement
        {
            if(editedText.Contains(match))
            {
                editedText = editedText.Replace(match, replacementWords[match]);
            }
            else
            {
                Debug.LogError("missing matchcase " + match);
            }
        }
        return editedText;
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

    public AlienStats(BodyType setBType, AgeType setAtype, OccupationType setJtype, GoalsType setGtype) //Constructor for preset alien
    {
        body = setBType;
        age = setAtype;
        job = setJtype;
        relationshipGoal = setGtype;
    }
}
public class EnumMatrix //Kind of dissapointed with this whole section. I cant seem to find a way to do what I want in c#
{
    private float[,] compatMatrix;
    private System.Type enumType;
    public EnumMatrix(System.Type sEnumType)
    {
        int enumLength = System.Enum.GetNames(sEnumType).Length;
        compatMatrix = new float[enumLength,enumLength]; //Define the compatability matrix as sized with the two properties.
        //The first index of the matrix represents the "desired item" - the request that the client sent out
        //The second index represents the actual item of the second client
        //The value contained represents how compatable the two properties are.
        enumType = sEnumType;

        if(enumType == typeof(BodyType))
        {
            GenerateBodyMatrix();
        }
        else if (enumType == typeof(AgeType))
        {
            GenerateAgeMatrix();
        }
        else if (enumType == typeof(OccupationType))
        {
            GenerateJobMatrix();
        }
        else if (enumType == typeof(GoalsType))
        {
            GenerateGoalMatrix();
        }
        else
        {
            Debug.LogError("invalid enum type");
        }
    }

    private void GenerateBodyMatrix()
    {
        AddMatrixRow("humanoid",new List<(string typeName, float fieldValue)>(){("humanoid",1.0f),("automaton",0.5f),("cephalopod",0.4f),("insectoid",0.3f),("NoPref",0.0f)});
        AddMatrixRow("automaton", new List<(string typeName, float fieldValue)>() { ("humanoid", 0.5f), ("automaton", 1.0f), ("cephalopod", 0.3f), ("insectoid", 0.5f), ("NoPref", 0.0f) });
        AddMatrixRow("cephalopod", new List<(string typeName, float fieldValue)>() { ("humanoid", 0.7f), ("automaton", 0.3f), ("cephalopod", 1.0f), ("insectoid", 0.1f), ("NoPref", 0.0f) });
        AddMatrixRow("insectoid", new List<(string typeName, float fieldValue)>() { ("humanoid", 0.4f), ("automaton", 0.3f), ("cephalopod", 0.1f), ("insectoid", 1.0f), ("NoPref", 0.0f) });
        AddMatrixRow("NoPref", new List<(string typeName, float fieldValue)>() { ("humanoid", 0.7f), ("automaton", 0.7f), ("cephalopod", 0.7f), ("insectoid", 0.7f), ("NoPref", 0.0f) });
    }
    private void GenerateAgeMatrix()
    {
        AddMatrixRow("adult", new List<(string typeName, float fieldValue)>() { ("adult", 1.0f), ("senior", 0.3f), ("immortal", 0.1f), ("NoPref", 0.0f) });
        AddMatrixRow("senior", new List<(string typeName, float fieldValue)>() { ("adult", 0.5f), ("senior", 1.0f), ("immortal", 0.5f), ("NoPref", 0.0f) });
        AddMatrixRow("immortal", new List<(string typeName, float fieldValue)>() { ("adult", 0.2f), ("senior", 0.5f), ("immortal", 1.0f), ("NoPref", 0.0f) });
        AddMatrixRow("NoPref", new List<(string typeName, float fieldValue)>() { ("adult", 0.7f), ("senior", 0.7f), ("immortal", 0.7f), ("NoPref", 0.0f) });
    }
    private void GenerateJobMatrix()
    {
        AddMatrixRow("unemployed", new List<(string typeName, float fieldValue)>() { ("unemployed", 1.0f), ("labourer", 0.5f), ("engineer", 0.3f), ("soldier", 0.3f), ("NoPref", 0.0f) });
        AddMatrixRow("labourer", new List<(string typeName, float fieldValue)>() { ("unemployed", 0.3f), ("labourer", 1.0f), ("engineer", 0.5f), ("soldier", 0.5f), ("NoPref", 0.0f) });
        AddMatrixRow("engineer", new List<(string typeName, float fieldValue)>() { ("unemployed", 0.1f), ("labourer", 0.3f), ("engineer", 1.0f), ("soldier", 0.3f), ("NoPref", 0.0f) });
        AddMatrixRow("soldier", new List<(string typeName, float fieldValue)>() { ("unemployed", 0.3f), ("labourer", 0.4f), ("engineer", 0.4f), ("soldier", 1.0f), ("NoPref", 0.0f) });
        AddMatrixRow("NoPref", new List<(string typeName, float fieldValue)>() { ("unemployed", 0.7f), ("labourer", 0.7f), ("engineer", 0.7f), ("soldier", 0.7f), ("NoPref", 0.0f) });
    }
    private void GenerateGoalMatrix()
    {
        AddMatrixRow("fling", new List<(string typeName, float fieldValue)>() { ("fling", 1.0f), ("relationship", 0.4f), ("marriage", 0.1f), ("deathbond", 0.1f), ("NoPref", 0.7f) });
        AddMatrixRow("relationship", new List<(string typeName, float fieldValue)>() { ("fling", 0.3f), ("relationship", 1.0f), ("marriage", 0.4f), ("deathbond", 0.1f), ("NoPref", 0.7f) });
        AddMatrixRow("marriage", new List<(string typeName, float fieldValue)>() { ("fling", 0.2f), ("relationship", 0.45f), ("marriage", 1.0f), ("deathbond", 0.4f), ("NoPref", 0.7f) });
        AddMatrixRow("deathbond", new List<(string typeName, float fieldValue)>() { ("fling", 0.1f), ("relationship", 0.3f), ("marriage", 0.45f), ("deathbond", 1.0f), ("NoPref", 0.7f) });
        AddMatrixRow("NoPref", new List<(string typeName, float fieldValue)>() { ("fling", 0.7f), ("relationship", 0.7f), ("marriage", 0.7f), ("deathbond", 0.7f), ("NoPref", 0.7f) });
    }
    private void AddMatrixRow(string rowName, List<(string typeName, float fieldValue)> input)
    {
        int rowIndex = (int)System.Convert.ChangeType(System.Enum.Parse(enumType, rowName),enumType); //Gets the index of the enumType in the compatMatrix

        for (int i = 0; i < input.Count; i++)
        {
            int itemRowIndex = (int)System.Convert.ChangeType(System.Enum.Parse(enumType, input[i].typeName), enumType); //Gets the index of the enumType in the compatMatrix
            compatMatrix[rowIndex, itemRowIndex] = input[i].fieldValue; //Appends the value into the compatability matrix
        }

    }
    public float GetComparisonValue(string expectedValue, string actualValue)
    {
        int expectedRowIndex = (int)System.Convert.ChangeType(System.Enum.Parse(enumType, expectedValue), enumType);
        int actualValueRowIndex = (int)System.Convert.ChangeType(System.Enum.Parse(enumType, actualValue), enumType);

        return compatMatrix[expectedRowIndex, actualValueRowIndex];
    }
}
