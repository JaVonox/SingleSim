using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class SaveItemScript : MonoBehaviour
{
    public Button LoadSave;
    public Button DeleteSave;
    // Start is called before the first frame update
    void Start()
    {
        LoadSave.onClick.AddListener(() => SelectLoadFile());
    }

    void SelectLoadFile()
    {
        Gameplay.HandleSaveLoad(this.name);
        SceneManager.LoadScene("Main");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
