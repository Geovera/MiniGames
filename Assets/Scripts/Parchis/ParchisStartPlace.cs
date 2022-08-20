using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ParchisStartPlace : MonoBehaviour
{
     [SerializeField]
     Transform[] spawnPoints;

     private List<GameObject> pieces = new List<GameObject>();

     public void Init(ParchisPlayerController controller, GameObject piecePrefab, PlayerColor color)
     {
          for(int i = 0; i < spawnPoints.Length; i++)
          {
               var go = Instantiate(piecePrefab);
               go.transform.position = spawnPoints[i].position;

               pieces.Add(go);
               var piece = go.GetComponent<ParchisPiece>();
               piece.color = color;
               piece.PieceID = i;
               piece.controller = controller;
               piece.PlayerID = controller.playerId;
               piece.startPosition = spawnPoints[i].position;
               controller.pieces.Add(piece);

               go.GetComponent<NetworkObject>().Spawn();
          }
     }

     public void Clear()
     {
          if (pieces == null) return;
          foreach(var go in pieces)
          {
               DestroyImmediate(go);
          }
     }
}
