using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using System.Linq;

public class AI : MonoBehaviour {
    protected static System.Random random = new System.Random();
    
    protected Hand _left = new Hand();
    protected Hand _right = new Hand();
    protected BlobGrid _grid;
   
    public GameObject handPrefab;

    protected List<Hand> _hands = new List<Hand>();

    protected Rigidbody _rigidBody;

    public virtual float Speed { get; set; } =  2.2f;

    public void AssignToGrid(BlobGrid grid) {
        _grid = grid;
        // drop out 
        var grabber = GetComponent<XRGrabInteractable>();
        grabber.interactionLayerMask = Layers.OUT;
        Debug.Log("scale: " + transform.localScale);
        transform.localScale = transform.localScale * 3.0f;
        _rigidBody = GetComponent<Rigidbody>();
        
    }

    void Start() {
        
    }
    protected bool BothHandsFree() {
        return _hands.All(_ => _.IsFree());
    }
    protected Hand OtherHand(Hand hand) {
        return hand == _left ? _right : _left;
    }

    protected virtual void CalculateWhatToDo() {
        foreach (Hand hand in _hands) {
            if (hand.IsIdle()) {
                var occupied = _grid.All()
                .Where(_ => _.Blob != null)
                .Where(_ => OtherHand(hand).Take != _.GridPosition)
                .ToList();

                var free = _grid.All()
                .Where(_ => _.Blob == null)
                .Where(_ => _.GridPosition.Y < _grid.Height - 1)
                .Where(_ => OtherHand(hand).MoveTo != _.GridPosition)
                .ToList();

                if (free.Count != 0 && occupied.Count != 0) {
                    var top = occupied.Find(_ => _.GridPosition.Y == _grid.Height - 1);                
                    var take = top != null ? top : occupied[random.Next(occupied.Count)];
                    
                    var sameColorNeighbours = free.Where(_ => _grid.NeighboursFor(_).Exists(n => n.Blob != null && n.Blob.Color == take.Blob.Color)).ToList();
                    if (sameColorNeighbours.Count == 0) {
                        break;
                    }
                    
                    var to = sameColorNeighbours[random.Next(sameColorNeighbours.Count)];
                    //var to = free[random.Next(free.Count)];

                    hand.Take = take.GridPosition;
                    hand.MoveTo = to.GridPosition;
                    // Debug.Log("From :" + take + "To: " + to);
                }
            }
        }
    }
    private bool _didOnceOnStart = false; 
    void OnceOnStart() {
        if (!_didOnceOnStart) {
            _hands.Add(_left);
            _hands.Add(_right);

            _left.HandVisual = Instantiate<GameObject>(handPrefab);
            _right.HandVisual = Instantiate<GameObject>(handPrefab);

            _didOnceOnStart = true;
            _rigidBody.useGravity = false;
        }
    }
    void FixedUpdate() {
        if (_grid != null) {
            OnceOnStart();

            CalculateWhatToDo();

            //
            foreach (Hand hand in _hands) {
                if (hand.Take != null || hand.MoveTo != null) { // todo:  what if it is not moving but holing ablob for some reason...
                    var target = _grid.SocketAt(hand.Take != null ? hand.Take : hand.MoveTo).transform.position;
                    
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
                        var path = target - hand.Position; // vector beween the hand and destination
                        var step = Speed * Time.deltaTime;
                        
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
            }    

            _rigidBody.transform.RotateAround(_grid.transform.position, Vector3.up, 20 * Time.deltaTime);
            //_rigidBody.transform.position = new Vector3(_rigidBody.transform.position.x, 1.0f, _rigidBody.transform.position.z);
            var betweenHAnds = new Vector3(
                (_left.Position.x + _right.Position.x) / 2,
                1.0f,
                (_left.Position.z + _right.Position.z) / 2);
            _rigidBody.transform.LookAt(betweenHAnds);
            //_rigidBody.MovePosition(new Vector3(
            //    (_left.Position.x + _right.Position.x) / 2,
            //    1.0f,
            //    (_left.Position.z + _right.Position.z) / 2));
            //_rigidBody. MoveRotation( Quaternion.identity);

        }
    }
    
    protected class Hand {
        public Vector3 Position = new Vector3(0,0,0);
        public Position MoveTo = null;
        public Position Take = null;
        public Blob Blob;

        public GameObject HandVisual;


        List<(Position, System.Action)> actions = new List<(Position, System.Action)>();
        public void ReleseBlob() {
            Blob.Rigidbody.velocity = Vector3.zero;
            MoveTo = null;
            Blob.interactionLayerMask = Layers.KEEP;
            Blob = null;
        }
        public void PickUpBlob(Socket socket) {
            var blob = socket.Blob;
            if (blob != null) {
                
                Blob = blob;
                blob.interactionLayerMask = Layers.OUT;
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
}
    
