using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; 

public class Blob : XRGrabInteractable
{
    public Color Color{ 
        get { return GetComponent<Renderer>().material.color; } //cache renderer?
        set { GetComponent<Renderer>().material.color = value; } 
    }

public Rigidbody Rigidbody {
    get { return GetComponent<Rigidbody>();}
}
    public void SetDropOutVisual() {
        transform.localScale = transform.localScale * 0.7f;
        var material = GetComponent<Renderer> ().material;
        material.color = Color.Lerp(material.color, Color.black, 0.8f);
    }
}
