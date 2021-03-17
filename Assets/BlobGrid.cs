using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

using System.Linq;
public class BlobGrid : MonoBehaviour
{
    public GameObject BlobPrefab;
    public GameObject SocketPrefab;

    // Dummy prefabs. Just for getting the layers.
    public GameObject DummyKeepPrefab;
    public GameObject DummyFallPrefab;
    public GameObject DummyOutPrefab;

    private LayerMask KeepLayer() {
        return DummyKeepPrefab.GetComponent<XRGrabInteractable>().interactionLayerMask;
    }
    
    private LayerMask FallLayer() {
        return DummyFallPrefab.GetComponent<XRGrabInteractable>().interactionLayerMask;
    }
    
    private LayerMask OutLayer() {
        return DummyOutPrefab.GetComponent<XRGrabInteractable>().interactionLayerMask;
    }

    int size = 4;
    int height = 8;
    float distance = 0.275f;
    int removeThreshold = 3;
    
    List<Color> colors = new List<Color> {Color.green, Color.magenta, Color.red, Color.yellow, Color.blue};
    private Node[,,] _grid;

    void Start()
    {      
        _grid = new Node[size, height, size];
        for(int x = 0; x < size; x++) {
          for(int y = 0; y < height; y++) {
              for(int z = 0; z < size; z++) {
                  InitNodeAt(new Position(x,y,z));
                }
            }  
        }
    }

// Update is called once per frame
    void Update()
    {

    }

    private void InitNodeAt(Position position)
    {
        float offset = size * distance / 2;
        
        GameObject blob = Instantiate(BlobPrefab);
        GameObject socket = Instantiate(SocketPrefab);

        blob.name = "blob " + position.ToString();
        socket.name = "socket " + position.ToString();
        
        var vector = new Vector3(position.X * distance - offset, position.Y * distance + 0.5f, position.Z * distance - offset);
        blob.transform.SetPositionAndRotation(vector, new Quaternion()); 

        //var color = colors[Random.Range(0, colors.Count)];
        var color = colors[(position.X + position.Y + position.Z) % (colors.Count - 1)];
        blob.GetComponent<Renderer> ().material.color = color;

        socket.transform.SetPositionAndRotation(vector, new Quaternion()); 

        var node = new Node {
            Blob = null, // will be blob when connecting to socket
            Socket = socket,
            Position = new Position(position.X, position.Y, position.Z),
            Color = color
            };

        var socketInteractor = socket.GetComponent<Socket>();
    
        socketInteractor.onSelectEntered.AddListener((_) => {
            var droppedBlob = _.gameObject;
            node.Blob = droppedBlob;
            node.Color = droppedBlob.GetComponent<Renderer> ().material.color;

            //var lowestFree = FindLowestFreeNode(node.Position.X, node.Position.Z); 
            //if (lowestFree != null && lowestFree.Position.Y < node.Position.Y) {
            //    FallAbove(lowestFree);
            //} else {
                var connectedNodes = ConnectedNodes(node);        
                if (connectedNodes.Count() >= removeThreshold) {
                    Debug.Log("connected: " + connectedNodes.Count + ", color: " + color);
                    StartCoroutine(DropOutInSeconds(connectedNodes, 0.5f));
                }
            //}

           
        });

        socketInteractor.onSelectExited.AddListener((_) => { 
            // dont init falling above blobs if exiting blob is a dop out. since drop out has special fall logic
            //if (node.Blob.GetComponent<XRGrabInteractable>().interactionLayerMask != OutLayer()) { 
            //    StartCoroutine(FallAboveIn(node, 0.5f));
            //}
            node.Blob = null;
        });


        _grid[position.X, position.Y, position.Z] = node;
    }
    private IEnumerator FallAboveIn(Node node, float t) {
        yield return new WaitForSeconds(t);
        FallAbove(node);
    }

    private Node FindLowestFreeNode(int x, int z) {
        for(int i = 0; i < height ; i++) {
            if (_grid[x,i,z].Blob == null)
            {
                Debug.Log("lowest free: " + _grid[x,i,z].Position);
                return _grid[x,i,z];
            }
        }
        return null;
    }

    private List<Node> GetAboveInOrder(Node node) {
        var above = new List<Node>();
        if (node.Position.Y == height - 1) {
            return above;
        }
        for( int i = node.Position.Y + 1; i < height ; i++) {
            above.Add(_grid[node.Position.X, i , node.Position.Z]);
        }
        return above;
    } 

    private IEnumerator DropOutInSeconds(List<Node> nodes, float t)
    {
        
        var rigidBodies = nodes.Select(node => node.Blob.GetComponent<Rigidbody>()).ToList();
        var blobGrabInteractables = nodes.Select(node => node.Blob.GetComponent<XRGrabInteractable>()).ToList();

        // scale mark as gone 
        foreach (var node in nodes) {
            node.Blob.transform.localScale = node.Blob.transform.localScale * 0.7f;
            
            var material = node.Blob.GetComponent<Renderer> ().material;
            material.color = Color.Lerp(material.color, Color.black, 0.8f);
            
            node.Blob = null; // (2) todo,might be dangerous this early 
        }

        //wait
        yield return new WaitForSeconds(t);
        
        // drop out
        foreach (var blobGrabInteractable in blobGrabInteractables) {
            blobGrabInteractable.interactionLayerMask = OutLayer();
        }
        // push a bit to not stuck while falling // todo maybe remove delay?
        StartCoroutine(ForceSoon(rigidBodies, 0.1f));


        //// fall above logic 
        //(1) lowest nodes with the same x z cordinates (lowest nodes dropping out), there should be a nicer way.... 
        var nodesByXZ = new Dictionary<(int, int), Node>();
        foreach (var n in nodes.OrderBy(n => n.Position.Y).ToList())
        {            
            var key = (n.Position.X, n.Position.Z);
            nodesByXZ[key] = nodesByXZ.ContainsKey(key) ? nodesByXZ[key] : n;
        }
        //var nodesByXZPosition = nodes
        //    .GroupBy( n => (n.Position.X, n.Position.Z))
        //    .OrderByDescending((key, n) => n.Position.Y);
        foreach(var bottonNode in nodesByXZ.Values) 
        {
            FallAbove(bottonNode);
        }
    }
    private void FallAbove(Node bottonNode) {
          // Release falling blobs 
            var aboveNodes = GetAboveInOrder(bottonNode).ToList();
            var fallingNodes = aboveNodes.Where(n => n.Blob != null).ToList();
            var fallingBlobs = fallingNodes.Select(n => n.Blob).ToList();


            //catch falling blob in bottom
            Debug.Log("falling blobs " + fallingBlobs.Count);

            if (fallingBlobs.Count() == 0) {
                return;
            }

            foreach (var fallingBlob in fallingBlobs)
            {
                fallingBlob.GetComponent<XRGrabInteractable>().interactionLayerMask = FallLayer();
            }
            foreach (var fallingNode in fallingNodes)
            {
                fallingNode.Blob = null; //might not be needed?
            }

            var bottonSocketInteractor = bottonNode.Socket.GetComponent<XRSocketInteractor>();

            var catchingNodes = new Node[]{bottonNode}.Concat(aboveNodes.Take(fallingNodes.Count - 1)).ToList();

            CatchFallingBlobs(catchingNodes);
    }

    // catchingNodes: ordered list of nodes with catching sockets.
    private void CatchFallingBlobs(List<Node> catchingNodes) {
        if (catchingNodes.Count == 0) {
            return;
        }
    
        var socketInteractor = catchingNodes.First().Socket.GetComponent<XRSocketInteractor>();
        socketInteractor.interactionLayerMask = FallLayer();

        UnityAction<XRBaseInteractable> action = null;
            action = (_) => {
                socketInteractor.onSelectEntered.RemoveListener(action);

                // catch regular blobs 
                socketInteractor.interactionLayerMask = KeepLayer();
                // make the catch blob not falling. 
                _.GetComponent<XRGrabInteractable>().interactionLayerMask = KeepLayer();
                /// todo take list of blob, interactor pairs, call itself here with next pair here to catch the rest.
                CatchFallingBlobs(catchingNodes.Skip(1).ToList());
            
            };
            socketInteractor.onSelectEntered.AddListener(action);

    }

    private IEnumerator DropAbove(List<Node> startNodes, float t) {
        yield return new WaitForSeconds(t);
        /*
        var aboveNodes = GetAboveInOrder(startNode);
        var fallingBlobs = aboveNodes.Where(node => node.Blob != null).Select(node => node.Blob);
         // make all above fall
        foreach (var blob in fallingBlobs)  {
              blob.GetComponent<XRGrabInteractable>().interactionLayerMask = LayerMask.NameToLayer(FALLING_LAYER_NAME);
        }
         
        

        var socketInterator = startNode.Socket.GetComponent<Socket>();
        UnityAction<XRBaseInteractable> action = null;
        action = (_) => {
            socketInterator.onSelectEntered.RemoveListener(action);
            foreach (var blob in fallingBlobs)  {
                blob.GetComponent<XRGrabInteractable>().interactionLayerMask = LayerMask.NameToLayer(KEEP_LAYER_NAME);
            }
            startNode.Socket.GetComponent<XRSocketInteractor>().interactionLayerMask = LayerMask.NameToLayer(KEEP_LAYER_NAME);
            
        };
        socketInterator.onSelectEntered.AddListener(action);

        startNode.Socket.GetComponent<XRSocketInteractor>().interactionLayerMask = LayerMask.NameToLayer(FALLING_LAYER_NAME);
       */

    }

    private IEnumerator ForceSoon(List<Rigidbody> rigidBodies, float t)
    {
        yield return new WaitForSeconds(t);
        foreach (var rigidBody in rigidBodies)
        {
            rigidBody.MovePosition(rigidBody.position + new Vector3(0.05f, 0.05f, 0.05f));
            //rigidBody.AddForce(new Vector3(10.05f, 10.05f, 10.05f));
        }
    }

    private List<Node> ConnectedNodes(Node node, HashSet<Node> visited = null) {
        visited = visited == null ? new HashSet<Node>() : visited;
        visited.Add(node);
        var toCheck = NeighboursFor(node)
            .Where(n => n.Color == node.Color)
            .Where(n => n.Blob != null)
            .Where(n => !visited.Contains(n));
        //Debug.Log("toCheck: " + toCheck.Count());

        var result = toCheck.SelectMany(nextNode => ConnectedNodes(nextNode, visited)).Concat(new[] { node });
        return result.ToList();
    } 

    private bool PositionInGrid(Position p) => p.X >= 0 && p.Y >= 0 && p.Z >= 0 && p.X < size && p.Y < height && p.Z < size;
     private Node NodeAt(Position p) {
        if (PositionInGrid(p)) {
            return _grid[p.X,p.Y,p.Z];
        } else {
            return null;
        }
    }
    
    private List<Node> NeighboursFor(Node node) {
        return directions.Select(direction => NodeAt(node.Position + direction)).Where(neighbour => neighbour != null).ToList();
    }
   List<Position> directions = new List<Position> {
        new Position(1, 0, 0),
        new Position(-1, 0, 0),
        new Position(0, 1, 0),
        new Position(0, -1, 0),
        new Position(0, 0, 1),
        new Position(0, 0, -1),
    };

    private class Node {
        public Position Position;
        public GameObject Blob;
        public GameObject Socket;
        public Color Color;
    }
    
    class Position {
        // Position NOT_IN_GRID = new Position(-100,-100,-100);
        public Position(int x, int y, int z) {X = x; Y = y; Z = z;} 
        
        public int X;
        public int Z;
        public int Y;
        public static Position operator +(Position a, Position b) => new Position(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Position operator -(Position a, Position b) => new Position(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static bool operator ==(Position a, Position b) => a.Equals(b);
        public static bool operator !=(Position a, Position b) => !a.Equals(b);

        public bool Equals(Position other) => X == other.X && Y == other.Y && Z == other.Z;
        //public override bool // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            return X == (obj as Position).X 
                && Y == (obj as Position).Y  
                && Z == (obj as Position).Z;
        }

        public override string ToString()
        {
            return "position x:" + X + " y:" + Y + " z:" + Z;
        }
        // override object.GetHashCode
        public override int GetHashCode()
        {
            // 17 and 23 prime
            int hash = 17;
            hash = hash * 23 + X.GetHashCode();
            hash = hash * 23 + Y.GetHashCode();
            hash = hash * 23 + Z.GetHashCode();
            return hash;
        }

    }

}
