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
    
    
    public Rigidbody Rigidbody;
    private XRGrabInteractable _grabInteractable;
    private LayerMask _grabLayer { set{ _grabInteractable.interactionLayerMask = value; } }

    public XRBaseInteractable Interactable {
        get { return GetComponent<XRBaseInteractable>(); }
    } 
    public BlobState _state = BlobState.RELEASED;
    public BlobState State {
        get { return _state; }
        set {
            _state = value;
            switch (value)
            {
                case BlobState.FALLING:
                    _grabLayer = Layers.FALL;
                    break;
                case BlobState.IN_SOCKET:
                case BlobState.AI_RELEASED:
                    _grabLayer = Layers.KEEP;
                    break;
                case BlobState.POPPED:
                case BlobState.NETWORK_CONTROLLED:
                case BlobState.AI_PICKED_UP:
                    _grabLayer = Layers.OUT;
                    break;
                case BlobState.PICKED_UP:
                case BlobState.RELEASED:
                default:
                    break;
            }
        }
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
        }    

        return blob;
    }
     private void Start() {
        Rigidbody = GetComponent<Rigidbody>();
        _grabInteractable = GetComponent<XRGrabInteractable>();

        Interactable.selectEntered.AddListener((SelectEnterEventArgs args) => {
            if (typeof(XRBaseControllerInteractor).IsAssignableFrom(args.interactor.GetType())) {
                State = BlobState.PICKED_UP;            
            }
        });
        Interactable.selectExited.AddListener((SelectExitEventArgs args) => {
            if (typeof(XRBaseControllerInteractor).IsAssignableFrom(args.interactor.GetType())) {
                State = BlobState.RELEASED;
            }        
        });
        
        State = BlobState.RELEASED;
        Rigidbody.useGravity = true;
        _grabLayer = Layers.KEEP;

    }
    public override void NetworkStart() {
        if (IsOwner) {    
            _color.Value = Colors[Random.Range(0, Colors.Count)];
            _scale.Value = StartScale;

            State = BlobState.RELEASED;
            Rigidbody.useGravity = true;
            _grabLayer = Layers.KEEP;
        } else {
            State = BlobState.NETWORK_CONTROLLED;
            Rigidbody.useGravity = false;
        }
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

    public void Destroy() {
        Destroy(gameObject);
    }


    public void SetDropOutVisual() {
        //Rigidbody.MovePosition(Rigidbody.position + new Vector3(0,2,0));
        Color = Color.Lerp(Color, Color.white, 0.7f);
    }
    
}
