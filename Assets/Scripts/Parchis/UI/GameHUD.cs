using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static ParchisPlayerController;

public class GameHUD : NetworkBehaviour
{
     const string TURN_TEXT_START = "Turn: ";
     const string END_GAME_END = " Wins!";
     const string DIE_ROLL_START = "Die Roll: ";
     const string STATE_START = "State: ";
     const string RETRIES_START = "Retries: ";

     [SerializeField]
     private TextMeshProUGUI turnText, endGameText, dieRollText, stateText, retriesText;

     [SerializeField]
     private Button stopDieButton, doActionButton, exitButton, nextTurnOverrideButton, abilityButton;

     [SerializeField]
     private GameObject networkPanel, endGamePanel, adminPanel;

     // Start is called before the first frame update
     void Start()
     {
          stopDieButton.onClick.AddListener(() =>
          {
               if (true == stopDieButton.enabled)
               {
                    stopDieButton.gameObject.SetActive(stopDieButton.enabled = false);
                    ParchisGameManager.Instance.StopDieServerRpc();
               }
          });

          doActionButton.onClick.AddListener(() =>
          {
               if (true == doActionButton.enabled)
               {
                    doActionButton.gameObject.SetActive(doActionButton.enabled = false);
                    abilityButton.gameObject.SetActive(abilityButton.enabled = false);
                    ParchisGameManager.Instance.PerformPieceActionServerRpc(NetworkManager.LocalClientId);
               }
          });

          exitButton.onClick.AddListener(() =>
          {
               Application.Quit();
          });

          nextTurnOverrideButton.onClick.AddListener(() =>
          {
               ParchisGameManager.Instance.NextTurnOverrideServerRpc();
          });

          abilityButton.onClick.AddListener(() =>
          {
               if (true == abilityButton.enabled)
               {
                    doActionButton.gameObject.SetActive(doActionButton.enabled = false);
                    abilityButton.gameObject.SetActive(abilityButton.enabled = false);
                    ParchisGameManager.Instance.PerformPieceAbilityServerRpc(NetworkManager.LocalClientId);
               }
          });
     }

     private void Update()
     {
          if(Input.GetKeyDown(KeyCode.A) && NetworkManager.Singleton.IsHost)
          {
               adminPanel.SetActive(!adminPanel.activeSelf);
          }
     }

     [ClientRpc]
    public void ChangeTurnClientRpc(ulong playerId, string name)
    {
          turnText.text = TURN_TEXT_START + name;

          stopDieButton.gameObject.SetActive(stopDieButton.enabled = NetworkManager.LocalClientId == playerId);
    }

     [ClientRpc]
     public void ShowActionButtonClientRpc(ulong playerId, PerformableActions performableActions)
     {
          if (FlagsHelper.IsSet(performableActions, PerformableActions.Move) || FlagsHelper.IsSet(performableActions, PerformableActions.Place))
          {
               doActionButton.gameObject.SetActive(doActionButton.enabled = NetworkManager.LocalClientId == playerId);
          }
          if(FlagsHelper.IsSet(performableActions, PerformableActions.Ability))
          {
               abilityButton.gameObject.SetActive(abilityButton.enabled = NetworkManager.LocalClientId == playerId);
          }
     }

     [ClientRpc]
     public void HideButtonsClientRpc()
     {
          doActionButton.gameObject.SetActive(false);
          abilityButton.gameObject.SetActive(false);
     }

     [ClientRpc]
     public void ShowEndOfGameClientRpc(string name)
     {
          networkPanel.SetActive(false);
          endGamePanel.SetActive(true);
          endGameText.text = name + END_GAME_END;
     }

     public void ChangeRollDie(uint roll)
     {
          dieRollText.text = DIE_ROLL_START + roll;
     }

     [ClientRpc]
     public void ChangeStateTextClientRpc(ParchisGameManager.GameState gameState)
     {
          stateText.text = STATE_START + gameState;
     }

     [ClientRpc]
     public void ChangeRetriesNumClientRpc (int retries)
     {
          retriesText.text = RETRIES_START + retries;
     }
}