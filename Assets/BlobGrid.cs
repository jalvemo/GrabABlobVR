using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

using System.Linq;
using MLAPI;



/*

sounds: 
https://mixkit.co/free-sound-effects/game/?page=2


music inspo:
https://www.epidemicsound.com/track/jsaRbhn32x/
https://www.epidemicsound.com/track/VDlqEXf2tP/
https://www.epidemicsound.com/track/lAhCbEuPMK/
https://www.epidemicsound.com/track/OGShqkVgkj/
https://www.epidemicsound.com/track/PpCfiCdTi1/
https://www.youtube.com/watch?v=7qqmRN198JU&ab_channel=AllNintendoMusic
https://youtu.be/PvSfqBJi0ss?t=3164
https://open.spotify.com/track/4AxrlOlmlKducWdz18bLYB?si=LPc0C1jMTGWTdHWptJ-1JA


* characta inspo:
    * https://joslin.artstation.com/projects/KgdLX

To-do list:



bugs
* when blobs fall fast they fall out of teir sockets., i removed interpolate solves it but that is choppy....look at collition detection mode?
* when falling blob miss the socket the socket is lost waiting for falling blob. 
* all blobs dont go in to a combo if one blabb is falling from high. connecting blobs disapear before far falling blob connects. 

small thing:
* let the music decide when to drop (on 8), let dificulity dcide how manny. if 1-3 drop first it would counitue fill from 4 on next drop

* if you holdon to a blob, or released it were something is supposed to fall to, make the falling blob be cached on the above slot.

bigger things
* loosing a ball.
    * penelty when loosing a ball. big black block. lives / cieling (fall out) life counter
* penelty when loosing a ball. big black block. lives / cieling (fall out) life counter 
    * penelty when loosing a ball. big black block. lives / cieling (fall out) life counter
    * put it in fall grid, regular layer or above..
* pre game menu, it would be cool if you dont have pre-menu and spawn diectly in the world to fool around. you can configure and join games from tthe world. 
* game mode 2d would still be fun i think 
* merge connecting blobs visually
* powerups in blobs, blobs pop and you get power up. 
* Score should take level/fall-rate in to account. It is a lot easier to do combos in the begining. 

improve working version:
* background sound
* sound efects 
* in game menu (pip boy?)
* environments


big things
* multiplayer
* AI 

ideas
Cooperative work on the same stack/grid 2 vs 0 2 vs 2 
different claws for different characters

bals get up like the marble macheene x. maybe also generates music?

Mariokart boxes, you get random boxes of "things you can do" when executing combos. but can only keep 1, 2, 3 specials. 
catch up, player behind might get better items like in marikart. (who is behind i dont know..)..
box power could be
* send different kind of blocks to the other player, like pyo pyo stones
* spawn at the other players stack to change there blobs for a short time, dropping blobs end powerup.
* throw things at the other player,(dont know what)
* fog for the other players
* pixel camera for the other players hope it is dizzy proof. like this: https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/pixelation-65554?aid=1100l355n&gclid=Cj0KCQjwl9GCBhDvARIsAFunhsnxGxkAdjtTEYirZv-vhIGEsPDZ93_kD2XYbR5LK5CI16obsGbkI6kaAp2vEALw_wcB&pubref=UnityAssets%2ADynNew08%2A1723478829%2A67594162255%2A336277500151%2Ag%2A%2A%2Ab%2Ac%2Agclid%3DCj0KCQjwl9GCBhDvARIsAFunhsnxGxkAdjtTEYirZv-vhIGEsPDZ93_kD2XYbR5LK5CI16obsGbkI6kaAp2vEALw_wcB&utm_source=aff
* release other players fillup.
* randomise other players stack
* pause fillup
* big release of random blobs on top of stack
* randomise a slice of other players stack
* dynamite, radial dstuction of blobs.  time it with the fuse. 
* rotate other players stack keeping the blob relations but a bit confusing
* AI controllers helping you out.
* AI controllers sabotaging.
* make the other playes stack Big / hard to reach. or small hard to be precise

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
    public GameObject SocketPrefab;
    public Light lighting;
    public ScoreBoard ScoreBoard;
    public GameObject DropPoint;
    private AudioSource audioSource;
    public AudioClip pop;
    public AudioClip noFit;
    public AudioClip gameOver;
    public AudioClip fallSound;
    public AudioClip levelUpSound;

    int _width = 4;
    int _depth = 2;
    int _height = 7;
    public int Height { get { return _height; }}
    int startHeight = 5;
    float distance = 0.3f;//0.275f; // distance between blob sockets
    
    public SocketSelector removeThresholdSelector;
    public SocketSelector dropDelay;
    public SocketSelector nextWidth;
    public SocketSelector nextHeight;
    public SocketSelector nextStartHeight;

    private Socket[,,] _grid;
    private Socket[,,] _fillerGrid;
    
    private int fillerGridYPositionRelative = 0;
    private float levelSpeedChange = 2.0f / 3.0f;
    // level  0-10 : 2.0 1.3, .88 .59 .39 .26 .17 .11 .07 .05 .034
    private float levelUpWaitSeconds = 20;

    private bool _started = false;

    public bool Started { get{return _started; } }

    ////// multiplayer properies START //////////////
    public NetworkSelection network = null;
    ////// multiplayer properies END //////////////
    private void LevelUp() {
        if (!_started) {return;}
        Invoke("LevelUp", levelUpWaitSeconds);
        dropDelay.curentValue = dropDelay.curentValue * levelSpeedChange;
        audioSource.PlayOneShot(levelUpSound);
        Debug.Log("Level up speed: " + dropDelay.curentValue);
        ScoreBoard.LevelUp();
    }

    public void Reset() {
        Debug.Log("Reset");

        Stop();
        PrepareStart();
    }
    public void Stop() {
        Debug.Log("stop");

        CancelInvoke("LevelUp");
        CancelInvoke("FillFiller");
        foreach (var socket in _grid.Cast<Socket>().Concat(_fillerGrid.Cast<Socket>())) {
            socket.Blob?.SetGrabLayer(Layers.OUT);
        }
        _started = false;
    }

    public void PrepareStart() {
        Debug.Log("PrepareStart");

        audioSource = GetComponent<AudioSource>();
        sequencialDropFailCount = 0;
        ScoreBoard.ResetBoard();

        //Debug.Log("Init grid. h: " + _height + " w: " + _width + " d: " + _depth);
        _grid = _grid == null ? new Socket[_width, _height, _depth] :_grid;
        for(int x = 0; x < _width; x++) {
            for(int y = 0; y < _height; y++) {
                for(int z = 0; z < _depth; z++) {
                    var position = new Position(x,y,z);
                    var positionVector =  GetPositionVector(position);
                    if (y < startHeight) {
                        //Debug.Log("Instantiate");
                        if (network != null) {
                            network.CreateBlobForMeServerRpc(positionVector);
                        } else {
                            Blob.Instantiate(positionVector,
                                color: Blob.Colors[(position.X + position.Y + position.Z) % (Blob.Colors.Count - 1)]);
                        }
                    }
                    if (_grid[x,y,z] == null) {
                        InitSocketAt(position, positionVector, _grid);
                    }
                }               
            }  
        }
        if (_fillerGrid == null) {
            _fillerGrid = new Socket[_width, 1, _depth];
            for(int x = 0; x < _width; x++) {
                int y = 0;
                for(int z = 0; z < _depth; z++) {
                    var position = new Position(x,y,z);
                    InitSocketAt(position, GetPositionVector(position, fillerGridYPositionRelative + _height), _fillerGrid, false);
                }                           
            }
        }
        

        nextWidth.onChanged = () => {
            _width = nextWidth.GetInt();
            Reset();
        };

        nextHeight.onChanged = () => {
            _height = nextHeight.GetInt();
            Reset();
        };

        nextStartHeight.onChanged = () => {
            startHeight = nextStartHeight.GetInt();
            Reset();
        };
        
        dropDelay.onChanged = () => {
            CancelInvoke("FillFiller");
            if (_started) {
                Invoke("FillFiller", dropDelay.curentValue);
            }
        };
    }
   public void StartGame() {
       if(!_started) {
            _started = true;

            Invoke("FillFiller", dropDelay.curentValue);
            Invoke("LevelUp", levelUpWaitSeconds);
       }
    }

    void Start()
    {   
        PrepareStart(); 
    }

    void FixedUpdate() {
        
    }
    void Update()
    {
        UpdateRedLight();
    }
    private void UpdateRedLight() {
        if (sequencialDropFailCount != 0) {
                lighting.intensity = Beats.GetPulseTriangle() * 20;
        } else {
                lighting.intensity = 0;
        } 
    }

    private float _fillPauseDelay = 0.0f;      
    private int gameOverDropFailThreshhold = 2;
    private int sequencialDropFailCount = 0;

    void FillFiller() {
        if (!_started) {return;}
        //Debug.Log("FillFiller");
        // Wait while dropping
        if (_fillPauseDelay > 0.0f) {
            Invoke("FillFiller", _fillPauseDelay);
            _fillPauseDelay = 0.0f;
            return;
        }    
        ScoreBoard.ApplyScore();
        // Fill
        for(int x = 0; x < _width; x++) {
            int y = 0;
            for(int z = 0; z < _depth; z++) {
                if (_fillerGrid[x,y,z].Blob == null) {
                    var position = new Position(x,y,z);
                    var vector = GetPositionVector(position, fillerGridYPositionRelative + _height); 
                    if (network != null) {
                        //Debug.Log("new blob : position : " + position);
                        network.CreateBlobForMeServerRpc(vector);
                    } else {
                        Blob.Instantiate(vector); 
                    }
                    Invoke("FillFiller", dropDelay.curentValue);
                    return;
                }
            }                           
        }
        // Fall
        bool failed = false;
        for(int x = 0; x < _width; x++) {
            int y = 0;
            for(int z = 0; z < _depth; z++) {
                var lowestFree = FindLowestFreeSocketOnTop(x, z); 
                _fillerGrid[x,y,z].Blob.SetGrabLayer(Layers.FALL);             
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
                Debug.Log("Game over");
                Stop();
            } else {
                audioSource.PlayOneShot(noFit);
            }
        } else {
            sequencialDropFailCount = 0;
            audioSource.PlayOneShot(fallSound, 1.0f);

        }

        // repeat
        Invoke("FillFiller", dropDelay.curentValue);
    }

    private void InitSocketAt(Position position, Vector3 vector, Socket[,,] grid, bool connectionListener = true)
    {   
        Socket socket = Instantiate(SocketPrefab, vector, new Quaternion()).GetComponent<Socket>();
        socket.name = "socket " + position.ToString();       
        socket.GridPosition = position;

        grid[position.X, position.Y, position.Z] = socket;

        socket.selectEntered.AddListener((_) => {
            _.interactable.GetComponent<AI>()?.AssignToGrid(this);
            var droppedBlob = _.interactable.GetComponent<Blob>();
            if (droppedBlob != null) { 
                //Debug.Log("socket got blob " + droppedBlob.Color);
                socket.Blob = droppedBlob;
                if (connectionListener) {
                    StartCoroutine(checkForConnctedSocketDrop(socket, 0.1f)); //0.1 allows some time to drop 2 connected blobs
                }
            }
        });

        socket.selectExited.AddListener((_) => { // move to socket..            
            socket.Blob = null;
            if(_.interactable.GetComponent<XRGrabInteractable>()?.interactionLayerMask == Layers.KEEP && !_started) { // interactionLayerMask so we dont start when we clear the board.... special call for AI that has another layer picing up. better solution would be nice, maybe tie blobs to a match id ???? 
            //if(!_started) {
                StartGame();
            }
        });
    }
    
    private IEnumerator checkForConnctedSocketDrop(Socket socket, float t) {
        yield return new WaitForSeconds(t);
        
        var connectedSockets = ConnectedSockets(socket);        
        if (connectedSockets.Count() >= removeThresholdSelector.GetInt()) {
            // Debug.Log("connected: " + connectedSockets.Count + ", color: " + socket.Blob.Color);
            StartCoroutine(DropOutInSeconds(connectedSockets, 0.5f));
        }
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
            //var blob = sockets[i].Blob;
            //StartCoroutine(i * t / sockets.Count,
            //    () => {
            //        blob.SetDropOutVisual();
            //        audioSource.PlayOneShot(pop);
            //    }
            //);
            sockets[i].Blob = null; // (2) todo,might be dangerous this early 
        }

        _fillPauseDelay = _fillPauseDelay + t + 0.5f; // pause filling while falling.. + 0.5f to wait for the fall a bit also. Some kind of callback when fall is complete might be better.  
    
        // score
        ScoreBoard.AddScore(blobs); 

        //wait
        yield return new WaitForSeconds(t);
        
        // drop out
        foreach (var blob in blobs) {
            //blob.Destroy();
            blob.SetGrabLayer(Layers.OUT);
            blob.MoveTowards(DropPoint.transform.position);
            //StartCoroutine(1f,() => {
            //    //blob.Rigidbody.MovePosition(new Vector3(1.58f, 2f, 2.7f));
            //});
            
        }


        // push a bit to not stuck while falling // todo maybe remove delay?
        //StartCoroutine(0.1f, () => {
        //     foreach (var blob in blobs) {
        //            blob.Rigidbody.MovePosition(blob.Rigidbody.position + new Vector3(0.05f, 0.05f, 0.05f));
        //            //rigidBody.AddForce(new Vector3(10.05f, 10.05f, 10.05f));
        //        }
        //});

        //// fall above logic 
        //(1) lowest sockets with the same x z cordinate (lowest socket dropping out will start catching falling blobs), there should be a nicer way.... 
        var socketsByXZ = new Dictionary<(int, int), Socket>();
        foreach (var s in sockets.OrderBy(s => s.GridPosition.Y).ToList())
        {            
            var key = (s.GridPosition.X, s.GridPosition.Z);
            socketsByXZ[key] = socketsByXZ.ContainsKey(key) ? socketsByXZ[key] : s;
        }
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
            fallingBlob.SetGrabLayer(Layers.FALL);
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
        nextCatchingSocket.interactionLayerMask = Layers.FALL;

        UnityAction<SelectEnterEventArgs> catchFallingBlob = null;
        catchFallingBlob = (_) => {
            var catchedBlob = _.interactable.GetComponent<Blob>();
            if (catchedBlob == null) {
                Debug.Log("Wops - Tried to catch falling blob, but was no blob: " + _);
                return;
            }

            nextCatchingSocket.selectEntered.RemoveListener(catchFallingBlob);
            // catch regular blobs 
            nextCatchingSocket.interactionLayerMask = Layers.KEEP;
            // make the catch blob not falling. 

            catchedBlob.SetGrabLayer(Layers.KEEP);
            CatchFallingBlobs(catchingSockets.Skip(1).ToList());
        };
        nextCatchingSocket.selectEntered.AddListener(catchFallingBlob);
    }

    // ------------------- Helpers -------------------

    // where new falling blobs will fall to 
    private Socket FindLowestFreeSocketOnTop(int x, int z) {
        if (_grid[x, _height - 1, z].Blob != null) { // top
            return null;
        }

        for(int i = Height - 2; i >= 0 ; i--) {
            if (_grid[x, i, z].Blob != null)
            {
                return _grid[x, i + 1, z];
            }
        }
        return _grid[x, 0, z]; // bottom
    }

    private List<Socket> GetAboveOrderedLowestFirst(Socket socket) {
        var above = new List<Socket>();
        if (socket.GridPosition.Y == _height - 1) {
            return above;
        }
        for( int i = socket.GridPosition.Y + 1; i < _height ; i++) {
            above.Add(_grid[socket.GridPosition.X, i , socket.GridPosition.Z]);
        }
        return above;
    } 

    private Vector3 GetPositionVector(Position position, int yStart = 0) {
        float dx = (_width - 1) * distance / 2;
        float dz = (_depth - 1) * distance / 2;
        return new Vector3(position.X * distance - dx, (position.Y + yStart)* distance + 0.5f, position.Z * distance - dz) + this.transform.position;
    }

    private List<Socket> ConnectedSockets(Socket socket, HashSet<Socket> visited = null) {
        if (socket.Blob == null) {
            return new List<Socket>();
        }
        
        visited = visited ?? new HashSet<Socket>();
        visited.Add(socket);
        var toCheck = NeighboursFor(socket)
            .Where(s => s.Blob != null)
            .Where(s => s.Blob.Color == socket.Blob.Color)
            .Where(s => !visited.Contains(s));
        //Debug.Log("toCheck: " + toCheck.Count());

        var result = new[] { socket }.Concat(toCheck.SelectMany(nextSocket => ConnectedSockets(nextSocket, visited)));
        return result.ToList();
    } 

    private bool PositionInGrid(Position p) => p.X >= 0 && p.Y >= 0 && p.Z >= 0 && p.X < _width && p.Y < _height && p.Z < _depth;
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

    private void StartCoroutine(float delayInSeconds, System.Action action) {
        StartCoroutine(DelayAction(delayInSeconds, action));
    }
    private IEnumerator DelayAction(float delayInSeconds, System.Action action) {
        yield return new WaitForSeconds(delayInSeconds);
        action.Invoke();
    }

}
