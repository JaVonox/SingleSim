using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TutorialIndicator : MonoBehaviour
{
    public GameObject indicator; //The indicator
    float yRotation;
    // Start is called before the first frame update
    void Start()
    {
        yRotation = 0;
    }

    // Update is called once per frame
    void Update()
    {
        yRotation += 0.5f;

        if(yRotation > 360) { yRotation = 0; }
        indicator.transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
