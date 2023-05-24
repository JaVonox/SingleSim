using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
enum LaptopModes
{
    Profiles,
    Specific,
    Shop,
    Console,
    Review,
    Email,
    SpecificEmail,
}
public class LaptopHandler : MonoBehaviour
{
    public Button profilesMode;
    public Button shopMode;
    public Button consoleMode;
    public Button emailMode;
    public Text creditsText;

    public GameObject profileSmallPrefab;
    public GameObject profilesContainer;

    public GameObject shopTab;
    public GameObject shopItemPrefab;
    public GameObject shopItemContainer;


    public GameObject profilesTab;
    public GameObject specificProfileTab;

    public GameObject consoleTab;
    public TMPro.TMP_InputField consoleInput;
    public TextMeshProUGUI consoleText;

    public GameObject reviewTab;

    public static List<Email> emails = new List<Email>(); //List of emails to load in.
    public static Queue<Email> emailQueue = new Queue<Email>(); //Queue of emails to be appended at update times
    int lastEmailCount = 0;

    public GameObject emailTab;
    public GameObject emailItemPrefab;
    public GameObject emailsContainer;

    public GameObject specificEmailTab;
    public Button returnEmailsSpecific;
    public TextMeshProUGUI emailSender;
    public TextMeshProUGUI emailContents;
    public GameObject emailScrollRect;

    private Alien comparitorAlien;
    private LaptopModes currentMode;

    private float dtTime;
    private bool deleteIsEnabled = false; //This prevents the deleting of profiles until the user completes the tutorial
    void Start()
    {
        creditsText.text = "Credits: " + Gameplay.credits;
        profilesMode.onClick.AddListener(() => SwitchMode("profilesMode"));
        shopMode.onClick.AddListener(() => SwitchMode("shopMode"));
        consoleMode.onClick.AddListener(() => SwitchMode("consoleMode"));
        emailMode.onClick.AddListener(() => SwitchMode("emailMode"));

        if (Gameplay.storyState < 6)
        {
            if (Gameplay.storyState == 0) { Gameplay.storyState = 1; Gameplay.tutorialStateUpdateNeeded = true; }
            SwitchMode("emailMode");
        }
        else { SwitchMode("profilesMode"); }
    }
    void SwitchUI() //Apply UI Changes
    {
        if (profilesContainer.transform.childCount > 0) //Unload all profiles
        {
            foreach (Transform child in profilesContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }

        if (shopItemContainer.transform.childCount > 0) //Unload all shop items
        {
            foreach (Transform child in shopItemContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }

        if (emailsContainer.transform.childCount > 0) //Unload all shop items
        {
            foreach (Transform child in emailsContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }

        switch (currentMode)
        {
            case LaptopModes.Profiles:
                profilesMode.interactable = false;
                shopMode.interactable = true;
                consoleMode.interactable = true;
                emailMode.interactable = true;
                profilesTab.SetActive(true);
                specificProfileTab.SetActive(false);
                shopTab.SetActive(false);
                reviewTab.SetActive(false);
                consoleTab.SetActive(false);
                emailTab.SetActive(false);
                specificEmailTab.SetActive(false);
                LoadProfiles();
                break;
            case LaptopModes.Specific:
                profilesMode.interactable = true;
                shopMode.interactable = true;
                consoleMode.interactable = true;
                emailMode.interactable = true;
                profilesTab.SetActive(false);
                specificProfileTab.SetActive(true);
                shopTab.SetActive(false);
                reviewTab.SetActive(false);
                consoleTab.SetActive(false);
                emailTab.SetActive(false);
                specificEmailTab.SetActive(false);
                break;
            case LaptopModes.Shop:
                profilesMode.interactable = true;
                shopMode.interactable = false;
                consoleMode.interactable = true;
                emailMode.interactable = true;
                profilesTab.SetActive(false);
                specificProfileTab.SetActive(false);
                shopTab.SetActive(true);
                reviewTab.SetActive(false);
                consoleTab.SetActive(false);
                emailTab.SetActive(false);
                specificEmailTab.SetActive(false);
                LoadShop();
                break;
            case LaptopModes.Console:
                profilesMode.interactable = true;
                shopMode.interactable = true;
                consoleMode.interactable = false;
                emailMode.interactable = true;
                profilesTab.SetActive(false);
                specificProfileTab.SetActive(false);
                shopTab.SetActive(false);
                reviewTab.SetActive(false);
                consoleTab.SetActive(true);
                emailTab.SetActive(false);
                specificEmailTab.SetActive(false);
                LaptopConsole.ReloadConsole(ref consoleText); //Reload the console data
                consoleInput.text = "";
                break;
            case LaptopModes.Review:
                profilesMode.interactable = true;
                shopMode.interactable = true;
                consoleMode.interactable = true;
                emailMode.interactable = true;
                profilesTab.SetActive(false);
                specificProfileTab.SetActive(false);
                shopTab.SetActive(false);
                reviewTab.SetActive(true);
                consoleTab.SetActive(false);
                emailTab.SetActive(false);
                specificEmailTab.SetActive(false);
                break;
            case LaptopModes.Email:
                profilesMode.interactable = true;
                shopMode.interactable = true;
                consoleMode.interactable = true;
                emailMode.interactable = false;
                profilesTab.SetActive(false);
                specificProfileTab.SetActive(false);
                shopTab.SetActive(false);
                reviewTab.SetActive(false);
                consoleTab.SetActive(false);
                emailTab.SetActive(true);
                specificEmailTab.SetActive(false);
                LoadEmails();
                break;
            case LaptopModes.SpecificEmail:
                profilesMode.interactable = true;
                shopMode.interactable = true;
                consoleMode.interactable = true;
                emailMode.interactable = false;
                profilesTab.SetActive(false);
                specificProfileTab.SetActive(false);
                shopTab.SetActive(false);
                reviewTab.SetActive(false);
                consoleTab.SetActive(false);
                emailTab.SetActive(false);
                specificEmailTab.SetActive(true);
                break;
            default:
                Debug.LogError("Invalid laptop tab");
                break;
        }
    }
    void SwitchMode(string sender) //Switch between modes based on button presses
    {
        switch (sender)
        {
            case "profilesMode":
                currentMode = LaptopModes.Profiles;
                break;
            case "shopMode":
                currentMode = LaptopModes.Shop;
                comparitorAlien = null;
                break;
            case "consoleMode":
                currentMode = LaptopModes.Console;
                comparitorAlien = null;
                break;
            case "specificMode":
                currentMode = LaptopModes.Specific;
                break;
            case "reviewMode":
                currentMode = LaptopModes.Review;
                break;
            case "emailMode":
                currentMode = LaptopModes.Email;
                comparitorAlien = null;
                break;
            case "specificEmailMode":
                currentMode = LaptopModes.SpecificEmail;
                comparitorAlien = null;
                break;
            default:
                Debug.LogError("Invalid laptop tab");
                break;
        }

        SwitchUI();
    }
    void LoadProfiles() //Iterate through each alien in the stored aliens list and spawn a tab marker for each
    {
        if (Gameplay.storedAliens.Count > 0)
        {
            for (int i = 0; i < Gameplay.storedAliens.Count; i++)
            {
                GameObject profile = Instantiate(profileSmallPrefab, profilesContainer.transform, false);

                RectTransform pfrt = (RectTransform)profilesContainer.transform; //profile container rect

                float width = (((RectTransform)profilesTab.transform).rect.width - 40) / 3.0f;
                float height = (((RectTransform)profilesTab.transform).rect.height) / 2.0f;

                profile.name = "Profile" + i;
                RectTransform rt = profile.GetComponentInChildren<RectTransform>(); //profile rect

                rt.localPosition = new Vector3(20 + (width * ((i % 3))), -(height * (((int)Mathf.Ceil(i / 3)))), 0);
                pfrt.sizeDelta = new Vector2(pfrt.sizeDelta.x, 50 + (Mathf.Ceil((i + 1) / 3.0f) * height));

                Transform profileBox = profile.transform.Find("ProfileBox");

                profileBox.Find("ProfileImage").GetComponent<Image>().sprite = Gameplay.storedAliens[i].ReturnImage(); //Set the image as the alien image
                profileBox.Find("DetailsPanel").Find("SignalName").GetComponent<Text>().text = Gameplay.storedAliens[i].signalName;
                profileBox.Find("DetailsPanel").Find("MatchDetails").GetComponent<TextMeshProUGUI>().text = GenerateProfileQuickText(Gameplay.storedAliens[i], comparitorAlien); //Loads up a cursory textblock to display self data
                Alien refAlien = Gameplay.storedAliens[i];

                if (comparitorAlien != null && comparitorAlien == Gameplay.storedAliens[i]) //Check for if the profile is the loaded comparison profile
                {
                    profileBox.GetComponent<Image>().color = new Color(0.05f, 0.45f, 0.72f);
                    profileBox.Find("LoadProfile").GetComponent<Button>().onClick.RemoveAllListeners();
                    profileBox.Find("LoadProfile").GetComponent<Button>().onClick.AddListener(() => ExitComparisonMode());
                    profileBox.Find("LoadProfile").GetComponentInChildren<Text>().text = "Stop Comparing";
                    profileBox.Find("LoadProfile").GetComponentInChildren<Text>().color = new Color(0.72f, 0.05f, 0.12f);
                }
                else
                {
                    profileBox.GetComponent<Image>().color = new Color(0.58f, 0.58f, 0.58f);
                    profileBox.Find("LoadProfile").GetComponent<Button>().onClick.RemoveAllListeners();
                    profileBox.Find("LoadProfile").GetComponent<Button>().onClick.AddListener(() => LoadSpecificProfile(refAlien));
                    profileBox.Find("LoadProfile").GetComponentInChildren<Text>().text = "Load Profile";
                    profileBox.Find("LoadProfile").GetComponentInChildren<Text>().color = new Color(0.12f, 0.72f, 0.05f);
                }
            }
        }

    }
    string GenerateProfileQuickText(Alien targetAlien, Alien? comparitorAlien)
    {
        string profileTextBody = "";
        string profileTextAge = "";
        string profileTextJob = "";
        string profileTextGoal = "";

        profileTextBody = targetAlien.selfParams.body.ToString();
        profileTextAge = targetAlien.selfParams.age.ToString();
        profileTextJob = targetAlien.selfParams.job.ToString();
        profileTextGoal = (targetAlien.selfParams.relationshipGoal == GoalsType.NoPref ? "no goal" :targetAlien.selfParams.relationshipGoal.ToString());

        if (comparitorAlien == null) { profileTextBody = "<color=#E7D112>" + profileTextBody; profileTextGoal = profileTextGoal + "</color>"; }
        else if(comparitorAlien != targetAlien) //If there is a comparison alien, highlight the matching properties
        {
            profileTextBody = "<color=" + GetScoringName(Gameplay.GetPrefComparisonMultiplier(typeof(BodyType), comparitorAlien.preferenceParams.body.ToString(), targetAlien.selfParams.body.ToString()), 0.2f).color + ">" + targetAlien.selfParams.body.ToString() + "</color>";
            profileTextAge = "<color=" + GetScoringName(Gameplay.GetPrefComparisonMultiplier(typeof(AgeType), comparitorAlien.preferenceParams.age.ToString(), targetAlien.selfParams.age.ToString()), 0.2f).color + ">" + targetAlien.selfParams.age.ToString() + "</color>";
            profileTextJob = "<color=" + GetScoringName(Gameplay.GetPrefComparisonMultiplier(typeof(OccupationType), comparitorAlien.preferenceParams.job.ToString(), targetAlien.selfParams.job.ToString()), 0.2f).color + ">" + targetAlien.selfParams.job.ToString() + "</color>";
            profileTextGoal = "<color=" + GetScoringName(Gameplay.GetPrefComparisonMultiplier(typeof(GoalsType), comparitorAlien.preferenceParams.relationshipGoal.ToString(), targetAlien.selfParams.relationshipGoal.ToString()), 0.2f).color + ">" + (targetAlien.selfParams.relationshipGoal == GoalsType.NoPref ? "no goal" : targetAlien.selfParams.relationshipGoal.ToString()) + "</color>";
        }
        else
        {
            return ""; //If the target and the comparison are the same, do not print the text
        }
        return profileTextBody + "\n" + profileTextAge + "\n" + profileTextJob + "\n" + profileTextGoal;
    }
    void LoadSpecificProfile(Alien alienProfile)
    {
        SwitchMode("specificMode"); //Load the specific profile

        Transform profileTransform = specificProfileTab.transform;

        profileTransform.Find("TargetPanel").Find("ProfileImage").GetComponent<Image>().sprite = alienProfile.ReturnImage();
        profileTransform.Find("TargetPanel").Find("ProfileText").GetComponent<Text>().text = alienProfile.GetAlienText();
        profileTransform.Find("ComparitorPanel").gameObject.SetActive(false);
        profileTransform.Find("ComparitorPanel").Find("ComparisonProfileImage").gameObject.SetActive(false);
        profileTransform.Find("ComparitorPanel").Find("ComparisonProfileText").gameObject.SetActive(false);
        profileTransform.Find("ControlPanel").Find("OverviewTextAlien1").gameObject.SetActive(false);
        profileTransform.Find("ControlPanel").Find("OverviewTextAlien2").gameObject.SetActive(false);

        if (comparitorAlien != null && comparitorAlien != alienProfile) //Check if the profile is loaded in comparison mode
        {
            profileTransform.Find("ControlPanel").Find("Match").gameObject.SetActive(true);
            profileTransform.Find("ControlPanel").Find("Match").GetComponentInChildren<Text>().text = "Match Users";
            profileTransform.Find("ControlPanel").Find("Match").GetComponent<Button>().onClick.RemoveAllListeners();
            profileTransform.Find("ControlPanel").Find("Match").GetComponent<Button>().onClick.AddListener(() => StartMatchup(alienProfile));
            profileTransform.Find("ControlPanel").Find("DeleteProfile").gameObject.SetActive(false);
            profileTransform.Find("ControlPanel").Find("Return").gameObject.SetActive(true);
            profileTransform.Find("ControlPanel").Find("Return").GetComponentInChildren<Text>().text = "Return";
            profileTransform.Find("ControlPanel").Find("Return").GetComponent<Button>().onClick.RemoveAllListeners();
            profileTransform.Find("ControlPanel").Find("Return").GetComponent<Button>().onClick.AddListener(() => SwitchMode("profilesMode"));

            profileTransform.Find("ComparitorPanel").gameObject.SetActive(true);
            profileTransform.Find("ComparitorPanel").Find("ComparisonProfileImage").gameObject.SetActive(true);
            profileTransform.Find("ComparitorPanel").Find("ComparisonProfileImage").GetComponent<Image>().sprite = comparitorAlien.ReturnImage();
            profileTransform.Find("ComparitorPanel").Find("ComparisonProfileText").gameObject.SetActive(true);
            profileTransform.Find("ComparitorPanel").Find("ComparisonProfileText").GetComponent<Text>().text = comparitorAlien.GetAlienText();
            profileTransform.Find("ControlPanel").Find("OverviewTextAlien1").gameObject.SetActive(true);
            profileTransform.Find("ControlPanel").Find("OverviewTextAlien1").GetComponent<Text>().text = "<size=14>Comparitor to target</size>\n" + GenerateProfileQuickText(alienProfile,comparitorAlien); 
            profileTransform.Find("ControlPanel").Find("OverviewTextAlien2").gameObject.SetActive(true);
            profileTransform.Find("ControlPanel").Find("OverviewTextAlien2").GetComponent<Text>().text = "<size=14>Target to comparitor</size>\n" + GenerateProfileQuickText(comparitorAlien, alienProfile);

        }
        else if (comparitorAlien != null && comparitorAlien == alienProfile) //This occurs if the quick comparison is active - disabling most buttons but allowing for quick access to comparison data
        {
            Debug.LogError("Old quick compare mode activated");
            profileTransform.Find("ControlPanel").Find("Match").gameObject.SetActive(false);
            profileTransform.Find("ControlPanel").Find("DeleteProfile").gameObject.SetActive(false);
            profileTransform.Find("ControlPanel").Find("Return").gameObject.SetActive(false);
        }
        else
        {
            profileTransform.Find("ControlPanel").Find("Match").gameObject.SetActive(true);
            profileTransform.Find("ControlPanel").Find("Match").GetComponentInChildren<Text>().text = "Find Match";
            profileTransform.Find("ControlPanel").Find("Match").GetComponent<Button>().onClick.RemoveAllListeners();
            profileTransform.Find("ControlPanel").Find("Match").GetComponent<Button>().onClick.AddListener(() => BeginComparisonMode(alienProfile));
            profileTransform.Find("ControlPanel").Find("Return").gameObject.SetActive(true);
            profileTransform.Find("ControlPanel").Find("Return").GetComponent<Button>().onClick.RemoveAllListeners();
            profileTransform.Find("ControlPanel").Find("Return").GetComponent<Button>().onClick.AddListener(() => SwitchMode("profilesMode"));
            profileTransform.Find("ControlPanel").Find("DeleteProfile").gameObject.SetActive(true);
        }

        if(!deleteIsEnabled)
        {
            profileTransform.Find("ControlPanel").Find("DeleteProfile").gameObject.SetActive(false);
        }
        else
        {
            profileTransform.Find("ControlPanel").Find("DeleteProfile").GetComponent<Button>().onClick.RemoveAllListeners();
            profileTransform.Find("ControlPanel").Find("DeleteProfile").GetComponent<Button>().onClick.AddListener(() => DeleteProfile(alienProfile));
        }
    }
    void DeleteProfile(Alien alienProfile)
    {
        specificProfileTab.SetActive(false); //Unload the examine profile screen to prevent errors
        Gameplay.storedAliens.Remove(alienProfile);
        SwitchMode("profilesMode");
    }
    void BeginComparisonMode(Alien alienProfile)
    {
        //An alien being loaded into memory is what signifies comparison mode is enabled
        //Comparison mode should be disabled when switching tab to anything but profiles/specific profiles
        comparitorAlien = alienProfile;
        SwitchMode("profilesMode"); //Load the specific profile
    }
    void ExitComparisonMode()
    {
        comparitorAlien = null;
        SwitchMode("profilesMode"); //Load the specific profile
    }
    void EnterQuickComparison(Alien passedAlien)
    {
        LoadSpecificProfile(comparitorAlien);
    }
    void StartMatchup(Alien selectedMatch) //Matches up the two profiles, removing them from memory and awarding some credits based on their compatability
    {
        Gameplay.MatchAliens(comparitorAlien, selectedMatch);
        SwitchMode("reviewMode");
        LoadReviewTab(comparitorAlien, selectedMatch);
        comparitorAlien = null;
    }

    private const float rPerLevel = (152.0f / 9.0f) / 255.0f;
    private const float gPerLevel = (-170.0f / 9.0f) / 255.0f;
    private const float bPerLevel = (18.0f / 9.0f) / 255.0f;
    void LoadShopItemLevels()
    {
        if (shopItemContainer.transform.childCount > 0) //Unload all shop items
        {
            foreach (Transform child in shopItemContainer.transform)
            {
                float level = Gameplay.GetItemStatistic(child.name,"Level");
                Transform shopPanel = child.Find("ShopItemPanel");
                child.GetComponentInChildren<Slider>().value = (float)level / 9.0f; //Get the slider component and set it equal to the upgrade level / 9
                shopPanel.Find("Slider").Find("Fill Area").GetComponentInChildren<Image>().color = new Color(0.12f + (rPerLevel * level), (0.72f + gPerLevel * level),0.05f + (bPerLevel * level));

                if(level >= 9.0f)
                {
                    shopPanel.Find("BuyUpgrade").GetComponent<Button>().interactable = false;
                    shopPanel.Find("BuyUpgrade").GetComponentInChildren<Text>().text = "Max Level";
                }
                else
                {
                    shopPanel.Find("BuyUpgrade").GetComponentInChildren<Text>().text = "Upgrade Cost: " + Gameplay.GetItemStatistic(child.name, "UpgradeCost");
                    if(Gameplay.credits < (int)Gameplay.GetItemStatistic(child.name, "UpgradeCost"))
                    {
                        shopPanel.Find("BuyUpgrade").GetComponent<Button>().interactable = false;
                    }
                    else
                    {
                        shopPanel.Find("BuyUpgrade").GetComponent<Button>().interactable = true;
                    }
                }

            }
        }

    }
    void LoadShop()
    {
        for (int i = 0; i < Gameplay.shopItems.Count; i++)
        {
            GameObject shop = Instantiate(shopItemPrefab, shopItemContainer.transform, false);

            RectTransform pfrt = (RectTransform)shopItemContainer.transform; //shop item container rect

            shop.name = Gameplay.shopItems[i].name;
            RectTransform rt = shop.GetComponentInChildren<RectTransform>(); //item rect

            rt.localPosition = new Vector3(0, -(30 + (65 * i)), 0);
            pfrt.sizeDelta = new Vector2(pfrt.sizeDelta.x, (70 + (65 * i)));

            Transform itemBox = shop.transform.Find("ShopItemPanel");

            itemBox.Find("ShopItemName").GetComponent<Text>().text = Gameplay.shopItems[i].name;

            itemBox.Find("BuyUpgrade").GetComponent<Button>().onClick.RemoveAllListeners();

            string nameStorage = Gameplay.shopItems[i].name;
            itemBox.Find("BuyUpgrade").GetComponent<Button>().onClick.AddListener(() => UpgradeShopItem(nameStorage)); 

        }

        LoadShopItemLevels();
    }

    void LoadEmails()
    {
        if (emails.Count > 0)
        {
            RectTransform pfrt = (RectTransform)emailsContainer.transform;
            pfrt.sizeDelta = new Vector2(pfrt.sizeDelta.x, (30 + (40 * emails.Count)));

            for (int i = 0; i < emails.Count; i++)
            {
                GameObject emailItem = Instantiate(emailItemPrefab, emailsContainer.transform, false);

                emailItem.name = i.ToString();
                RectTransform rt = emailItem.GetComponentInChildren<RectTransform>(); //item rect

                rt.localPosition = new Vector3(0, -(25 + (40 * (emails.Count-(i+1)))), 0);

                Transform itemBox = emailItem.transform.Find("Border").Find("EmailPanel");

                itemBox.Find("SenderLine").GetComponent<Text>().text = "From: " + emails[i].sender;
                itemBox.Find("SubjectLine").GetComponent<Text>().text = "Subject: " + emails[i].subject;
                itemBox.Find("OpenEmail").GetComponent<Button>().onClick.RemoveAllListeners();
                string emailIDstorage = i.ToString();
                itemBox.Find("OpenEmail").GetComponent<Button>().onClick.AddListener(() => LoadSpecificEmail(emailIDstorage));


            }
        }

    }

    void LoadSpecificEmail(string ind)
    {
        emailScrollRect.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
        int index = int.Parse(ind);
        emailSender.text = "From: " + emails[index].sender + "\nSubject:" + emails[index].subject + "\nCC:";
        emailContents.text = emails[index].text;
        returnEmailsSpecific.onClick.RemoveAllListeners();
        returnEmailsSpecific.onClick.AddListener(() => SwitchMode("emailMode"));
        //RectTransform pfrt = (RectTransform)scrollableSpecificEmail.transform; //emails rect
        //pfrt.sizeDelta = new Vector2(pfrt.sizeDelta.x, pfrt.sizeDelta.y+ 10000);
        SwitchMode("specificEmailMode");
    }
    
    void UpgradeShopItem(string name) //Upgrade item and reload the item levels for shop objects
    {
        Gameplay.UpgradeVariable(name,true);
        LoadShopItemLevels();
    }
    void LoadReviewTab(Alien alien1,Alien alien2)
    {
        reviewTab.transform.Find("ExitReview").GetComponent<Button>().onClick.RemoveAllListeners();
        reviewTab.transform.Find("ExitReview").GetComponent<Button>().onClick.AddListener(() => SwitchMode("profilesMode"));
        reviewTab.transform.Find("Alien1").GetComponent<Image>().sprite = alien1.ReturnImage();
        reviewTab.transform.Find("Alien2").GetComponent<Image>().sprite = alien2.ReturnImage();
        string reviewText = GenerateReviewText(alien1, alien2);
        reviewTab.transform.Find("ReviewText").GetComponent<TextMeshProUGUI>().text = reviewText;
        GenerateReviewEmail(reviewText, alien1, alien2);
    }
    void GenerateReviewEmail(string reviewText, Alien alien1,Alien alien2)
    {
        System.DateTime emailTime = System.DateTime.Now;

        string processedText = reviewText.Replace("<size=14>", "").Replace("<size=8>","").Replace("</size>","").Replace("<color=#FFFFFF>","<color=#000000>").Replace("<size=18>","").Replace("<color=#0E75B8>","<color=#000000>");
        Email rEmail = new Email();
        rEmail.sender = "noreply-reviewlogger@Internal.yc";
        rEmail.subject = "Matchup review " + emailTime.ToString("dd/MM/yyyy HH:mm:ss");
        rEmail.recievedTime = emailTime;
        rEmail.text = "Matchup data from period " + emailTime.ToString("dd/MM/yyyy HH:mm:ss") + "\n\n" +
            "Signal 1 Text:\n" + alien1.GetAlienText() + "\n\n" +
            "Signal 2 Text:\n" + alien2.GetAlienText() + "\n\n"+
            "Final Report:\n\n" + processedText;
        emailQueue.Enqueue(rEmail);
    }
    string GenerateReviewText(Alien alien1,Alien alien2)
    {
        Dictionary<string, float> statCompatabilityScore = new Dictionary<string, float>()
        {
            {"Body type",Gameplay.GetAverageComparisonMultiplier(typeof(BodyType),alien1,alien2)},
            {"Age range",Gameplay.GetAverageComparisonMultiplier(typeof(AgeType),alien1,alien2)},
            {"Occupation",Gameplay.GetAverageComparisonMultiplier(typeof(OccupationType),alien1,alien2)},
            {"Relationship goals",Gameplay.GetAverageComparisonMultiplier(typeof(GoalsType),alien1,alien2)},
        };

        string bodyText = "";

        foreach (string stat in statCompatabilityScore.Keys)
        {
            bodyText += "<size=14><color=#FFFFFF>" + stat + " compatability: </color>";

            (string name, string colour) scoreVals = GetScoringName(statCompatabilityScore[stat], 0.2f);

            bodyText += "<color=" + scoreVals.colour + ">" + scoreVals.name + "</color></size>\n";

            (string name, string colour) scoringA1 = ("NULL","#FFFFFF");
            (string name, string colour) scoringA2 = ("NULL", "#FFFFFF");

            switch (stat)
            {
                case "Body type":
                    scoringA1 = GetScoringName(Gameplay.GetPrefComparisonMultiplier(typeof(BodyType), alien1.preferenceParams.body.ToString(), alien2.selfParams.body.ToString()), 0.2f);
                    scoringA2 = GetScoringName(Gameplay.GetPrefComparisonMultiplier(typeof(BodyType), alien2.preferenceParams.body.ToString(), alien1.selfParams.body.ToString()), 0.2f);
                    bodyText += "<size=8><color=#0E75B8>[" + alien1.preferenceParams.body + "->" + alien2.selfParams.body + "=</color><color=" + scoringA1.colour + ">" + scoringA1.name + "</color><color=#0E75B8>] ";
                    bodyText += "[" + alien2.preferenceParams.body + "->" + alien1.selfParams.body + "=</color><color=" + scoringA2.colour + ">" + scoringA2.name + "</color><color=#0E75B8>]</color></size>\n";
                    break;
                case "Age range":
                    scoringA1 = GetScoringName(Gameplay.GetPrefComparisonMultiplier(typeof(AgeType), alien1.preferenceParams.age.ToString(), alien2.selfParams.age.ToString()), 0.2f);
                    scoringA2 = GetScoringName(Gameplay.GetPrefComparisonMultiplier(typeof(AgeType), alien2.preferenceParams.age.ToString(), alien1.selfParams.age.ToString()), 0.2f);
                    bodyText += "<size=8><color=#0E75B8>[" + alien1.preferenceParams.age + "->" + alien2.selfParams.age + "=</color><color=" + scoringA1.colour + ">" + scoringA1.name + "</color><color=#0E75B8>] ";
                    bodyText += "[" + alien2.preferenceParams.age + "->" + alien1.selfParams.age + "=</color><color=" + scoringA2.colour + ">" + scoringA2.name + "</color><color=#0E75B8>]</color></size>\n";
                    break;
                case "Occupation":
                    scoringA1 = GetScoringName(Gameplay.GetPrefComparisonMultiplier(typeof(OccupationType), alien1.preferenceParams.job.ToString(), alien2.selfParams.job.ToString()), 0.2f);
                    scoringA2 = GetScoringName(Gameplay.GetPrefComparisonMultiplier(typeof(OccupationType), alien2.preferenceParams.job.ToString(), alien1.selfParams.job.ToString()), 0.2f);
                    bodyText += "<size=8><color=#0E75B8>[" + alien1.preferenceParams.job + "->" + alien2.selfParams.job + "=</color><color=" + scoringA1.colour + ">" + scoringA1.name + "</color><color=#0E75B8>] ";
                    bodyText += "[" + alien2.preferenceParams.job + "->" + alien1.selfParams.job + "=</color><color=" + scoringA2.colour + ">" + scoringA2.name + "</color><color=#0E75B8>]</color></size>\n";
                    break;
                case "Relationship goals":
                    scoringA1 = GetScoringName(Gameplay.GetPrefComparisonMultiplier(typeof(GoalsType), alien1.preferenceParams.relationshipGoal.ToString(), alien2.selfParams.relationshipGoal.ToString()), 0.2f);
                    scoringA2 = GetScoringName(Gameplay.GetPrefComparisonMultiplier(typeof(GoalsType), alien2.preferenceParams.relationshipGoal.ToString(), alien1.selfParams.relationshipGoal.ToString()), 0.2f);
                    bodyText += "<size=8><color=#0E75B8>[" + alien1.preferenceParams.relationshipGoal + "->" + alien2.selfParams.relationshipGoal + "=</color><color=" + scoringA1.colour + ">" + scoringA1.name + "</color><color=#0E75B8>] ";
                    bodyText += "[" + alien2.preferenceParams.relationshipGoal + "->" + alien1.selfParams.relationshipGoal + "=</color><color=" + scoringA2.colour + ">" + scoringA2.name + "</color><color=#0E75B8>]</color></size>\n";
                    break;
                default:
                    Debug.LogError("error generating review text");
                    break;
            }

        }

        while(bodyText.Contains("NoPref")) //Remove all nopref occurances
        {
            bodyText = bodyText.Replace("NoPref", "no preference");

        }

        int credScore = Gameplay.GetCreditScore(alien1, alien2);

        (string name, string colour) finalScoreVal = GetScoringName(credScore, 12);

        bodyText += "<size=18><color=#FFFFFF> Final credit score: </color><color=" + finalScoreVal.colour + ">" + credScore + "</color></size>\n";

        return bodyText;
    }
    private (string descriptor, string color) GetScoringName(float value, float divisor) //Groups score into five segments of equal size
    {
        string desc = "NULL";
        string colour = "#FFFFFF";

        if (value <= divisor * 1.0f)
        {
            desc = "Abysmal";
            colour = "#b80e20";
        }
        else if (value <= divisor * 2.0f)
        {
            desc = "Poor";
            colour = "#b8510e";
        }
        else if (value <= divisor * 3.0f)
        {
            desc = "Mediocre";
            colour = "#edd613";
        }
        else if (value <= divisor * 4.0f)
        {
            desc = "Good";
            colour = "#20b80e";
        }
        else
        {
            desc = "Excellent";
            colour = "#0eb851";
        }

        return (desc, colour);
    }
    void Update()
    {
        if (currentMode == LaptopModes.Email && lastEmailCount != emails.Count)
        {
            SwitchMode("emailMode");
        }
        lastEmailCount = emails.Count;

        if (Gameplay.storyState >= 6 && !deleteIsEnabled) //Check if the user has finished the tutorial and enable deleting profiles when true
        {
            deleteIsEnabled = true;
        }

        dtTime += Time.deltaTime;

        if(dtTime >= 0.1f)
        {
            creditsText.text = "Credits: " + Gameplay.credits;
        }

        if(currentMode == LaptopModes.Console) //If in console mode, pressing enter allows for submission of text to console
        {
            if (Input.GetKeyUp(KeyCode.Return) && consoleInput.text != "") 
            { 
                SubmitConsoleInput(consoleInput.text);
                consoleInput.text = "";
            }
        }
    }
    private void SubmitConsoleInput(string input)
    {
        LaptopConsole.SubmitItem(ref consoleText, input);
        consoleInput.ActivateInputField();
    }

    public static void AddEmail(string sender, string title, string body, bool isSilent)
    {
        emails.Add(new Email(sender,title,body));
        if (!isSilent) { AudioHandler.EmailNotification(); } //Play an email notification sound  
    }
    public static void AddEmail(Email sendEmail, bool isSilent)
    {
        emails.Add(sendEmail);
        if (!isSilent) { AudioHandler.EmailNotification(); } //Play an email notification sound  
    }
    public static void AddEmail(string sender, string title, string body, System.DateTime time, bool isSilent)
    {
        emails.Add(new Email(sender, title, body,time));
        if (!isSilent) { AudioHandler.EmailNotification(); } //Play an email notification sound  
    }

    public static void HandleEmailQueue()
    {
        if (emailQueue.Count > 0)
        {
            AddEmail(emailQueue.Dequeue(), false); //Remove top of email queue
        }
    }
}

public struct Email
{
    public System.DateTime recievedTime;
    public string sender;
    public string subject;
    public string text;

    public Email(string newSender, string newTitle, string newBody)
    {
        sender = newSender;
        subject = newTitle;
        text = newBody;
        recievedTime = System.DateTime.Now;
    }

    public Email(string newSender, string newTitle, string newBody, System.DateTime time)
    {
        sender = newSender;
        subject = newTitle;
        text = newBody;
        recievedTime = time;
    }
}