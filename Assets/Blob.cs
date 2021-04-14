using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MLAPI;
using MLAPI.NetworkVariable;

public class Blob : NetworkBehaviour
{
    private static GameObject _prefab;
    public static GameObject Prefab { get { return _prefab ?? (_prefab = Resources.Load<GameObject>("Blob")); } }
    public static List<Color> Colors = new List<Color> {Color.green, Color.magenta, Color.red, Color.yellow, Color.blue};
    public static Blob Instantiate(Vector3 vector, Color? color = null) {    
        Blob blob = Instantiate(Resources.Load<GameObject>("blob"), vector, new Quaternion()).GetComponent<Blob>();
        blob.Color = color ?? Colors[Random.Range(0, Colors.Count)];
        blob.SetGrabLayer(Layers.KEEP);
        blob.Rigidbody.useGravity = true;
        if (NetworkManager.Singleton.IsServer) { 
            blob.GetComponent<NetworkObject>().Spawn();
        }        
        return blob;
    }

    public Blob() {
        _color.OnValueChanged += (fromColor, toColor) => {
            GetComponent<Renderer>().material.color = toColor; 
        };
        _scale.OnValueChanged += (fromScale, toScale) => {
            transform.localScale = toScale;
        };
    }

    private NetworkVariable<Color> _color = new NetworkVariable<Color>(Color.white);
    private NetworkVariable<Vector3> _scale = new NetworkVariable<Vector3>(new Vector3(0.182641f, 0.182641f, 0.182641f));

    public Color Color {
        get { return _color.Value; }
        set { _color.Value = value; }
    }
      
    public void SetGrabLayer(LayerMask layer) {
        GetComponent<XRGrabInteractable>().interactionLayerMask = layer;
    }

    public Rigidbody Rigidbody {
        get { return GetComponent<Rigidbody>();}
    }

    public void SetDropOutVisual() {
        Rigidbody.MovePosition(Rigidbody.position + new Vector3(0,2,0));

        _scale.Value = _scale.Value * 0.7f;
        Color = Color.Lerp(Color, Color.black, 0.8f);
    }
    
}
