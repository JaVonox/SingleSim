using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DecoderControls : MonoBehaviour
{
    public GameObject alienImage;
    public GameObject splitPrefab;
    private List<GameObject> imageSplits = new List<GameObject>();

    private const float prog = 0.5f; //temporary progress constant, will be replaced with a static in gameplay

    // Start is called before the first frame update
    void Start()
    {
        UpdateImage(); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateImage()
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

        alienImage.GetComponent<Image>().color = new Color32(255, 255, 255, 0);

        int stage = (int)Mathf.Floor(prog / 0.25f); //0-4 stages

        int splitsTotal = (int)Mathf.Pow(4, stage); //Total number of splits
        int splitPerSide = (int)Mathf.Sqrt(splitsTotal);

        float widthPerSplit = ((RectTransform)alienImage.transform).rect.width / splitPerSide;
        float heightPerSplit = ((RectTransform)alienImage.transform).rect.height / splitPerSide;

        float originX = alienImage.transform.localPosition.x;
        float originY = alienImage.transform.localPosition.y;

        Debug.Log(widthPerSplit + "," + heightPerSplit);
        Debug.Log(splitsTotal + ":" + splitPerSide);

        for (int y = 1; y < splitPerSide + 1; y++)
        {
            for (int x = 1; x < splitPerSide + 1; x++)
            {
                GameObject newSplit = Instantiate(splitPrefab, alienImage.transform);
                RectTransform rt = newSplit.GetComponentInChildren<RectTransform>();

                rt.sizeDelta = new Vector2(widthPerSplit, heightPerSplit); //Set width and height to precalculated values
                Debug.Log(originX + "+ (" + widthPerSplit + " * " + (x - 1) + ")");
                rt.localPosition = new Vector3((widthPerSplit * (x - 1)), - (heightPerSplit * (y - 1)), 0);

                imageSplits.Add(newSplit);
            }
        }
    }
}
