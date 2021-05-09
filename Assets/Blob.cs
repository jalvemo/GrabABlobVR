using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Prototyping;

public class Blob : NetworkBehaviour
{
    private Vector3? _moveTowards = null;
    private static Vector3 StartScale = new Vector3(0.18f, 0.18f, 0.18f);
    private static GameObject _networkPrefab;
    private static GameObject _prefab;
    public static GameObject NetworkPrefab { get { return _networkPrefab ?? (_networkPrefab = Resources.Load<GameObject>("BlobNetwork")); } }
    public static GameObject Prefab { get { return _prefab ?? (_prefab = Resources.Load<GameObject>("Blob")); } }
    public static List<Color> Colors = new List<Color> {Color.green, Color.magenta, Color.red, Color.yellow, Color.blue};
    //public static List<Color> Colors = new List<Color> {Color.magenta, Color.yellow};
    
    private NetworkVariable<Color> _color = new NetworkVariable<Color>(new NetworkVariableSettings {WritePermission = NetworkVariablePermission.OwnerOnly}, Color.white);
    private NetworkVariable<Vector3?> _scale = new NetworkVariable<Vector3?>(new NetworkVariableSettings {WritePermission = NetworkVariablePermission.OwnerOnly}, value:null);
    public XRBaseInteractable Interactable {
        get { return GetComponent<XRBaseInteractable>(); }
    }
    
    public enum State {
        IN_SOCKET,
        FALLING,
        PICKED_UP,
        POPPED,
        RELEASED,
        NETWORK_CONTROLLED,
    }

    public static Blob Instantiate(Vector3 vector, Color? color = null, ulong? clientOwnerId = null) {            
        Blob blob = null;
        if (NetworkManager.Singleton.IsServer) { 
            //Debug.Log("creating server instance");
            blob = Instantiate(NetworkPrefab, vector, new Quaternion()).GetComponent<Blob>();
            var networkObject = blob.GetComponent<NetworkObject>();    
            if (clientOwnerId != null) {
                //Debug.Log("creating server instance for id;" + clientOwnerId.Value);
                networkObject.SpawnWithOwnership(clientOwnerId.Value);
            } else {
                //Debug.Log("creating server instance for me;");
                networkObject.Spawn();
            }
        } else if (NetworkManager.Singleton.IsClient){
            return null;
        } else {
            //Debug.Log("creating local blob instance");
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
    
        //Interactable.selectEntered.AddListener((SelectEnterEventArgs args) => {
        //    
        //});
        //Interactable.selectExited.AddListener((SelectExitEventArgs args) => {
        //    
        //});
    }
    public override void NetworkStart() {
        if (IsOwner) {    
            SetGrabLayer(Layers.KEEP);
            _color.Value = Colors[Random.Range(0, Colors.Count)];
            _scale.Value = StartScale;
            Rigidbody.useGravity = true;
        } else {
            SetGrabLayer(Layers.OUT);
            Rigidbody.useGravity = false;
        }
   }
    private void Update() {
        _scale.Value = StartScale * (1 + Beats.GetPulseTriangle() / 10);
    }

    public void MoveTowards(Vector3 destination) {
        _moveTowards = destination;
    }
    private void FixedUpdate() {
        if (_moveTowards != null) {
            float distanceToStop = 0.3f;
            float speed = 2.5f;
            if(Vector3.Distance(transform.position, _moveTowards.Value) > distanceToStop)
            {
                Rigidbody.useGravity = false;
                transform.LookAt(_moveTowards.Value);
                Rigidbody.AddRelativeForce(Vector3.forward * speed, ForceMode.Force);
                
            } else {
                Rigidbody.useGravity = true;
                Rigidbody.AddRelativeForce(Vector3.zero, ForceMode.VelocityChange);
                _moveTowards = null;
            }
        }
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

    public void Destroy() {
        Destroy(gameObject);
        //var newLocal = Instantiate(transform.position, Color);
        //newLocal.SetGrabLayer(Layers.OUT);

    }


    public void SetDropOutVisual() {
        Rigidbody.MovePosition(Rigidbody.position + new Vector3(0,2,0));

        _scale.Value = StartScale * 0.7f;
        Color = Color.Lerp(Color, Color.white, 0.7f);
    }
    
}
