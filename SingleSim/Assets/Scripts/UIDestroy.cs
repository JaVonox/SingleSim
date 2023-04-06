using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDestroy : MonoBehaviour
{
    public event System.Action objectDestroyMethod;

    private void OnDestroy()
    {
        objectDestroyMethod(); //run method for destroying the object
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel")) //When pressing escape destroy self
        {
            Destroy(gameObject);
        }
    }
}
