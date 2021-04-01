using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using System.Linq;

public class AI : MonoBehaviour {
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