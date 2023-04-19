using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DecoderControls : MonoBehaviour
{
    public GameObject alienImage;
    public GameObject splitPrefab;
    public Slider progSlider;
    public Button startDecode;
    public Button uploadDecoded;
    public Scrollbar scrolltext;
    public TMPro.TextMeshProUGUI console;

    private bool isLoaded = false;

    private float updateTime = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        startDecode.onClick.AddListener(() => BeginDecode());
        uploadDecoded.onClick.AddListener(() => EndDecode());
        LoadDecoder();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLoaded) //If nothing is loaded, check if something needs to be loaded
        {
            if(Gameplay.activeAlien != null)
            {
                LoadDecoder();
            }
            updateTime = 0.2f;
        }
        else
        {
            progSlider.value = Gameplay.activeAlien.decoderProgress;

            updateTime += Time.deltaTime;

            if(updateTime >= 0.2f)
            {
                DecoderUpdate();
                updateTime = 0;

                if(Gameplay.activeAlien.decoderProgress >= 1)
                {
                    startDecode.gameObject.SetActive(true);
                    uploadDecoded.gameObject.SetActive(true);

                    startDecode.interactable = false;
                    uploadDecoded.interactable = true;
                }
            }
        }
    }
    void LoadDecoder()
    {
        if(Gameplay.activeAlien == null)
        {
            alienImage.SetActive(false);
            progSlider.gameObject.SetActive(false);
            progSlider.value = 0;
            startDecode.gameObject.SetActive(false);
            startDecode.interactable = false;
            uploadDecoded.gameObject.SetActive(false);
            uploadDecoded.interactable = false;
            console.text = "No signal loaded. Find a signal using the scanner to begin.";
            isLoaded = false;
        }
        else
        {
            alienImage.SetActive(true);
            alienImage.GetComponent<Image>().sprite = Gameplay.activeAlien.ReturnImage();
            DecoderUpdate();

            progSlider.gameObject.SetActive(true);
            progSlider.value = Gameplay.activeAlien.decoderProgress;

            startDecode.gameObject.SetActive(true);
            uploadDecoded.gameObject.SetActive(true);

            if (Gameplay.activeAlien.decoderProgress == -1)
            {
                startDecode.interactable = true;
            }
            else
            {
                startDecode.interactable = false;

                if(Gameplay.activeAlien.decoderProgress >= 1)
                {
                    uploadDecoded.interactable = true;
                }
            }

            console.text = "Signal Loaded, press 'Begin Decoding' to translate";
            isLoaded = true;
        }
    }

    void BeginDecode()
    {
        Gameplay.activeAlien.decoderProgress = 0;
        startDecode.interactable = false;
    }
    void EndDecode()
    {
        if(Gameplay.activeAlien.decoderProgress >= 1)
        {
            Gameplay.storedAliens.Add(new Alien(Gameplay.activeAlien)); //Store completed alien
            Gameplay.activeAlien = null; //Delete active alien
            LoadDecoder();
            Gameplay.scannerConsolePopupEnabled = false;
            Gameplay.UItext = "...";
        }
    }
    void DecoderUpdate()
    {
        //Algorithm - make a split of images based on the progress - update each with an appropriate colour based on the pixel in the image
        //for each 0.25 split, 4^x where x is number of 0.25 prog
        //Square root of this number is how many need to render per y value
        //inherit colour from top left colour in image

        if (alienImage.transform.childCount > 0) //Unload all splits
        {
            foreach (Transform child in alienImage.transform)
            {
                Destroy(child.gameObject);
            }
        }

        if (Gameplay.activeAlien.decoderProgress < 1)
        {

            alienImage.GetComponent<Image>().color = new Color32(255, 255, 255, 0);

            int stage = (int)Mathf.Round(Gameplay.activeAlien.decoderProgress / 0.25f) + 1; //0-5 stages

            int splitsTotal = (int)Mathf.Pow(4, stage); //Total number of splits
            int splitPerSide = (int)Mathf.Sqrt(splitsTotal);

            float widthPerSplit = ((RectTransform)alienImage.transform).rect.width / splitPerSide;
            float heightPerSplit = ((RectTransform)alienImage.transform).rect.height / splitPerSide;

            float originX = alienImage.transform.localPosition.x;
            float originY = alienImage.transform.localPosition.y;

            Texture2D alienImgTexture = alienImage.GetComponent<Image>().sprite.texture;

            for (int y = 1; y < splitPerSide + 1; y++)
            {
                for (int x = 1; x < splitPerSide + 1; x++)
                {
                    GameObject newSplit = Instantiate(splitPrefab, alienImage.transform);
                    RectTransform rt = newSplit.GetComponentInChildren<RectTransform>();

                    rt.sizeDelta = new Vector2(widthPerSplit, heightPerSplit); //Set width and height to precalculated values
                    rt.localPosition = new Vector3((widthPerSplit * (x - 1)), -(heightPerSplit * (y - 1)), 0);

                    Color locColour = alienImgTexture.GetPixel((int)Mathf.Floor(widthPerSplit * (x - 1)), (int)Mathf.Floor(alienImgTexture.height - heightPerSplit * (y - 1))); //Get colour at top left of split

                    //Set colour for image
                    
                    newSplit.GetComponentInChildren<Image>().color = new Color32((byte)Mathf.Floor(locColour.r * 255), (byte)Mathf.Floor(locColour.g * 255), (byte)Mathf.Floor(locColour.b * 255), 255);
                    newSplit.name = "Split (" + x + "," + y + ")";


                }
            }
        }
        else //When the decoder completes running
        {
            alienImage.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            Gameplay.activeAlien.decoderProgress = 1;

            console.text = "Decoded message:\n" + Gameplay.activeAlien.GenerateText();
        }
    }
}
