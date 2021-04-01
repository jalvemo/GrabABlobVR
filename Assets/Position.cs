using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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