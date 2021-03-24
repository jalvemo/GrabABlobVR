﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hide : MonoBehaviour
{
    // Start is called before the first frame update
    bool active = true;
    void Start()
    {
        HideMe();
    }

    public void Toggle() {
        if (active)
            HideMe();
        else
            ShowMe();
    }
    public void ShowMe() {
        active = true;
        Debug.Log("show, active; " + active);
        this.gameObject.SetActive(active);
    }
    public void HideMe() {
        active = false;
        Debug.Log("hide, active; " + active);
        this.gameObject.SetActive(active);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
