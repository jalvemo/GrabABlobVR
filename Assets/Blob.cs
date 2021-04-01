using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; 

public class Blob : XRGrabInteractable
{
    private Color _color;
    public Color Color{ 
        get { return _color; }
        set { _color = value; GetComponent<Renderer>().material.color = value; } 
    }    
    public Rigidbody Rigidbody {
        get { return GetComponent<Rigidbody>();}
    }
    public void SetDropOutVisual() {
        Rigidbody.MovePosition(Rigidbody.position + new Vector3(0,2,0));

        transform.localScale = transform.localScale * 0.7f;
        var material = GetComponent<Renderer> ().material;
        material.color = Color.Lerp(material.color, Color.black, 0.8f);
    }
}
