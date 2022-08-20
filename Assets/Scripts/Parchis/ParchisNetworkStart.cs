using GV.Core.Singletons;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.UI;

public class ParchisNetworkStart : Singleton<ParchisNetworkStart>
{
     [SerializeField]
     private Button startHostButton;

     [SerializeField]
     private Button startServerButton;

     [SerializeField]
     private Button startClientButton;

     [SerializeField]
     private TextMeshProUGUI playersInGameText;

     [SerializeField]
     private TMP_InputField inputField;

     // Panels
     [SerializeField]
     private GameObject startNetworkPanel;
     [SerializeField]
     private GameObject waitingRoomPanel;

     // Start is called before the first frame update
     public override void OnNetworkSpawn()
     {
          Cursor.visible = true;

          ParchisPlayersManager.Instance.PlayersInGameNet.OnValueChanged += (prevVal, newVal) =>
          {
               UpdatePlayerCount();
          };
     }
     private void Start()
     {
          startHostButton.onClick.AddListener(() =>
          {
               if(NetworkManager.Singleton.StartHost())
               {
                    Debug.Log("Host started...");

                    startNetworkPanel.SetActive(false);
                    waitingRoomPanel.SetActive(true);

                    PieceSelectionManager.Instance.InitPieceSelection();
               }
               else
               {
                    Debug.Log("Host failed to start...");
               }
          });
          startServerButton.onClick.AddListener(() =>
          {
               if (NetworkManager.Singleton.StartServer())
               {
                    Debug.Log("Server started...");
               }
               else
               {
                    Debug.Log("Server failed to start...");
               }
          });
          startClientButton.onClick.AddListener(() =>
          {
               string ip = inputField.text;
               Debug.Log(ip);
               var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UNetTransport;
               transport.ConnectAddress = ip;
               if (NetworkManager.Singleton.StartClient())
               {
                    Debug.Log("Client started...");
                    startNetworkPanel.SetActive(false);
                    waitingRoomPanel.SetActive(true);

                    PieceSelectionManager.Instance.InitPieceSelection();
               }
               else
               {
                    Debug.Log("Client failed to start...");
               }
          });
     }

     public void UpdatePlayerCount()
     {
          playersInGameText.text = $"Players In game: {ParchisPlayersManager.Instance.PlayersInGame}";
     }

    // Update is called once per frame
    void Update()
    {
        
    }
}
