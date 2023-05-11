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
        List<SaveItem>? saves = FileLoading.LoadSaves();

        if (saves == null)
        {

        }
        else
        {
            for (int i = 0; i < saves.Count; i++)
            {
                GameObject newBox = Instantiate(saveBoxPrefab, savesContainer.transform, false);

                RectTransform pfrt = (RectTransform)savesContainer.transform; //shop item container rect

                RectTransform rt = newBox.GetComponentInChildren<RectTransform>(); //item rect

                rt.localPosition = new Vector3(0, -(45 + (90 * i)), 0);
                pfrt.sizeDelta = new Vector2(pfrt.sizeDelta.x, (90 + (90 * i)));

                Transform childObj = newBox.transform.Find("SavegamePanel");
                childObj.Find("SaveName").GetComponent<TMPro.TextMeshProUGUI>().text = saves[i].name;
                childObj.Find("DateTime").GetComponent<TMPro.TextMeshProUGUI>().text = saves[i].version + "\n" + saves[i].lastTime;
                newBox.name = saves[i].filepath; //Store filepath as gameobject name to allow loading
            }
        }
    }
}
