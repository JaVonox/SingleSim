using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionsCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("A");
    }
}
