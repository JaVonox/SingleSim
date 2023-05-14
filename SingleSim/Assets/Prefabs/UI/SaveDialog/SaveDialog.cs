using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SaveDialog : MonoBehaviour
{
    public GameObject saveBoxPrefab;
    public GameObject savesContainer;
    public Button closeButton;
    public TextMeshProUGUI noSavesText;
    void Start()
    {
        CreateFileBoxes();
    }

    void CreateFileBoxes()
    {
        if (savesContainer.transform.childCount > 0) //Unload all saves
        {
            foreach (Transform child in savesContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }

        List<SaveItem>? saves = FileLoading.LoadSaves();
        if (saves == null)
        {
            noSavesText.text = "No save files found";
        }
        else
        {
            noSavesText.text = "";
            saves = saves.OrderBy(x => System.DateTime.Parse(x.lastTime)).Reverse().ToList();
            for (int i = 0; i < saves.Count; i++)
            {
                GameObject newBox = Instantiate(saveBoxPrefab, savesContainer.transform, false);

                RectTransform pfrt = (RectTransform)savesContainer.transform; //shop item container rect

                RectTransform rt = newBox.GetComponentInChildren<RectTransform>(); //item rect

                rt.localPosition = new Vector3(0, -(45 + (90 * i)), 0);
                pfrt.sizeDelta = new Vector2(pfrt.sizeDelta.x, (90 + (90 * i)));

                Transform childObj = newBox.transform.Find("SavegamePanel");
                childObj.Find("SaveName").GetComponent<TMPro.TextMeshProUGUI>().text = saves[i].name;
                
                childObj.Find("DateTime").GetComponent<TMPro.TextMeshProUGUI>().text = (saves[i].version == Gameplay.gameVersion ? "<color=#AAAAAA>" : "<color=#b80e20>") + saves[i].version + "\n" + saves[i].lastTime + "</color>";
                childObj.Find("DeleteSave").GetComponent<Button>().onClick.AddListener(() => DeleteItem(newBox));
                childObj.Find("DeleteSave").GetComponentInChildren<Text>().text = "Delete Save";
                newBox.name = saves[i].filepath; //Store filepath as gameobject name to allow loading
            }
        }
    }

    void DeleteItem(GameObject saveObj)
    {
        if (savesContainer.transform.childCount > 0) //Remove really delete state from all items except the current one.
        {
            foreach (Transform child in savesContainer.transform)
            {
                if (saveObj.transform != child) { 
                    child.GetComponentInChildren<SaveItemScript>().reallyDelete = false;
                    child.transform.Find("SavegamePanel").Find("DeleteSave").GetComponentInChildren<Text>().text = "Delete Save";
                }
            }
        }

        if (saveObj.GetComponentInChildren<SaveItemScript>().reallyDelete == false)
        {
            saveObj.transform.Find("SavegamePanel").Find("DeleteSave").GetComponentInChildren<Text>().text = "Really Delete?";
            saveObj.GetComponentInChildren<SaveItemScript>().reallyDelete = true;
        }
        else
        {
            FileLoading.DeleteSave(saveObj.name);
            CreateFileBoxes();
        }
    }
}
