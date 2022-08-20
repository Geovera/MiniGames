using GV.Core.Singletons;
using GV.Shared.Collections;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static ParchisPlayerController;

public class ParchisGameManager : Singleton<ParchisGameManager>
{

     public enum GameState
     {
          ROLLING_DIE,
          CHOOSING_ACTION,
          PERFORMING_ACTION,
          PERFORMING_ABILITY
     }

     public event Notify OnNextTurn;

     [SerializeField]
     private int maxNumOfPlayers = 4;
     [SerializeField]
     private int maxNumOfRetries = 3;
     [SerializeField]
     private int piecesToWin = 1;

     private int currentRetries = 0;
     private uint currentRoll = 0;
     private ParchisPiece selectedPiece;
     private GameState state = GameState.ROLLING_DIE;

     [SerializeField]
     private GameObject waitingRoomPanel, pieceSelectionPanel, gameHUDPanel;

     private Dictionary<ulong, ParchisPlayerController> idToController = new Dictionary<ulong, ParchisPlayerController>();

     CircularList<ParchisPlayerController> activePlayers = new CircularList<ParchisPlayerController>();

     [SerializeField]
     private GameHUD hud;

     private ParchisPlayerController currentTurnPlayer;

     public void Awake()
     {
          SetInstance(this);
     }

     public override void OnNetworkSpawn()
     {
          if (IsServer)
          {
               ParchisPlayersManager.Instance.OnPlayerCountChanged += CheckAmountOfPlayers;
               StartCoroutine(CheckWithDelay());
          }

          ParchisBoardManager.Instance.CreateBoard();
     }

     IEnumerator CheckWithDelay()
     {
          yield return new WaitForSeconds(.2f);
          CheckAmountOfPlayers(0);
     }

     public override void OnNetworkDespawn()
     {
          if (IsServer)
          {
               ParchisPlayersManager.Instance.OnPlayerCountChanged -= CheckAmountOfPlayers;
          }
     }

     public void CheckAmountOfPlayers(ulong id)
     {
          if(!idToController.ContainsKey(id))
          {
               var po = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(id);

               var controller = po.GetComponent<ParchisPlayerController>();
               idToController[id] = controller;
          }
          else
          {
               idToController.Remove(id);
          }
          Debug.Log(ParchisPlayersManager.Instance.PlayersInGame);
          if(ParchisPlayersManager.Instance.PlayersInGame >= maxNumOfPlayers)
          {
               MovePlayersToLobby();
          }
     }

     void MovePlayersToLobby()
     {
          MovePlayersToLobbyClientRpc();
     }

     [ClientRpc]
     void MovePlayersToLobbyClientRpc()
     {
          waitingRoomPanel.SetActive(false);
          pieceSelectionPanel.SetActive(true);
     }

     [ClientRpc]
     void StartHUDClientRpc()
     {
          pieceSelectionPanel.SetActive(false);
          gameHUDPanel.SetActive(true);
     }

     public void TryStartGame()
     {
          GameStartServerRpc();
     }

     [ServerRpc(RequireOwnership = false)]
     void GameStartServerRpc()
     {
          int selectedCount = PieceSelectionManager.Instance.PlayerSelectedCount();
          if (selectedCount == maxNumOfPlayers)
          {
               Debug.Log("Starting game...");

               PieceSelectionManager.Instance.hasStarted = true;
               var tiles = PieceSelectionManager.Instance.PieceTiles;

               foreach(var tile in tiles)
               {
                    ulong userID = tile.SelectedUser;
                    if(idToController.ContainsKey(userID) == false) { continue; }

                    ParchisPlayerController controller = idToController[userID];
                    controller.playerId = userID;
                    controller.selectedPieceDataIndex.Value = tile.tileIndex;
                    controller.selectedPieceData = tile.PieceData;

                    activePlayers.Add(controller);
               }

               ParchisBoardManager.Instance.CreatePawns(activePlayers);
               foreach(var controller in activePlayers)
               {
                    controller.Init();
               }
               StartHUDClientRpc();
               currentTurnPlayer = activePlayers.Current();
               StartTurn();


          }
          else
          {
               Debug.Log($"Not all players have selected a piece. Count: {selectedCount}");
          }
     }

     public ParchisPlayerController GetControllerFromID(ulong userID)
     {
          return idToController[userID];
     }

     [ServerRpc(RequireOwnership = false)]
     public void StopDieServerRpc()
     {
          currentRoll = DiceScript.Instance.StopRoll();
          bool canDoAction = currentTurnPlayer.CanPerformActions(currentRoll);

          if (false == canDoAction)
          {
               hud.ChangeRetriesNumClientRpc(++currentRetries);
               if (currentRetries < maxNumOfRetries)
               {
                    StartTurn();
               }
               else
               {
                    NextTurn();
               }
          }
          else
          {
               SetState(GameState.CHOOSING_ACTION);
          }
     }

     public void StartTurn()
     {
          SetState(GameState.ROLLING_DIE);
          hud.ChangeTurnClientRpc(currentTurnPlayer.playerId, currentTurnPlayer.selectedPieceData.pieceName);
          DiceScript.Instance.StartRollClientRpc();
     }

     public void NextTurn()
     {
          Debug.Log("Next Turn");
          selectedPiece = null;
          currentTurnPlayer.Reset();
          currentTurnPlayer = activePlayers.Next();
          hud.ChangeRetriesNumClientRpc(currentRetries = 0);
          OnNextTurn?.Invoke(currentTurnPlayer.playerId);
          StartTurn();
     }

     [ServerRpc]
     public void NextTurnOverrideServerRpc()
     {
          NextTurn();
     }

     [ServerRpc(RequireOwnership = false)]
     public void TrySelectPieceServerRpc(ulong playerId, NetworkBehaviourReference pieceToSelect)
     {
          if (pieceToSelect.TryGet(out ParchisPiece piece))
          {
               if (playerId == currentTurnPlayer.playerId)
               {
                    switch (state)
                    {
                         case GameState.CHOOSING_ACTION:
                              var performableActions = currentTurnPlayer.CanPiecePerformAction(piece, currentRoll, true, ref selectedPiece);
                              hud.HideButtonsClientRpc();
                              if (performableActions != PerformableActions.None)
                              {
                                   hud.ShowActionButtonClientRpc(playerId, performableActions);
                              }
                              break;
                         case GameState.PERFORMING_ABILITY:
                              if (currentTurnPlayer.IsValidAbilityTarget(piece))
                              {
                                   currentTurnPlayer.PerformPieceAbility(piece, state);
                              }
                              break;
                         default:
                              // Do nothing
                              break;
                    }

               }
          }
     }

     [ServerRpc(RequireOwnership = false)]
     public void PerformPieceActionServerRpc(ulong playerId)
     {
          if(playerId == currentTurnPlayer.playerId)
          {
               SetState(GameState.PERFORMING_ACTION);
               currentTurnPlayer.PerformPieceAction(selectedPiece, currentRoll);
          }
     }

     [ServerRpc(RequireOwnership = false)]
     public void PerformPieceAbilityServerRpc(ulong playerId)
     {
          if (playerId == currentTurnPlayer.playerId)
          {
               currentTurnPlayer.PerformPieceAbility(selectedPiece, state);
               SetState(GameState.PERFORMING_ABILITY);
          }
     }

     [ServerRpc(RequireOwnership = false)]
     public void ActionDoneServerRpc()
     {
          hud.HideButtonsClientRpc();
          if (currentRoll == 6)
          {
               Debug.Log("Another Turn");
               currentTurnPlayer.Reset();
               currentRetries = maxNumOfRetries;
               selectedPiece = null;
               StartTurn();
          }
          else
          {
               NextTurn();
          }
     }

     public void PieceReachedHome()
     {
          int piecesOnHome = currentTurnPlayer.AddPieceToHome();
          if(piecesOnHome == piecesToWin)
          {
               TriggerWin();
          }
     }

     void TriggerWin()
     {
          hud.ShowEndOfGameClientRpc(currentTurnPlayer.selectedPieceData.pieceName);
          currentTurnPlayer.PlayVictorySoundClientRpc();
     }

     void SetState(GameState newState)
     {
          state = newState;
          hud.ChangeStateTextClientRpc(state);
     }
}
