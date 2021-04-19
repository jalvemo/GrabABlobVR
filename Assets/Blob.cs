using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Prototyping;

public class Blob : NetworkBehaviour
{
    private static GameObject _networkPrefab;
    private static GameObject _prefab;
    public static GameObject NetworkPrefab { get { return _networkPrefab ?? (_networkPrefab = Resources.Load<GameObject>("BlobNetwork")); } }
    public static GameObject Prefab { get { return _prefab ?? (_prefab = Resources.Load<GameObject>("Blob")); } }
    public static List<Color> Colors = new List<Color> {Color.green, Color.magenta, Color.red, Color.yellow, Color.blue};
    
    private NetworkVariable<Color> _color = new NetworkVariable<Color>(new NetworkVariableSettings {WritePermission = NetworkVariablePermission.OwnerOnly}, Color.white);
    private NetworkVariable<Vector3?> _scale = new NetworkVariable<Vector3?>(new NetworkVariableSettings {WritePermission = NetworkVariablePermission.OwnerOnly}, value:null);

    public static Blob Instantiate(Vector3 vector, Color? color = null, ulong? clientOwnerId = null) {    
        
        if (!NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsClient) { 
            //Debug.Log("IsClient, skipp");
            return null;
        }  
        Blob blob = null;
         if (NetworkManager.Singleton.IsServer) { 
            //Debug.Log("creating server instance");
            blob = Instantiate(NetworkPrefab, vector, new Quaternion()).GetComponent<Blob>();
            var networkObject = blob.GetComponent<NetworkObject>();    
            if (clientOwnerId != null) {
                Debug.Log("creating server instance for id;" + clientOwnerId.Value);
                networkObject.SpawnWithOwnership(clientOwnerId.Value);
            } else {
                //Debug.Log("creating server instance for me;");
                networkObject.Spawn();
            }
        } else {
            //return null;
            //Debug.Log("creating local instance");
            blob = Instantiate(Prefab, vector, new Quaternion()).GetComponent<Blob>();
            blob.Color = color ?? Colors[Random.Range(0, Colors.Count)];
            blob.SetGrabLayer(Layers.KEEP);
            blob.Rigidbody.useGravity = true;
        }    

        return blob;
    }

    public Blob() {
        _color.OnValueChanged += (from, to) => {
            GetComponent<Renderer>().material.color = to; 
        };
        _scale.OnValueChanged += (from, to) => {
            if (to.HasValue) {
                transform.localScale = to.Value;
            }
        };
    }
    public override void NetworkStart() {
        if (IsOwner) {    
            Debug.Log("I am Owner, setting stuff");
            SetGrabLayer(Layers.KEEP);
            _color.Value = Colors[Random.Range(0, Colors.Count)];
            _scale.Value = new Vector3(0.182641f, 0.182641f, 0.182641f);
            Rigidbody.useGravity = true;
        } else {
            SetGrabLayer(Layers.FALL);
            Rigidbody.useGravity = false;
        }
   }
    private void Update() {
       // if (IsOwner) {
       //    Debug.Log("moving blob up:" + new Vector3(0f, 0.1f * Time.deltaTime , 0f));
       //    //transform.Translate(new Vector3(0f, 0.2f * Time.deltaTime , 0f));
       //}
    }

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
