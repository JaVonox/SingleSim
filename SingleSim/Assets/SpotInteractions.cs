using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SpotInteractions : MonoBehaviour
{
    public Button a;

    // Start is called before the first frame update
    void Start()
    {
        a.onClick.AddListener(() => b());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void b()
    {
        Debug.Log("A");
    }
}
