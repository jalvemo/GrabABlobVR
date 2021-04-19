using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AITant : AI
{
    public AITant() {
        Speed = 0.5f;
    }

    //protected override float Speed { get; set; } =  0.5f;

    protected override void CalculateWhatToDo() {
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
}
