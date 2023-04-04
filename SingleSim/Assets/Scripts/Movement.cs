using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private const float moveSpeed = 0.05f;
    public GameObject playerChar;

    // Update is called once per frame
    void Update()
    {
        MoveChar();
    }
    void MoveChar()
    {
        if (Input.GetKey(KeyCode.W))
        {
            playerChar.transform.position += new Vector3(0,0,moveSpeed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            playerChar.transform.position += new Vector3(-moveSpeed, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            playerChar.transform.position += new Vector3(moveSpeed, 0, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            playerChar.transform.position += new Vector3(0, 0, -moveSpeed);
        }
    }

}
