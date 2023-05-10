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
}
public class LaptopHandler : MonoBehaviour
{
    public Button profilesMode;
    public Button shopMode;
    public Button consoleMode;
    public Text creditsText;

    public GameObject profileSmallPrefab;
    public GameObject profilesContainer;

    public GameObject shopItemPrefab;
    public GameObject shopItemContainer;

    public GameObject profilesTab;
    public GameObject specificProfileTab;

    public GameObject consoleTab;
    public TMPro.TMP_InputField consoleInput;
    public TextMeshProUGUI consoleText;

    public GameObject shopTab;

    public GameObject reviewTab;

    private Alien comparitorAlien;
    private LaptopModes currentMode;

    private float dtTime;
    void Start()
    {
        creditsText.text = "Credits: " + Gameplay.credits;
        profilesMode.onClick.AddListener(() => SwitchMode("profilesMode"));
        shopMode.onClick.AddListener(() => SwitchMode("shopMode"));
        consoleMode.onClick.AddListener(() => SwitchMode("consoleMode"));
        SwitchMode("profilesMode");
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

        switch (currentMode)
        {
            case LaptopModes.Profiles:
                profilesMode.interactable = false;
                shopMode.interactable = true;
                consoleMode.interactable = true;
                profilesTab.SetActive(true);
                specificProfileTab.SetActive(false);
                shopTab.SetActive(false);
                reviewTab.SetActive(false);
                consoleTab.SetActive(false);
                LoadProfiles();
                break;
            case LaptopModes.Specific:
                profilesMode.interactable = true;
                shopMode.interactable = true;
                consoleMode.interactable = true;
                profilesTab.SetActive(false);
                specificProfileTab.SetActive(true);
                shopTab.SetActive(false);
                reviewTab.SetActive(false);
                consoleTab.SetActive(false);
                break;
            case LaptopModes.Shop:
                profilesMode.interactable = true;
                shopMode.interactable = false;
                consoleMode.interactable = true;
                profilesTab.SetActive(false);
                specificProfileTab.SetActive(false);
                shopTab.SetActive(true);
                reviewTab.SetActive(false);
                consoleTab.SetActive(false);
                LoadShop();
                break;
            case LaptopModes.Console:
                profilesMode.interactable = true;
                shopMode.interactable = true;
                consoleMode.interactable = false;
                profilesTab.SetActive(false);
                specificProfileTab.SetActive(false);
                shopTab.SetActive(false);
                reviewTab.SetActive(false);
                consoleTab.SetActive(true);
                LaptopConsole.ReloadConsole(ref consoleText); //Reload the console data
                consoleInput.text = "";
                break;
            case LaptopModes.Review:
                profilesMode.interactable = true;
                shopMode.interactable = true;
                consoleMode.interactable = true;
                profilesTab.SetActive(false);
                specificProfileTab.SetActive(false);
                shopTab.SetActive(false);
                reviewTab.SetActive(true);
                consoleTab.SetActive(false);
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
                profileBox.Find("SignalName").GetComponent<Text>().text = Gameplay.storedAliens[i].signalName;
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
    void LoadSpecificProfile(Alien alienProfile)
    {
        SwitchMode("specificMode"); //Load the specific profile

        Transform profileTransform = specificProfileTab.transform;

        profileTransform.Find("ProfileImage").GetComponent<Image>().sprite = alienProfile.ReturnImage();
        profileTransform.Find("ProfileText").GetComponent<Text>().text = alienProfile.GetAlienText();

        if(comparitorAlien != null && comparitorAlien != alienProfile) //Check if the profile is loaded in comparison mode
        {
            profileTransform.Find("Match").gameObject.SetActive(true);
            profileTransform.Find("Match").GetComponentInChildren<Text>().text = "Match Users";
            profileTransform.Find("Match").GetComponent<Button>().onClick.RemoveAllListeners();
            profileTransform.Find("Match").GetComponent<Button>().onClick.AddListener(() => StartMatchup(alienProfile));
            profileTransform.Find("DeleteProfile").gameObject.SetActive(false);
            profileTransform.Find("ShowComparitor").gameObject.SetActive(true);
            profileTransform.Find("ShowComparitor").GetComponentInChildren<Text>().text = "Compare";
            profileTransform.Find("ShowComparitor").GetComponentInChildren<Text>().color = new Color(0.12f, 0.72f, 0.05f);
            profileTransform.Find("ShowComparitor").GetComponent<Button>().onClick.RemoveAllListeners();
            profileTransform.Find("ShowComparitor").GetComponent<Button>().onClick.AddListener(() => EnterQuickComparison(alienProfile));
            profileTransform.Find("Return").gameObject.SetActive(true);
            profileTransform.Find("Return").GetComponentInChildren<Text>().text = "Return";
            profileTransform.Find("Return").GetComponent<Button>().onClick.RemoveAllListeners();
            profileTransform.Find("Return").GetComponent<Button>().onClick.AddListener(() => SwitchMode("profilesMode"));
        }
        else if (comparitorAlien != null && comparitorAlien == alienProfile) //This occurs if the quick comparison is active - disabling most buttons but allowing for quick access to comparison data
        {
            profileTransform.Find("ShowComparitor").gameObject.SetActive(false);
            profileTransform.Find("Match").gameObject.SetActive(false);
            profileTransform.Find("DeleteProfile").gameObject.SetActive(false);
            profileTransform.Find("Return").gameObject.SetActive(false);
            profileTransform.Find("ShowComparitor").gameObject.SetActive(true);
            profileTransform.Find("ShowComparitor").GetComponentInChildren<Text>().color = new Color(0.72f, 0.05f, 0.12f);
            profileTransform.Find("ShowComparitor").GetComponentInChildren<Text>().text = "Stop Comparing";
        }
        else
        {
            profileTransform.Find("ShowComparitor").gameObject.SetActive(false);
            profileTransform.Find("Match").gameObject.SetActive(true);
            profileTransform.Find("Match").GetComponentInChildren<Text>().text = "Find Match";
            profileTransform.Find("Match").GetComponent<Button>().onClick.RemoveAllListeners();
            profileTransform.Find("Match").GetComponent<Button>().onClick.AddListener(() => BeginComparisonMode(alienProfile));
            profileTransform.Find("Return").gameObject.SetActive(true);
            profileTransform.Find("Return").GetComponent<Button>().onClick.RemoveAllListeners();
            profileTransform.Find("Return").GetComponent<Button>().onClick.AddListener(() => SwitchMode("profilesMode"));
            profileTransform.Find("DeleteProfile").gameObject.SetActive(true);
            profileTransform.Find("DeleteProfile").GetComponent<Button>().onClick.RemoveAllListeners();
            profileTransform.Find("DeleteProfile").GetComponent<Button>().onClick.AddListener(() => DeleteProfile(alienProfile));
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
        specificProfileTab.transform.Find("ShowComparitor").GetComponent<Button>().onClick.RemoveAllListeners();
        specificProfileTab.transform.Find("ShowComparitor").GetComponent<Button>().onClick.AddListener(() => LoadSpecificProfile(passedAlien));
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
    void UpgradeShopItem(string name) //Upgrade item and reload the item levels for shop objects
    {
        Gameplay.UpgradeVariable(name);
        LoadShopItemLevels();
    }
    void LoadReviewTab(Alien alien1,Alien alien2)
    {
        reviewTab.transform.Find("ExitReview").GetComponent<Button>().onClick.AddListener(() => SwitchMode("profilesMode"));
        reviewTab.transform.Find("Alien1").GetComponent<Image>().sprite = alien1.ReturnImage();
        reviewTab.transform.Find("Alien2").GetComponent<Image>().sprite = alien2.ReturnImage();
        reviewTab.transform.Find("ReviewText").GetComponent<TextMeshProUGUI>().text = GenerateReviewText(alien1,alien2); //TODO maybe use line by line speed?
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
}
