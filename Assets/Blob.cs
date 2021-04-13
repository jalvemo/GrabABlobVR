using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MLAPI;
using MLAPI.NetworkVariable;

public class Blob : NetworkBehaviour
{
    public Blob() {

        _color.OnValueChanged += (fromColor, toColor) => {
            GetComponent<Renderer>().material.color = toColor; 
        }; 
    }
    void Start()
    {
        if (NetworkManager.Singleton.IsServer) {
            GetComponent<NetworkObject>().Spawn();
        }        
    }
    private XRGrabInteractable GrabInteractable {
        get { return GetComponent<XRGrabInteractable>(); }
    }

    public void SetGrabLayer(LayerMask layer) {
        GrabInteractable.interactionLayerMask = layer;
    }
    ///private NetworkVariable<Color> _color = new NetworkVariable<Color>();
    private NetworkVariableColor _color = new NetworkVariableColor(Color.white);
    //private Color _color;
    public Color Color { 
        get { return _color.Value; }
        set { _color.Value = value; } //GetComponent<Renderer>().material.color = value;Debug.Log("seeeeet"  );} 
    }    
    private NetworkObject _networkObject;
    public Rigidbody Rigidbody {
        get { return GetComponent<Rigidbody>();}
    }
    public void SetDropOutVisual() {
        Rigidbody.MovePosition(Rigidbody.position + new Vector3(0,2,0));

        //transform.localScale = transform.localScale * 0.7f;
        Color = Color.Lerp(Color, Color.black, 0.8f);
    }
}
