using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

using System.Linq;


/*

To-do list:

bugs
* when blobs fall fast they fall out of teir sockets., remove interpolate solves it but that is choppy....

bigger things
* sound efects 
* background sound
* environments
* vizualize controllers, catch blobs away from hand.
* penelty when loosing a ball. big black block. lives
* in game menu (pip boy?)
* pre game menu, it would be cool if you dont have pre-menu and spawn diectly in the world to fool around. you can configure and join games from tthe world. 
* game mode 2d would still be fun i think 
* point counter 
* difficulity increesed
* cieling (fall out)
* merge connecting blobs 
* warning on stak close maxed out (sound? arrow?)
* hearts in blobs, release to gain health 



big things
* multiplayer


ideas
Cooperative work on the same stack/grid 2 vs 0 2 vs 2 

Mariokart boxes, you get random boxes of "things you can do" when executing combos. but can only keep 1, 2, 3 specials. 
catch up, player behind might get better items like in marikart. (who is behind i dont know..)..
box power could be
* send different kind of blocks to the other player, like pyo pyo stones
* spawn at the other players stack to change there blobs for a short time, dropping blobs end powerup.
* throw things at the other player,(dont know what) something that require 
* fog foe the other players
* pixel camera for the other players hope it is dizzy proof. like this: https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/pixelation-65554?aid=1100l355n&gclid=Cj0KCQjwl9GCBhDvARIsAFunhsnxGxkAdjtTEYirZv-vhIGEsPDZ93_kD2XYbR5LK5CI16obsGbkI6kaAp2vEALw_wcB&pubref=UnityAssets%2ADynNew08%2A1723478829%2A67594162255%2A336277500151%2Ag%2A%2A%2Ab%2Ac%2Agclid%3DCj0KCQjwl9GCBhDvARIsAFunhsnxGxkAdjtTEYirZv-vhIGEsPDZ93_kD2XYbR5LK5CI16obsGbkI6kaAp2vEALw_wcB&utm_source=aff
* release other players fillup.
* randomise other players stgack
* pause fillup
* big release of random blobs on top of stack
* randomise a slice of other players stack
* dynamite, radial dstuction of blobs.  time it with the fuse. 
* rotate other players stack keeping the blob relations but a bit confusing

handicap worse player need lesser combos for powerups 

*/
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

    int size = 3;
    int height = 7;
    int startHeight = 4;
    float distance = 0.3f;//0.275f;
    int removeThreshold = 4;

    float spawnTimeInSeconds = 2.1f;
    
    List<Color> colors = new List<Color> {Color.green, Color.magenta, Color.red, Color.yellow, Color.blue};
    private Node[,,] _grid;
    private Node[,,] _fillerGrid;

    private int fillerGridYPosition = 8; // higher then height

    void Start()
    {      
        _grid  = new Node[size, height, size];
        for(int x = 0; x < size; x++) {
          for(int y = 0; y < height; y++) {
              for(int z = 0; z < size; z++) {
                  var position = new Position(x,y,z);
                  InitNodeAt(position, GetPositionVector(position),  _grid, y < startHeight);
                }               
            }  
        }
        _fillerGrid = new Node[size, 1, size];
        for(int x = 0; x < size; x++) {
            int y = 0;
            for(int z = 0; z < size; z++) {
                var position = new Position(x,y,z);
                InitNodeAt(position, GetPositionVector(position, fillerGridYPosition), _fillerGrid, false, false);
            }                           
        }
     InvokeRepeating("FillFiller", 1f, spawnTimeInSeconds);
    }

    void FillFiller() {
        for(int x = 0; x < size; x++) {
            int y = 0;
            for(int z = 0; z < size; z++) {
                if (_fillerGrid[x,y,z].Blob == null) {
                    var position = new Position(x,y,z);
                    var vector = GetPositionVector(position, fillerGridYPosition); 
                    CreateBlob(position, vector, true); 
                    return;
                }

            }                           
        }
        // Fall
        for(int x = 0; x < size; x++) {
            int y = 0;
            for(int z = 0; z < size; z++) {
                var lowestFree = FindLowestFreeNodeOnTop(x, z); 
                _fillerGrid[x,y,z].Blob.GetComponent<XRGrabInteractable>().interactionLayerMask = FallLayer();
                _fillerGrid[x,y,z].Blob.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;                
                _fillerGrid[x,y,z].Blob = null;
                if (lowestFree != null ){
                    CatchFallingBlobs(new List<Node>(){lowestFree}); 
                }
            }
        }
    }
 

// Update is called once per frame
    void Update()
    {

    }

private void CreateBlob(Position position, Vector3 vector, bool randomColor = false) {    
    GameObject blob = Instantiate(BlobPrefab);
    blob.name = "blob " + position.ToString();
    blob.transform.SetPositionAndRotation(vector, new Quaternion()); 
    var color = randomColor ? colors[Random.Range(0, colors.Count)] : colors[(position.X + position.Y + position.Z) % (colors.Count - 1)];    
    blob.GetComponent<Renderer> ().material.color = color;
}
private Vector3 GetPositionVector(Position position, int yStart = 0) {
    float offset = size * distance / 2;
    return new Vector3(position.X * distance - offset, (position.Y + yStart)* distance + 0.5f, position.Z * distance - offset);
}

    private void InitNodeAt(Position position, Vector3 vector, Node[,,] grid, bool addBlob, bool connectionListener = true)
    {   
        
        if (addBlob) { // todo break out
            CreateBlob(position, vector);
        }

        GameObject socket = Instantiate(SocketPrefab);
        socket.name = "socket " + position.ToString();       

        socket.transform.SetPositionAndRotation(vector, new Quaternion()); 

        var node = new Node {
            Blob = null, // will be blob when connecting to socket
            Socket = socket,
            Position = new Position(position.X, position.Y, position.Z),
            Color = Color.white // just something not used
            };

        var socketInteractor = socket.GetComponent<Socket>();

        grid[position.X, position.Y, position.Z] = node;

        socketInteractor.onSelectEntered.AddListener((_) => { // have a queue for multiple trigger at the same time ?
            var droppedBlob = _.gameObject;
            node.Blob = droppedBlob;
            node.Color = droppedBlob.GetComponent<Renderer> ().material.color;

            //droppedBlob.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;

            if (connectionListener) {
                var connectedNodes = ConnectedNodes(node);        
                if (connectedNodes.Count() >= removeThreshold) {
                    Debug.Log("connected: " + connectedNodes.Count + ", color: " + node.Color);
                    StartCoroutine(DropOutInSeconds(connectedNodes, 0.5f));
                }
            }

        });

        socketInteractor.onSelectExited.AddListener((_) => { 
            node.Blob = null;
        });


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
    private Node FindLowestFreeNodeOnTop(int x, int z) {
        if (_grid[x, height - 1, z].Blob != null) { // top
            return null;
        }

        for(int i = height - 2; i >= 0 ; i--) {
            if (_grid[x, i, z].Blob != null)
            {
                return _grid[x, i + 1, z];
            }
        }
        return _grid[x, 0, z]; // bottom
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
                fallingBlob.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;                

                
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
    private void CatchFallingBlobs(List<Node> catchingNodes) { // todo: what if falling is blocked?
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
