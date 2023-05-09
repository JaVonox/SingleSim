using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SaveDialog : MonoBehaviour
{
    public GameObject saveBoxPrefab;
    public GameObject savesContainer;
    void Start()
    {
        CreateFileBoxes();
    }

    void CreateFileBoxes()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject newBox = Instantiate(saveBoxPrefab, savesContainer.transform, false);

            RectTransform pfrt = (RectTransform)savesContainer.transform; //shop item container rect

            newBox.name = "Save" + i;
            RectTransform rt = newBox.GetComponentInChildren<RectTransform>(); //item rect

            rt.localPosition = new Vector3(0, -(45 + (90 * i)), 0);
            pfrt.sizeDelta = new Vector2(pfrt.sizeDelta.x, (90 + (90 * i)));

            Transform childObj = newBox.transform.Find("SavegamePanel");
            childObj.Find("SaveName").GetComponent<TMPro.TextMeshProUGUI>().text = "Save" + i;
            childObj.Find("DateTime").GetComponent<TMPro.TextMeshProUGUI>().text = "LAST UPDATE";
        }
    }
}
