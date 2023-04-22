using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
enum LaptopModes
{
    Profiles,
    Matchup,
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
    private LaptopModes currentMode;

    private List<GameObject> loadedProfiles;
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
    void SwitchUI()
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
                LoadProfiles();
                break;
            case LaptopModes.Matchup:
                profilesMode.interactable = true;
                shopMode.interactable = true;
                consoleMode.interactable = true;
                break;
            case LaptopModes.Shop:
                profilesMode.interactable = true;
                shopMode.interactable = false;
                consoleMode.interactable = true;
                break;
            case LaptopModes.Console:
                profilesMode.interactable = true;
                shopMode.interactable = true;
                consoleMode.interactable = false;
                break;
            default:
                Debug.LogError("Invalid laptop tab");
                break;
        }
    }
    void SwitchMode(string sender)
    {

        switch (sender)
        {
            case "profilesMode":
                currentMode = LaptopModes.Profiles;
                break;
            case "shopMode":
                currentMode = LaptopModes.Shop;
                break;
            case "consoleMode":
                currentMode = LaptopModes.Console;
                break;
            case "MatchupMode":
                currentMode = LaptopModes.Matchup;
                break;
            default:
                Debug.LogError("Invalid laptop tab");
                break;
        }

        SwitchUI();
    }
    void LoadProfiles() //Iterate through each alien in the stored aliens list and spawn a tab marker for each
    {
        for (int i = 0; i < 8; i++)
        {
            GameObject profile = Instantiate(profileSmallPrefab, profilesContainer.transform,false);

            RectTransform pfrt = (RectTransform)profilesContainer.transform; //profile container rect

            float width = (((RectTransform)profilesTab.transform).rect.width - 40) / 3.0f;
            float height = (((RectTransform)profilesTab.transform).rect.height) / 2.0f;

            profile.name = "Profile" + i;
            RectTransform rt = profile.GetComponentInChildren<RectTransform>(); //profile rect

            rt.localPosition = new Vector3(20 + (width * ((i % 3))), -(height * (((int)Mathf.Floor(i/3)))), 0);

            pfrt.sizeDelta = new Vector2(pfrt.sizeDelta.x, 50 + ((i/3.0f) * height)); //TODO change this to apply after the loop completes
        }

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
