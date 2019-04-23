using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public GameObject MC;
    
    // Update is called once per frame
    void LateUpdate()
    {
        LoadDLL mc = MC.GetComponent<LoadDLL>();
        //if(1)
        mc.threshold *= 1.01f;
        mc.Reflash();
        //if (1)
           // mc.Half();

    }
}
