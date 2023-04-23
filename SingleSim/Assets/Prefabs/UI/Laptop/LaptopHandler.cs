using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
enum LaptopModes
{
    Profiles,
    Specific,
    Shop,
    Console
}
public class LaptopHandler : MonoBehaviour
{
    public Button profilesMode;
    public Button shopMode;
    public Button consoleMode;
    public Text creditsText;
    public GameObject profileSmallPrefab;
    public GameObject profilesContainer;

    public GameObject profilesTab;
    public GameObject specificProfileTab;

    private Alien comparitorAlien;
    private LaptopModes currentMode;

    // Start is called before the first frame update

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

        switch (currentMode)
        {
            case LaptopModes.Profiles:
                profilesMode.interactable = false;
                shopMode.interactable = true;
                consoleMode.interactable = true;
                profilesTab.SetActive(true);
                specificProfileTab.SetActive(false);
                LoadProfiles();
                break;
            case LaptopModes.Specific:
                profilesMode.interactable = true;
                shopMode.interactable = true;
                consoleMode.interactable = true;
                profilesTab.SetActive(false);
                specificProfileTab.SetActive(true);
                break;
            case LaptopModes.Shop:
                profilesMode.interactable = true;
                shopMode.interactable = false;
                consoleMode.interactable = true;
                profilesTab.SetActive(false);
                specificProfileTab.SetActive(false);
                break;
            case LaptopModes.Console:
                profilesMode.interactable = true;
                shopMode.interactable = true;
                consoleMode.interactable = false;
                profilesTab.SetActive(false);
                specificProfileTab.SetActive(false);
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
                    profileBox.Find("LoadProfile").GetComponent<Button>().onClick.AddListener(() => ExitComparisonMode());
                    profileBox.Find("LoadProfile").GetComponentInChildren<Text>().text = "Stop Comparing";
                    profileBox.Find("LoadProfile").GetComponentInChildren<Text>().color = new Color(0.72f, 0.05f, 0.12f);
                }
                else
                {
                    profileBox.GetComponent<Image>().color = new Color(0.58f, 0.58f, 0.58f);
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
        profileTransform.Find("ProfileText").GetComponent<Text>().text = alienProfile.decodeTextMessage;

        if(comparitorAlien != null) //Check if the profile is loaded in comparison mode
        {
            profileTransform.Find("ExitComparisonMode").gameObject.SetActive(true);
            profileTransform.Find("Match").GetComponentInChildren<Text>().text = "Match Users";
            profileTransform.Find("DeleteProfile").GetComponent<Button>().onClick.AddListener(() => ExitComparisonMode());
        }
        else
        {
            profileTransform.Find("ExitComparisonMode").gameObject.SetActive(false);
            profileTransform.Find("Match").GetComponentInChildren<Text>().text = "Find Match";
            profileTransform.Find("Match").GetComponent<Button>().onClick.AddListener(() => BeginComparisonMode(alienProfile));
            profileTransform.Find("ExitComparisonMode").GetComponent<Button>().onClick.AddListener(() => ExitComparisonMode());
        }

        profileTransform.Find("Return").GetComponent<Button>().onClick.AddListener(()=>SwitchMode("profilesMode"));
        profileTransform.Find("DeleteProfile").GetComponent<Button>().onClick.AddListener(() => DeleteProfile(alienProfile));
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
    void Update()
    {
        dtTime += Time.deltaTime;

        if(dtTime >= 0.1f)
        {
            creditsText.text = "Credits: " + Gameplay.credits;
        }
    }
}
