using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using GV.Shared.Collections;
using System;
using static ParchisGameManager;

[RequireComponent(typeof(NetworkObject))]
public class ParchisPlayerController : NetworkBehaviour
{
     enum PieceAction
     {
          NOTHING,
          MOVE,
          PLACE
     }

     [Flags]
     public enum PerformableActions
     {
          None = 0,
          Move = 1,
          Place = 2,
          Ability = 4
     }

     public ulong playerId;
     public ParchisPieceData selectedPieceData;
     public Tile startTile;
     public uint currentRoll;
     private PieceAction actionToPerform = PieceAction.NOTHING;
     private ParchisPiece selectedPiece;
     [SerializeField]
     private ParchisPieceDataManager pieceDataManager;

     private GameObject settingsPanel;

     public List<ParchisPiece> pieces = new List<ParchisPiece>();
     public NetworkVariable<int> selectedPieceDataIndex = new NetworkVariable<int>();

     public int piecesAtHome = 0;
     public override void OnNetworkSpawn()
     {
          selectedPieceDataIndex.OnValueChanged += (prevVal, newVal) =>
          {
               selectedPieceData = pieceDataManager.piecesData[newVal];
          };
     }
     private void Start()
     {
          settingsPanel = GameObject.FindGameObjectWithTag("Settings").transform.GetChild(0).gameObject;
     }

     public void Init()
     {
          foreach(var piece in pieces)
          {
               piece.Init();
          }
     }

     public bool CanMovePiece(uint diceRoll)
     {
          bool output = false;

          foreach (var (piece, index) in pieces.WithIndex())
          {
               if (true == piece.CanMove(diceRoll))
               {
                    output = true;
                    piece.SetOutlineClientRpc(1);
               }
          }

          return output;
     }

     public bool CanPlacePieceOnBoard(uint dieRoll)
     {
          bool output = false;

          foreach(var (piece, index) in pieces.WithIndex())
          {
               if(true == piece.CanPlace(startTile, dieRoll))
               {
                    output = true;
                    piece.SetOutlineClientRpc(1);
               }
          }

          return output;
     }

     public bool CanPerformActions(uint dieRoll)
     {
          ParchisPiece _ = null;
          bool ret = false;
          foreach(var p in pieces)
          {
               if(CanPiecePerformAction(p, dieRoll, false, ref _) != PerformableActions.None)
               {
                    p.SetOutlineClientRpc(1);
                    ret = true;
               }
          }

          return ret;
     }

     public PerformableActions CanPiecePerformAction(ParchisPiece p, uint dieRoll, bool outline, ref ParchisPiece piece)
     {
          if (p.color != pieces[0].color) return PerformableActions.None;
          currentRoll = dieRoll;
          PerformableActions performableActions = PerformableActions.None;
          var prev = piece;
          piece = p;

          if(piece.CanMove(dieRoll))
          {
               actionToPerform = PieceAction.MOVE;
               FlagsHelper.Set(ref performableActions, PerformableActions.Move);
          }
          else if(piece.CanPlace(startTile, dieRoll))
          {

               actionToPerform = PieceAction.PLACE;
               FlagsHelper.Set(ref performableActions, PerformableActions.Place);
          }

          if(piece.CanUseAbilitySelf())
          {
               FlagsHelper.Set(ref performableActions, PerformableActions.Ability);
          }

          if (outline == true && performableActions != PerformableActions.None)
          {
               if (outline == true)
               {
                    if (selectedPiece != null)
                    {
                         selectedPiece.SetOutlineClientRpc(1);
                    }
                    piece.SetOutlineClientRpc(2);

                    selectedPiece = piece;
               }
               else
               {
                    piece = prev;
               }
          }

          return performableActions;
     }

     public void RemovePiecesEffect()
     {
          foreach (var piece in pieces)
          {
               piece.SetOutlineClientRpc(0);
          }
     }

     public void Reset()
     {
          selectedPiece = null;
          actionToPerform = PieceAction.NOTHING;
          RemovePiecesEffect();
     }

     public void PerformPieceAction(ParchisPiece piece, uint rollDice)
     {
          switch(actionToPerform)
          {
               case PieceAction.MOVE:
                    piece.MovePiece(rollDice);
                    break;
               case PieceAction.PLACE:
                    piece.PlacePiece(startTile);
                    break;
               default:
                    // Do nothing
                    break;
          }
     }

     public void PerformPieceAbility(ParchisPiece piece, GameState state)
     {
          selectedPiece.UseAbility(piece, state);
     }

     public bool IsValidAbilityTarget(ParchisPiece target)
     {
          return selectedPiece.IsValidAbilityTarget(target);
     }

     private void Update()
     {
          if(Input.GetMouseButtonDown(0))
          {
               Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

               if(Physics.Raycast(ray, out RaycastHit hitInfo))
               {
                    ParchisPiece piece = hitInfo.collider.gameObject.GetComponent<ParchisPiece>();
                    if (piece != null)
                    {
                         ParchisGameManager.Instance.TrySelectPieceServerRpc(NetworkManager.LocalClientId, piece);
                    }
               }
          }

          if(Input.GetKeyDown(KeyCode.Escape))
          {
               settingsPanel.SetActive(!settingsPanel.activeSelf);
          }
     }

     public int AddPieceToHome()
     {
          return ++piecesAtHome;
     }

     public bool HasPieceOnBoard()
     {
          return pieces.Exists(piece => !piece.InHome);
     }

     [ClientRpc]
     public void PlayDeathSoundClientRpc()
     {
          pieces[0].PlayOneShot(selectedPieceData.deathClip);
     }

     [ClientRpc]
     public void PlayVictorySoundClientRpc()
     {
          pieces[0].PlayOneShot(selectedPieceData.victoryClip, .05f);
     }
}
