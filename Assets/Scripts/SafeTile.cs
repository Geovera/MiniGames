using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeTile : Tile
{
     public override bool Occupy(ParchisPiece piece)
     {
          base.Occupy(piece);
          currentPiece.canKill = false;
          Debug.Log("CANNOT KILL");

          return true;
     }

     public override void Leave(ParchisPiece piece)
     {
          currentPiece.canKill = true;
          Debug.Log("CAN KILL");
          base.Leave(piece);
     }
}
