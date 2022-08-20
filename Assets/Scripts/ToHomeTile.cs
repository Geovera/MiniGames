using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToHomeTile : Tile
{
     public Tile toHome;
     public PlayerColor playerColor;

     public override Tile Next(ParchisPiece pieceToMove)
     {
          if(pieceToMove.color == playerColor)
          {
               return toHome;
          }
          return next;
     }
}
