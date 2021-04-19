using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

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

    public ulong? ClientOwnerId = null;


        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                StartButtons();
            }

            GUILayout.EndArea();
        }

        void StartButtons()
        {
            if (GUILayout.Button("dummy")) {
                Debug.Log("dummy");
            }
            if (GUILayout.Button("Host")) {
                HostGame();
            }
            if (GUILayout.Button("Client")) {
                JoinGame();
            }
        
        }
    //public override void NetworkStart()
    public void Start()
    {
        HostSocket.onSelectEntered.AddListener((_) => { HostGame(); });
        JoinSocket.onSelectEntered.AddListener((_) => { JoinGame(); });

        NetworkManager.Singleton.OnServerStarted += () => {Debug.Log("Server started");};

        NetworkManager.Singleton.OnClientConnectedCallback += (id) => {
            Debug.Log("Client joined: " + id);
            if  (IsHost) {
                Debug.Log("Start as Host");

                HostAI?.SetActive(true);
                HostGrid.PrepareStart();
                //ClientGrid.OtherClientOwnerId = id;
                //ClientGrid.PrepareStart();              
            }
            if (id == NetworkManager.Singleton.LocalClientId) {
                ClientAI?.SetActive(true);
                ClientGrid.network = this;
                ClientGrid.PrepareStart();           
                Debug.Log("Stop. Waiting for Host To start");
                CreateBlobForMeServerRpc(ClientGrid.transform.position + Vector3.up);
                //todo teleport   
            }
        };

        //HostSocket.onSelectExited.AddListener((_) => {});
        
    }

    private void HostGame() {        
                Debug.Log("Start Host.");
                HostGrid.Stop();
                ClientGrid.Stop(); 
                NetworkManager.Singleton.StartHost();
    }
    private void JoinGame() {
                Debug.Log("Start Client.");
                HostGrid.Stop();
                ClientGrid.Stop();  
                NetworkManager.Singleton.StartClient();
    }


    [ServerRpc(RequireOwnership = false)]
    public void CreateBlobForMeServerRpc(Vector3 position, ServerRpcParams rpcParams = default)
    {
        Debug.Log("CreateForMe from client id:" + rpcParams.Receive.SenderClientId);
        Blob.Instantiate(position, Color.yellow, rpcParams.Receive.SenderClientId);
    }
}
