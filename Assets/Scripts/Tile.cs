using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

     public Tile prev;
     public Tile next;
     public ParchisPiece currentPiece;
     public ParchisPiece secondaryPiece;
     public bool isColored = false;

     public Transform tileCenter;
    // Start is called before the first frame update
    void Start()
    {
    }

     public void ChangeMaterial(Material mat)
     {
          Renderer mRenderer = GetComponent<Renderer>();
          mRenderer.material = mat;
     }

     public bool isOccupied()
     {
          return currentPiece != null;
     }

     public virtual bool Occupy(ParchisPiece piece)
     {
          if (currentPiece != null )
          {
               if (piece.isSameColor(currentPiece) == false && currentPiece.canKill == true)
               {
                    currentPiece.Reset();
                    currentPiece.controller.PlayDeathSoundClientRpc();
                    currentPiece = piece;
                    currentPiece.MoveTo(tileCenter.position);
               }
               else
               {
                    secondaryPiece = piece;
               }
          }
          else
          {
               currentPiece = piece;
          }

          if(true == isColored && false == piece.inEnd)
          {
               piece.inEnd = true;
               ParchisGameManager.Instance.PieceReachedHome();
          }

          return true;
     }

     public virtual void KillFirst()
     {
          currentPiece.Reset();
          currentPiece.controller.PlayDeathSoundClientRpc();

          if (secondaryPiece == null)
          {
               currentPiece = null;
          }
          else
          {
               currentPiece = secondaryPiece;
               currentPiece.MoveTo(tileCenter.position);
               secondaryPiece = null;
          }
     }

     public virtual void Leave(ParchisPiece piece)
     {
          if(piece == secondaryPiece)
          {
               secondaryPiece = null;
          }
          else if(secondaryPiece != null)
          {
               currentPiece = secondaryPiece;
               secondaryPiece = null;
               currentPiece.MoveTo(tileCenter.position);
          }
          else
          {
               currentPiece = null;
          }
     }

     public bool CanOccupy(ParchisPiece piece)
     {
          if (currentPiece == null) return true;
          if (false == isColored && secondaryPiece == null && piece.isSameColor(currentPiece) == true) return true;
          if (piece.isSameColor(currentPiece) == false && currentPiece.canKill == true) return true;

          return false;

     }

     public virtual Tile Next(ParchisPiece pieceToMove)
     {
          return next;
     }

     public Vector3 GetPlacePosition()
     {
          if(currentPiece == null)
          {
               return tileCenter.position;
          }
          return tileCenter.position + (tileCenter.right * .5f);
     }

     public bool CanTransverse()
     {
          return secondaryPiece == null;
     }
}
