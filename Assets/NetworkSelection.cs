using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.Transports.UNET;

public class NetworkSelection : NetworkBehaviour
{
     [SerializeField]
    public XRSocketInteractor HostSocket;
    [SerializeField]
    public XRSocketInteractor JoinSocket;


    public BlobGrid HostGrid;
    public BlobGrid ClientGrid;


    public GameObject HostAI;
    public GameObject ClientAI;

    public XRRig xrRig;

    private bool _runnigOnPc = false;
    public ulong? ClientOwnerId = null;


        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                StartButtons();
            }// else {
                if (GUILayout.Button("AIHost")) {
                    HostAI?.GetComponent<AI>()?.AssignToGrid(HostGrid);
                }
                if (GUILayout.Button("AIClient")) {
                    ClientAI?.GetComponent<AI>()?.AssignToGrid(ClientGrid);
                }
                if (GUILayout.Button("Disconnect")) {
                    Disconnect();
                }
            //}
           

            GUILayout.EndArea();
        }

        void StartButtons()
        {            
            if (GUILayout.Button("Server")) {
                _runnigOnPc = true;
                StartServer();
            }
            if (GUILayout.Button("Host")) {
                _runnigOnPc = true;
                HostGame();
            }
            if (GUILayout.Button("Client")) {
                _runnigOnPc = true;
                JoinGame();
            }
            if (GUILayout.Button("prepare")) {
                HostGrid.PrepareStart();
                ClientGrid.PrepareStart();
            }
            if (GUILayout.Button("start")) {
                HostGrid.StartGame();
                ClientGrid.StartGame(); 
            }
            if (GUILayout.Button("stop")) {
                HostGrid.Stop();
                ClientGrid.Stop();  
                Debug.Log("dummy stop only");
            }
        }
    //public override void NetworkStart()
    public void Start()
    {
        HostSocket.onSelectEntered.AddListener((_) => { HostGame(); });
        JoinSocket.onSelectEntered.AddListener((_) => { JoinGame(); });
        HostSocket.onSelectExited.AddListener((_) => { Disconnect(); });
        JoinSocket.onSelectExited.AddListener((_) => { Disconnect(); });

        NetworkManager.Singleton.OnServerStarted += () => {
                Debug.Log("Server started");
            };

        NetworkManager.Singleton.OnClientDisconnectCallback += (id) => {Disconnect();};
        NetworkManager.Singleton.OnClientConnectedCallback += (id) => {
            Debug.Log("Client joined: " + id);
            Debug.Log("Connected clients: " + NetworkManager.Singleton.ConnectedClients.Count);
            Debug.Log("Connected clients: " + NetworkManager.Singleton.ConnectedClients.Keys);
            if  ((IsServer || IsHost) && NetworkManager.Singleton.ConnectedClients.Count == 2) {
                Debug.Log("Start game: " + NetworkManager.Singleton.ConnectedClients.Count);
                

                StartClientRpc(
                    HostGrid.transform.position,
                    new ClientRpcParams() {
                        Send = new ClientRpcSendParams() {
                            TargetClientIds = new ulong[]{NetworkManager.Singleton.ConnectedClients.Keys.ToList()[0]}
                        }
                });
                StartClientRpc(
                    ClientGrid.transform.position,
                    new ClientRpcParams() {
                        Send = new ClientRpcSendParams() {
                            TargetClientIds = new ulong[]{NetworkManager.Singleton.ConnectedClients.Keys.ToList()[1]}
                        }
                });

            }

            if  (IsServer) {
                Debug.Log("Is server");
            }
            if  (IsHost) {
                Debug.Log("Is Host");
                //HostAI?.SetActive(true);
                //HostGrid.PrepareStart();

                //ClientGrid.OtherClientOwnerId = id;
                //ClientGrid.PrepareStart();              
            }
            if (id == NetworkManager.Singleton.LocalClientId) {
                Debug.Log("Is Client");                
                //ClientAI?.SetActive(true);
                //ClientGrid.network = this;
                //ClientGrid.PrepareStart();           
                //CreateBlobForMeServerRpc(ClientGrid.transform.position + Vector3.up);
                //todo teleport   
            } 
            
        };

        //HostSocket.onSelectExited.AddListener((_) => {});
        
    }

    private void StartServer() {        
                Debug.Log("Start Server.");
                HostGrid.Stop();
                ClientGrid.Stop(); 
                //NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = " 192.168.1.190";// "127.0.0.1";
                NetworkManager.Singleton.StartServer();
    }
    private void HostGame() {        
                Debug.Log("Start Host.");
                HostGrid.Stop();
                ClientGrid.Stop(); 
                //NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = " 192.168.1.190";// "127.0.0.1";
                NetworkManager.Singleton.StartHost();
    }
    private void JoinGame() {
                Debug.Log("Start Client.");
                HostGrid.Stop();
                ClientGrid.Stop();  
                NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = "192.168.1.190";// "2020 pc ip";
                //NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = "192.168.1.146";// "basement"
            

                NetworkManager.Singleton.StartClient();
    }
    private void Disconnect() {
        //NetworkManager.Singleton.DisconnectClient();
        Debug.Log("Disconnect");

        Debug.Log("Stop grids");
        HostGrid.Stop();
        ClientGrid.Stop();  

        //var blobs = FindObjectsOfType<Blob>();
        
        if (IsServer) {
            Debug.Log("stop server");
            NetworkManager.Singleton.StopServer();
        }
        if (IsClient) {
            Debug.Log("stop client");
            NetworkManager.Singleton.StopClient();
        }
        if (IsHost) {
            Debug.Log("stpp host");
            NetworkManager.Singleton.StopHost();
        }
        //Debug.Log("Load Scene");
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    [ClientRpc]
    public void StartClientRpc(Vector3 position, ClientRpcParams rpcParams = default)
    {
        var myId = NetworkManager.Singleton.LocalClientId;
        Debug.Log("start client (" + myId + ") rpc");

        MoveRig(position);

        if (position == HostGrid.transform.position) {
            Debug.Log("first position");    
            HostGrid.network = IsClient ? this : null; 
            HostGrid.PrepareStart();  
        } else if (position == ClientGrid.transform.position) {
            Debug.Log("seccond position");
            ClientGrid.network = IsClient ? this : null; 
            ClientGrid.PrepareStart();  
        } else {
            Debug.Log("position: " + position);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CreateBlobForMeServerRpc(Vector3 position, ServerRpcParams rpcParams = default)
    {
        //Debug.Log("CreateForMe from client id:" + rpcParams.Receive.SenderClientId);
        Blob.Instantiate(position, Color.yellow, rpcParams.Receive.SenderClientId);
    }
    private void MoveRig(Vector3 position) {
        var heightAdjustment = xrRig.rig.transform.up * xrRig.cameraInRigSpaceHeight;
        var cameraDestination = position + heightAdjustment;
        if (!_runnigOnPc) {
            xrRig.MoveCameraToWorldLocation(cameraDestination);
        }
    }
}
