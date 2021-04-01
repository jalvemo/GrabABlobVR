﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

using System.Linq;


/*

sounds: 
https://mixkit.co/free-sound-effects/game/?page=2

To-do list:


bugs
* when blobs fall fast they fall out of teir sockets., i removed interpolate solves it but that is choppy....look at collition detection mode?
* when falling blob miss the socket the socket is lost waiting for falling blob. 
* all blobs dont go in to a combo if one blabb is falling from high. connecting blobs disapear before far falling blob connects. 

small thing:

bigger things
* penelty when loosing a ball. big black block. lives / cieling (fall out) life counter 
* pre game menu, it would be cool if you dont have pre-menu and spawn diectly in the world to fool around. you can configure and join games from tthe world. 
* game mode 2d would still be fun i think 
* point counter 
* difficulity increesed
* merge connecting blobs visually
* warning on stak close maxed out (sound? arrow?)
* hearts in blobs, release to gain health 
* characta inspo:
    * https://joslin.artstation.com/projects/KgdLX
* Score should take level/fall-rate in to account. It is a lot easier to do combos in the begining. 

improve working version:
* background sound
* sound efects 
* in game menu (pip boy?)
* imptove controllers, catch blobs away from hand.
* environments


big things
* multiplayer
* AI 

ideas
Cooperative work on the same stack/grid 2 vs 0 2 vs 2 

bals get up like the marble macheene x. maybe also generates music?

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
* AI controllers helping you out.
* AI controllers sabotaging.

handicap worse player need lesser combos for powerups 


==AI==
* identify clusters of blobs organize them by color 
* Find first move(s) that generate a fall at a rate. If 
* Find move generate fall, and try to make another move first that will generate combo.


Dificulty couold be
 * Rate of each check, how nammy chacks of each per seccond.
 * Max move rate. like  2 moves / 2 secconds
 * Arm speed

*/
public class BlobGrid : MonoBehaviour
{
    public XRInteractionManager interactionManager;
    public GameObject BlobPrefab;
    public GameObject SocketPrefab;
    public GameObject AIHandPrefab;
    public Light lighting;
    public ScoreBoard ScoreBoard;
    // Dummy prefabs. Just for getting the layers.
    public GameObject DummyKeepPrefab;
    public GameObject DummyFallPrefab;
    public GameObject DummyOutPrefab;
    public LayerMask KeepLayer() {
        return DummyKeepPrefab.GetComponent<XRGrabInteractable>().interactionLayerMask;
    }
        
    public  LayerMask FallLayer() {
        return DummyFallPrefab.GetComponent<XRGrabInteractable>().interactionLayerMask;
    }
        
    public LayerMask OutLayer() {
        return DummyOutPrefab.GetComponent<XRGrabInteractable>().interactionLayerMask;
    }   

    public enum Layer {KEEP, Fall, OUT}
    public static Dictionary<Layer, LayerMask> layers = new Dictionary<Layer, LayerMask>();

    private AudioSource audioSource;
    public AudioClip pop;
    public AudioClip noFit;
    public AudioClip gameOver;
    public AudioClip fallSound;
    public AudioClip levelUpSound;



    int width = 3;
    int height = 6;
    int startHeight = 4;
    float distance = 0.3f;//0.275f;
    
    public SocketSelector removeThresholdSelector;

    public SocketSelector dropDelay;
    
    public SocketSelector nextWidth;
    public SocketSelector nextHeight;
    public SocketSelector nextStartHeight;

    List<Color> colors = new List<Color> {Color.green, Color.magenta, Color.red, Color.yellow, Color.blue};
  
    private Socket[,,] _grid;
    private Socket[,,] _fillerGrid;

    private int fillerGridYPositionRelative = 0;
    private float levelSpeedChange = 2.0f / 3.0f;
    // level  0-10 : 2.0 1.3, .88 .59 .39 .26 .17 .11 .07 .05 .034
    private float levelUpWaitSeconds = 20;

    private void LevelUp() {
        Invoke("LevelUp", levelUpWaitSeconds);
        dropDelay.curentValue = dropDelay.curentValue * levelSpeedChange;
        audioSource.PlayOneShot(levelUpSound);
        Debug.Log("Level up speed: " + dropDelay.curentValue);
        ScoreBoard.LevelUp();
    }
    public void Restart() {
        Stop();
        Invoke("Start", 1.5f);
        
    }
    public void Stop() {
        CancelInvoke("LevelUp");
        CancelInvoke("FillFiller");
        System.Action<Socket> cleanSocket = (socket) => {
            interactionManager.UnregisterInteractable(socket.Interactable);
            // todo: find to reuse or actually remove theese.
            socket.enableInteractions = false;
            //socket.Socket.GetComponent<XRSocketInteractor>().enableInteractions = false;
        };

        foreach (var socket in _grid) { cleanSocket.Invoke(socket); }
        foreach (var socket in _fillerGrid) { cleanSocket.Invoke(socket); }

        //Start();
    }

    private class AI {
        static System.Random random = new System.Random();
        
        private class Hand {
            public Vector3 Position = new Vector3(0,0,0);
            public Position MoveTo = null;
            public Position Take = null;
            public Blob Blob;

            public GameObject HandVisual;


            List<(Position, System.Action)> actions = new List<(Position, System.Action)>();
            public void ReleseBlob() {
                Blob.Rigidbody.velocity = Vector3.zero;
                MoveTo = null;
                Blob.interactionLayerMask = BlobGrid.layers[Layer.KEEP];
                Blob = null;
            }
            public void PickUpBlob(Socket socket) {
                var blob = socket.Blob;
                if (blob != null) {
                    Debug.Log("SetDropOutVisual Rigidbody.isKinematic = " + blob.Rigidbody.isKinematic);


                    Blob = blob;
                    blob.interactionLayerMask = BlobGrid.layers[Layer.OUT];
                    //blob.Rigidbody.isKinematic = true;
                    Take = null;
                } else {  
                    Debug.Log("no blob to pickup.");
                    Take = null;
                    MoveTo = null;
                }
            }


            public bool IsFree() {
                return Blob == null && MoveTo == null;
            }
            public bool IsIdle() {
                return IsFree() && Take == null;
            }

        }
        private Hand _left = new Hand();
        private Hand _right = new Hand();
        private BlobGrid _grid;

        private List<Hand> _hands = new List<Hand>();

        public AI(BlobGrid grid, GameObject handPrefab) {
            _grid = grid;
            _hands.Add(_left);
            _hands.Add(_right);

            _left.HandVisual = Instantiate<GameObject>(handPrefab);
            _right.HandVisual = Instantiate<GameObject>(handPrefab);
        }
        private bool BothHandsFree() {
            return _hands.All(_ => _.IsFree());
        }
        private Hand OtherHand(Hand hand) {
            return hand == _left ? _right : _left;
        }
        public void Update() {
            foreach (Hand hand in _hands) {
                if (hand.IsIdle()) {
                    var occupied = _grid.All()
                    .Where(_ => _.Blob != null)
                    .Where(_ => OtherHand(hand).Take != _.GridPosition)
                    .ToList();

                    var free = _grid.All()
                    .Where(_ => _.Blob == null)
                    .Where(_ => _.GridPosition.Y < _grid.height - 1)
                    .Where(_ => OtherHand(hand).MoveTo != _.GridPosition)
                    .ToList();

                    if (free.Count != 0 && occupied.Count != 0) {

                        var top = occupied.Find(_ => _.GridPosition.Y == _grid.height - 1);
                    
                        var take = top != null ? top : occupied[random.Next(occupied.Count)];
                        
                        var sameColorNeighbours = free.Where(_ => _grid.NeighboursFor(_).Exists(n => n.Blob != null && n.Blob.Color == take.Blob.Color)).ToList();
                        if (sameColorNeighbours.Count == 0) {
                            break;
                        }
                        
                        var to = sameColorNeighbours[random.Next(sameColorNeighbours.Count)];
                        //var to = free[random.Next(free.Count)];

                        hand.Take = take.GridPosition;
                        hand.MoveTo = to.GridPosition;
                        Debug.Log("From :" + take + "To: " + to);
                    }
                }
            }

            foreach (Hand hand in _hands) {
                if (hand.Take != null || hand.MoveTo != null) { // todo:  what if it is not moving but holing ablob for some reason...
                    var target = _grid.SocketAt(hand.Take != null ? hand.Take : hand.MoveTo).transform.position;
                   
                                      
                    var path = target - hand.Position; // vector beween the hand and destination
                    var step = 2.2f * Time.deltaTime;
                    
                    if (hand.Position == target) {
                        // todo: if (_grid.SocketAt(MovingTo).Blob == null) {}
                        // RELEASE  blob 
                        if (hand.Blob == null && hand.Take != null) {
                            //hand.PickUpBlob
                            var socket = _grid.SocketAt(hand.Take);
                            hand.PickUpBlob(socket);
                        } else if (hand.Blob != null && hand.MoveTo != null) {
                            // hand.Blob.Rigidbody.MovePosition(hand.Position);
                            hand.ReleseBlob();
                            
                            hand.MoveTo = null;
                        } else {
                            Debug.Log("EHHH what do we do now?");
                        }
                        
                    } else {
                        if (Vector3.Distance(hand.Position, target) < step) {
                            hand.Position = target;                    
                        } else {
                            hand.Position = Vector3.MoveTowards(hand.Position, target, step);
                        }
                        if (hand.Blob != null) {
                            hand.Blob.Rigidbody.MovePosition(hand.Position);
                        }
                        if (hand.HandVisual != null) {
                            hand.HandVisual.transform.SetPositionAndRotation(hand.Position, new Quaternion());
                        }
                    }                    
                    
                }

                // move blobs
            }    
        }
        
    }
    

    private Blob AIPickUp(Position position) {
        var socket = SocketAt(position);
        var blob = socket.Blob;
        blob.interactionLayerMask = OutLayer();
        //socket.Blob = null;
        //StartCoroutine(0.1f, () => blob.Rigidbody.MovePosition(new Vector3(0,10,0)));       
        return blob;
    }
    public void AIPlace(Blob blob, Position position) {
        blob.Rigidbody.MovePosition(_grid[position.X, position.Y, position.Z].transform.position);
        blob.interactionLayerMask = KeepLayer();
    }

    private void AIMove() {
        var blob1 = AIPickUp(new Position(0,0,0));
        var blob2 = AIPickUp(new Position(2,1,2));

        if (blob1 != null) {
            StartCoroutine(2.0f, () => AIPlace(blob1, new Position(2,1,2)));
        }
        if (blob2 != null) {
            StartCoroutine(2.0f, () => AIPlace(blob2, new Position(0,0,0)));
        }
    }
    private List<AI> _ais = new List<AI>();
    void Start()
    {   
        _ais.Add(new AI(this, AIHandPrefab));

        if (layers.Count == 0) {
            layers[Layer.KEEP] = KeepLayer();
            layers[Layer.OUT] = OutLayer();
            layers[Layer.Fall] = FallLayer();
        }

        //StartCoroutine(3.0f, () => AIMove());   

        audioSource = GetComponent<AudioSource>();
        sequencialDropFailCount = 0;
        ScoreBoard.ResetBoard();

        _grid  = new Socket[width, height, width];
        for(int x = 0; x < width; x++) {
            for(int y = 0; y < height; y++) {
                for(int z = 0; z < width; z++) {
                    var position = new Position(x,y,z);
                    var positionVector =  GetPositionVector(position);
                    if (y < startHeight) {
                    CreateBlob(position, positionVector);
                    }
                    InitSocketAt(position,positionVector, _grid);
                }               
            }  
        }
        _fillerGrid = new Socket[width, 1, width];
        for(int x = 0; x < width; x++) {
            int y = 0;
            for(int z = 0; z < width; z++) {
                var position = new Position(x,y,z);
                InitSocketAt(position, GetPositionVector(position, fillerGridYPositionRelative + height), _fillerGrid, false);
            }                           
        }

    nextWidth.onChanged = () => {
        width = nextWidth.GetInt();
        Restart();
    };

    nextHeight.onChanged = () => {
        height = nextHeight.GetInt();
        Restart();
    };

    nextStartHeight.onChanged = () => {
        startHeight = nextStartHeight.GetInt();
        Restart();
    };

        Invoke("FillFiller", dropDelay.curentValue);
        Invoke("LevelUp", levelUpWaitSeconds);
        
        dropDelay.onChanged = () => {
            CancelInvoke("FillFiller");
            Invoke("FillFiller", dropDelay.curentValue);
        };
    }

    private float _fillPauseDelay = 0.0f;      
    private int gameOverDropFailThreshhold = 2;
    private int sequencialDropFailCount = 0;

    void FillFiller() {
        // Wait for Combo 
        if (_fillPauseDelay > 0.0f) {
            Invoke("FillFiller", _fillPauseDelay);
            _fillPauseDelay = 0.0f;
            ScoreBoard.ApplyScore();
            return;
        }    

        // Fill
        for(int x = 0; x < width; x++) {
            int y = 0;
            for(int z = 0; z < width; z++) {
                if (_fillerGrid[x,y,z].Blob == null) {
                    var position = new Position(x,y,z);
                    var vector = GetPositionVector(position, fillerGridYPositionRelative + height); 
                    CreateBlob(position, vector, true); 
                    Invoke("FillFiller", dropDelay.curentValue);
                    return;
                }

            }                           
        }
        // Fall
        bool failed = false;
        for(int x = 0; x < width; x++) {
            int y = 0;
            for(int z = 0; z < width; z++) {
                var lowestFree = FindLowestFreeSocketOnTop(x, z); 
                _fillerGrid[x,y,z].Blob.interactionLayerMask = FallLayer();             
                _fillerGrid[x,y,z].Blob = null;
                if (lowestFree != null ){
                    CatchFallingBlobs(new List<Socket>(){ lowestFree }); 
                } else {
                    failed = true;
                }
            }
        }
        // top out logic
        if (failed) {
            sequencialDropFailCount++;
            if (sequencialDropFailCount >= gameOverDropFailThreshhold) {
                audioSource.PlayOneShot(gameOver);
                Stop();
            } else {
                audioSource.PlayOneShot(noFit);
                lighting.color = Color.red;
            }
        } else {
            sequencialDropFailCount = 0;
            lighting.color = Color.white;
            audioSource.PlayOneShot(fallSound, 1.0f);

        }
        Invoke("FillFiller", dropDelay.curentValue);
    }

    void FixedUpdate() {
        _ais.ForEach(_ => _.Update());
    }
    void Update()
    {
        //_ais.ForEach(_ => _.Update());

        if (sequencialDropFailCount != 0) {
            var val = Time.time % 2.0f;
            if (val <= 1.0f) {
                lighting.color = new Color(1, 1.0f - val, 1.0f - val);
            } else {
                lighting.color = new Color(1, val - 1.0f,  val - 1.0f);
            }
        }
    }

    private void CreateBlob(Position position, Vector3 vector, bool randomColor = false) {    
        Blob blob = Instantiate(BlobPrefab).GetComponent<Blob>();
        blob.name = "blob " + position.ToString();
        blob.transform.SetPositionAndRotation(vector, new Quaternion()); 
        var color = randomColor ? colors[Random.Range(0, colors.Count)] : colors[(position.X + position.Y + position.Z) % (colors.Count - 1)];    
        blob.Color = color;
    }
    private Vector3 GetPositionVector(Position position, int yStart = 0) {
        float offset = width * distance / 2;
        return new Vector3(position.X * distance - offset, (position.Y + yStart)* distance + 0.5f, position.Z * distance - offset);
}

    private void InitSocketAt(Position position, Vector3 vector, Socket[,,] grid, bool connectionListener = true)
    {   
        Socket socket = Instantiate(SocketPrefab).GetComponent<Socket>();
        socket.name = "socket " + position.ToString();       
        socket.transform.SetPositionAndRotation(vector, new Quaternion()); // need game object?
    
        socket.GridPosition = new Position(position.X, position.Y, position.Z);
        socket.DropCheckActive = connectionListener;

        grid[position.X, position.Y, position.Z] = socket;

        socket.onSelectEntered.AddListener((_) => { // have a queue for multiple trigger at the same time ?
            var droppedBlob = _.GetComponent<Blob>();
            if (droppedBlob == null) { // it was not a blob
                return;
            }
            BlobDroppedInSocket(droppedBlob, socket);
        });

        socket.onSelectExited.AddListener((_) => { // move to socket..
            socket.Blob = null;
        });
    }

    private void BlobDroppedInSocket(Blob droppedBlob, Socket socket) {
        socket.Blob = droppedBlob;
            if (socket.DropCheckActive) {
                StartCoroutine(checkForConnctedSocketDrop(socket, 0.1f)); //0.1 allows some time to drop 2 connected blobs
            }
    }

    private IEnumerator checkForConnctedSocketDrop(Socket socket, float t) {
        yield return new WaitForSeconds(t);
        
        var connectedSockets = ConnectedSockets(socket);        
        if (connectedSockets.Count() >= removeThresholdSelector.GetInt()) {
            Debug.Log("connected: " + connectedSockets.Count + ", color: " + socket.Blob.Color);
            StartCoroutine(DropOutInSeconds(connectedSockets, 0.5f));
        }
    }

    // where new falling blobs will fall to 
    private Socket FindLowestFreeSocketOnTop(int x, int z) {
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

    private List<Socket> GetAboveOrderedLowestFirst(Socket socket) {
        var above = new List<Socket>();
        if (socket.GridPosition.Y == height - 1) {
            return above;
        }
        for( int i = socket.GridPosition.Y + 1; i < height ; i++) {
            above.Add(_grid[socket.GridPosition.X, i , socket.GridPosition.Z]);
        }
        return above;
    } 


    private IEnumerator BlobDropAudioVisual(Blob blob, float t) {
        yield return new WaitForSeconds(t);
        blob.SetDropOutVisual();
        audioSource.PlayOneShot(pop);

    }

    private IEnumerator DropOutInSeconds(List<Socket> sockets, float t)
    {
        var blobs = sockets.Select(socket => socket.Blob).ToList();    

        // scale mark as gone 
        for(int i = 0; i < sockets.Count; i++) {
            StartCoroutine(BlobDropAudioVisual(sockets[i].Blob, i * t / sockets.Count));
            //var blob =  sockets[i].Blob;
            //StartCoroutine(i * t / sockets.Count,
            //    () => {
            //        blob.SetDropOutVisual();
            //        audioSource.PlayOneShot(pop);
            //    }
            //);
            sockets[i].Blob = null; // (2) todo,might be dangerous this early 
        }

        _fillPauseDelay = t; // pause filling while falling..
    
        // score
        ScoreBoard.AddScore(blobs); 

        //wait
        yield return new WaitForSeconds(t);
        
        // drop out
        foreach (var blob in blobs) {
            blob.interactionLayerMask = OutLayer();
        }


        // push a bit to not stuck while falling // todo maybe remove delay?
        StartCoroutine(0.1f, () => {
             foreach (var blob in blobs) {
                    blob.Rigidbody.MovePosition(blob.Rigidbody.position + new Vector3(0.05f, 0.05f, 0.05f));
                    //rigidBody.AddForce(new Vector3(10.05f, 10.05f, 10.05f));
                }
        });

        //// fall above logic 
        //(1) lowest sockets with the same x z cordinate (lowest socket dropping out will start catching falling blobs), there should be a nicer way.... 
        var socketsByXZ = new Dictionary<(int, int), Socket>();
        foreach (var s in sockets.OrderBy(s => s.GridPosition.Y).ToList())
        {            
            var key = (s.GridPosition.X, s.GridPosition.Z);
            socketsByXZ[key] = socketsByXZ.ContainsKey(key) ? socketsByXZ[key] : s;
        }
        //var socketsByXZPosition = sockets
        //    .GroupBy( n => (n.Position.X, n.Position.Z))
        //    .OrderByDescending((key, n) => n.Position.Y);
        foreach(var bottonSocket in socketsByXZ.Values) 
        {
            FallAbove(bottonSocket);
        }
    }
    private void FallAbove(Socket bottomCatchingSocket) {
        // Release falling blobs 
        var aboveSockets = GetAboveOrderedLowestFirst(bottomCatchingSocket).ToList();
        var socketsWithFallingBlobs = aboveSockets.Where(s => s.Blob != null).ToList();
        var fallingBlobs = socketsWithFallingBlobs.Select(s => s.Blob).ToList();


        //catch falling blob in bottom
        Debug.Log("falling blobs " + fallingBlobs.Count);

        if (fallingBlobs.Count() == 0) {
            return;
        }

        foreach (var fallingBlob in fallingBlobs)
        {
            fallingBlob.interactionLayerMask = FallLayer();                          
        }
        foreach (var fallingSocket in socketsWithFallingBlobs)
        {
            fallingSocket.Blob = null; //might not be needed?
        }

        var catchingSockets = new Socket[]{bottomCatchingSocket}.Concat(aboveSockets.Take(socketsWithFallingBlobs.Count - 1)).ToList();

        CatchFallingBlobs(catchingSockets);
    }

    // catchingSockets: ordered list of sockets with catching falling blobs.
    private void CatchFallingBlobs(List<Socket> catchingSockets) { // todo: what if falling is blocked?
        if (catchingSockets.Count == 0) {
            return;
        }
    
        var nextCatchingSocket = catchingSockets.First();
        nextCatchingSocket.interactionLayerMask = FallLayer();

        UnityAction<XRBaseInteractable> catchFallingBlob = null;
            catchFallingBlob = (_) => {
                var catchedBlob = _.GetComponent<Blob>();
                if (catchedBlob == null) {
                    Debug.Log("Wops - Tried to catch falling blob, but was no blob: " + _);
                    return;
                }

                nextCatchingSocket.onSelectEntered.RemoveListener(catchFallingBlob);
                // catch regular blobs 
                nextCatchingSocket.interactionLayerMask = KeepLayer();
                // make the catch blob not falling. 

                catchedBlob.interactionLayerMask = KeepLayer();
                CatchFallingBlobs(catchingSockets.Skip(1).ToList());
            };
            nextCatchingSocket.onSelectEntered.AddListener(catchFallingBlob);
    }


    private List<Socket> ConnectedSockets(Socket socket, HashSet<Socket> visited = null) {
        if (socket.Blob == null) {
            return new List<Socket>();
        }
        visited = visited == null ? new HashSet<Socket>() : visited;
        visited.Add(socket);
        var toCheck = NeighboursFor(socket)
            .Where(s => s.Blob != null)
            .Where(s => s.Blob.Color == socket.Blob.Color)
            .Where(s => !visited.Contains(s));
        //Debug.Log("toCheck: " + toCheck.Count());

        var result = new[] { socket }.Concat(toCheck.SelectMany(nextSocket => ConnectedSockets(nextSocket, visited)));
        return result.ToList();
    } 

    private bool PositionInGrid(Position p) => p.X >= 0 && p.Y >= 0 && p.Z >= 0 && p.X < width && p.Y < height && p.Z < width;
    public Socket SocketAt(Position p) {
        if (PositionInGrid(p)) {
            return _grid[p.X,p.Y,p.Z];
        } else {
            return null;
        }
    }
    public IEnumerable<Socket> All() {
        return _grid.Cast<Socket>();
    }
    
    public List<Socket> NeighboursFor(Socket socket) {
        return directions.Select(direction => SocketAt(socket.GridPosition + direction)).Where(neighbour => neighbour != null).ToList();
    }
   List<Position> directions = new List<Position> {
        new Position(1, 0, 0),
        new Position(-1, 0, 0),
        new Position(0, 1, 0),
        new Position(0, -1, 0),
        new Position(0, 0, 1),
        new Position(0, 0, -1),
    };

    
    public class Position {
        // Position NOT_IN_GRID = new Position(-100,-100,-100);
        public Position(int x, int y, int z) {X = x; Y = y; Z = z;} 
        
        public int X;
        public int Z;
        public int Y;
        public static Position operator +(Position a, Position b) => new Position(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Position operator -(Position a, Position b) => new Position(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static bool operator ==(Position a, Position b) => Position.Equals(a, b);
        public static bool operator !=(Position a, Position b) => !Position.Equals(a, b);

        public bool Equals(Position other) => X == other.X && Y == other.Y && Z == other.Z;
        //public override bool // override object.Equals
        public override bool Equals(object obj)
        {
            return Position.Equals(this, obj);
        }
        public static bool Equals(Position one , Position other) {
            if (one is null && other is null) {
                return true;
            }
            if (one is null || other is null) {
                return false;
            }

            if (one.GetType() != other.GetType()) {
                return false;
            }
            return one.X == other.X 
                && one.Y == other.Y  
                && one.Z == other.Z;
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

    private void StartCoroutine(float delayInSeconds, System.Action action) {
        StartCoroutine(DelayAction(delayInSeconds, action));
    }
    private IEnumerator DelayAction(float delayInSeconds, System.Action action) {
        yield return new WaitForSeconds(delayInSeconds);
        action.Invoke();
    }

}
