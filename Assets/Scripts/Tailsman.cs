using UnityEngine;
using System.Collections.Generic;

public class Tailsman : MonoBehaviour
{
    [SerializeField] private GameObject tailsmanObj;

    public bool isHold = false;
    public bool isShoot = false;
    void Start()
    {
        tailsmanObj = GameObject.FindWithTag("Tailsman");    
        tailsmanObj.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            isHold = true;
        }
        else
        {
            isHold = false;
        }

        if (isHold)
        {
            tailsmanObj.SetActive(true);
            if (Input.GetMouseButtonDown(1))
            {
                isShoot = true;
            }
        }

        else
        {
            tailsmanObj.SetActive(false);
        }
    }
}
