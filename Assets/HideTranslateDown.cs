using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideTranslateDown : MonoBehaviour
{
    // Start is called before the first frame update
    bool active = true;
    void Start()
    {
        //yield return new WaitForSeconds(1f);
        HideMe();
        Debug.Log("init hide on start");
    }

    public void Toggle() {
        if (active)
            HideMe();
        else
            ShowMe();
    }
    public void ShowMe() {
            Debug.Log("show me1");
        if (!active) {
            active = true;
            Debug.Log("show me2");
            gameObject.transform.SetPositionAndRotation(gameObject.transform.position + new Vector3(0.0f,10.0f,0.0f), transform.rotation);
        }
    }
    public void HideMe() {
            Debug.Log("hide me1");
        if (active) {
            active = false;
            Debug.Log("hide me2");
            gameObject.transform.SetPositionAndRotation(gameObject.transform.position + new Vector3(0.0f,-10.0f,0.0f), transform.rotation);
        }
    
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
