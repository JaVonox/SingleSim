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
    public static string scanUIText;
    public static int currentScanTextPos = -1;
    public static float textTime = 0;

    public static float textSpeed = 4;
    public static float textDisplayChance = 0.925f; //Chance that the text will properly display

    public GameObject scannerObject;
    public List<Sprite> scannerSpriteStates;
    public GameObject decoderObject;
    public List<Sprite> decoderSpriteStates;

    public List<Sprite> alienSpritesToLoad; //Non static field to import the sprites and load them into the static loaded
    public static List<Sprite> alienSprites;

    public static int credits = 0;

    public static Dictionary<System.Type, EnumMatrix> prefComparisons = new Dictionary<System.Type, EnumMatrix>(); //Matrices for the preferences vs actual grid

    public static List<(BodyType type, string unprocessedContents, Dictionary<System.Type,string> noPrefReplacements, string selfUnemployedReplacement, string prefUnemployedReplacement)> LoadedMessages = new List<(BodyType type, string unprocessedContents, Dictionary<System.Type, string> noPrefReplacements, string selfUnemployedReplacement, string prefUnemployedReplacement)>(); //Messages sorted by body type and details filled out as appropriate
    // Start is called before the first frame update
    void Start()
    {
        prefComparisons[typeof(BodyType)] = new EnumMatrix(typeof(BodyType));
        prefComparisons[typeof(AgeType)] = new EnumMatrix(typeof(AgeType));
        prefComparisons[typeof(OccupationType)] = new EnumMatrix(typeof(OccupationType));
        prefComparisons[typeof(GoalsType)] = new EnumMatrix(typeof(GoalsType));

        LoadedMessages = FileLoading.GetMessages();
        //Load alien sprites into static space to allow access from methods
        alienSprites = alienSpritesToLoad.GetRange(0, alienSpritesToLoad.Count);
        alienSpritesToLoad.Clear();

        LaptopConsole.FirstConsoleLoad();
        SetScannerState("idle");
    }

    // Update is called once per frame
    void Update()
    {
        secsSinceLastUpdate += Time.deltaTime;

        if (secsSinceLastUpdate >= 0.2f)
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

                for (int i = 0; i < numberOfScansSpots; i++) //Generate a new scan spot inside the bounds
                {
                    scanCoords.Add((Random.Range(10, bounds.xBound), Random.Range(10, bounds.yBound))); //Spawn in a new scanspot at random coordinates dictated by the boundaries of the map
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
        activeAlien = new Alien(ReturnImage);
    }
    public static Sprite ReturnImage(int imageID)
    {
        return alienSprites[imageID];
    }
    public void SetScannerState(string state)
    {
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
        credits += GetCreditScore(sender1,sender2);

        storedAliens.Remove(sender1);
        storedAliens.Remove(sender2);
    }

    public static int GetCreditScore(Alien sender1,Alien sender2)
    {
        int creditsToAppend = 0;
        //For each of the two aliens, get 10 * the multiplier and average between the two
        creditsToAppend += Mathf.RoundToInt(GetAverageComparisonMultiplier(typeof(BodyType),sender1,sender2) * 15.0f);
        creditsToAppend += Mathf.RoundToInt(GetAverageComparisonMultiplier(typeof(AgeType), sender1, sender2) * 15.0f);
        creditsToAppend += Mathf.RoundToInt(GetAverageComparisonMultiplier(typeof(OccupationType), sender1, sender2) * 15.0f);
        creditsToAppend += Mathf.RoundToInt(GetAverageComparisonMultiplier(typeof(GoalsType), sender1, sender2) * 15.0f);

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
        ("Scanner Efficiency",0.05f,0.025f,0,20),
        ("Decoder Efficiency",1.0f,0.2f,0,20),
        ("Signal Accuracy",0.925f,0.008f,0,10)
    };
    public static void UpgradeVariable(string varName)
    {
        int activeIndex = shopItems.IndexOf(shopItems.Where(x => x.name == varName).First()); //Find the position in the list of shop items

        if (shopItems[activeIndex].upgradeLevel < 9)
        {
            shopItems[activeIndex] = (shopItems[activeIndex].name, shopItems[activeIndex].baseValue, shopItems[activeIndex].incrementValue, shopItems[activeIndex].upgradeLevel + 1, shopItems[activeIndex].upgradeCost);
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
                case "Signal Accuracy":
                    textDisplayChance = shopItems[activeIndex].baseValue + (shopItems[activeIndex].incrementValue * shopItems[activeIndex].upgradeLevel);
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
        List<(BodyType type, string unprocessedContents, Dictionary<System.Type, string> noPrefReplacements, string selfUnemployedReplacement, string prefUnemployedReplacement)> possibleMessages = Gameplay.LoadedMessages.Where(n => n.type == selfParams.body).ToList(); //Get all body type specific messages
        int messageID = Random.Range(0, possibleMessages.Count); //Get a random message body
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

    private static readonly List<string> replacementStrings = new List<string>() { "[self_body]","[pref_body]","[self_age]","[pref_age]","[self_job]","[pref_job]","[pref_goal]"}; 
    public string ProcessText(string preprocessedText, Dictionary<System.Type, string> noPrefReplacements, string selfUnemployedReplacement, string prefUnemployedReplacement)
    {
        string editedText = preprocessedText;
        Dictionary<string, string> replacementWords = new Dictionary<string, string>();

        //Add replacements for strings
        replacementWords.Add("[self_body]", selfParams.body.ToString());
        replacementWords.Add("[pref_body]", preferenceParams.body == BodyType.NoPref ? noPrefReplacements[typeof(BodyType)] : preferenceParams.body.ToString());
        replacementWords.Add("[self_age]", selfParams.age.ToString());
        replacementWords.Add("[pref_age]", preferenceParams.age == AgeType.NoPref ? noPrefReplacements[typeof(AgeType)] : preferenceParams.age.ToString());
        replacementWords.Add("[self_job]", selfParams.job == OccupationType.unemployed ? selfUnemployedReplacement : selfParams.job.ToString());
        replacementWords.Add("[pref_job]", preferenceParams.job == OccupationType.unemployed ? prefUnemployedReplacement : (preferenceParams.job == OccupationType.NoPref ? noPrefReplacements[typeof(OccupationType)] : preferenceParams.job.ToString()));
        replacementWords.Add("[pref_goal]", preferenceParams.relationshipGoal == GoalsType.NoPref ? noPrefReplacements[typeof(GoalsType)] : preferenceParams.relationshipGoal.ToString());
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
