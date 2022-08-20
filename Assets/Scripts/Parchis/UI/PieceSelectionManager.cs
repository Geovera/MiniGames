using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using System.Linq;
using UnityEngine;
using GV.Core.Singletons;
using TMPro;

public class PieceSelectionManager : Singleton<PieceSelectionManager>
{
     private List<SelectablePieceTile> pieceTiles = new List<SelectablePieceTile>();
     [SerializeField]
     private ParchisPieceDataManager pieceDataManager;
     [SerializeField]
     private GameObject selecteablePiecePrefab;
     [SerializeField]
     private Transform contentTransform;
     [SerializeField]
     private TextMeshProUGUI abilityNameText, abilityDescriptionText;

     private Dictionary<ulong, SelectablePieceTile> userToTileIndex = new Dictionary<ulong, SelectablePieceTile>();

     public List<SelectablePieceTile> PieceTiles { get { return pieceTiles; } }
     public bool hasStarted = false;

     private int selectedPieceIndex = -1;

     private void Start()
     {
          SetInstance(this);

          abilityNameText.text = "";
          abilityDescriptionText.text = "";
     }

     private void Update()
     {
          if(false == hasStarted && Input.GetKeyDown(KeyCode.C))
          {
               UserRemoveSelectionServerRpc(NetworkManager.Singleton.LocalClientId);
          }
     }

     public void InitPieceSelection()
     {
          for (int i = 0; i < pieceDataManager.piecesData.Count; i++)
          {
               GameObject go = Instantiate(selecteablePiecePrefab, contentTransform);
               var pieceTile = go.GetComponent<SelectablePieceTile>();
               pieceTile.SetPieceData(pieceDataManager.piecesData[i], i);
               go.SetActive(true);
               pieceTiles.Add(pieceTile);
          }
     }

     [ServerRpc(RequireOwnership = false)]
     public void UserRemoveSelectionServerRpc(ulong userID)
     {
          if (userToTileIndex.ContainsKey(userID))
          {
               var pieceTile = userToTileIndex[userID];
               pieceTile.ServerSelectClear();
               ToggleTakenTileClientRpc(userID, pieceTiles.IndexOf(pieceTile), false);
               userToTileIndex.Remove(userID);
          }
     }

     [ServerRpc(RequireOwnership = false)]
     public void UserSelectPieceServerRpc(ulong userID, int pieceIndex)
     {
          var newSelectTile = pieceTiles[pieceIndex];
          if (true == newSelectTile.isTaken)
          {
               Debug.Log("Piece is already taken");
               return;
          }
          else
          {
               if(userToTileIndex.ContainsKey(userID))
               {
                    var pieceTile = userToTileIndex[userID];
                    pieceTile.ServerSelectClear();
                    ToggleTakenTileClientRpc(userID, pieceTiles.IndexOf(pieceTile), false);
                    userToTileIndex.Remove(userID);
               }

               newSelectTile.SeverUserSelect(userID);
               ToggleTakenTileClientRpc(userID, pieceIndex, true);
               userToTileIndex[userID] = newSelectTile;

               Debug.Log($"User {userID} selected {pieceIndex}");
          }
     }

     [ClientRpc]
     void ToggleTakenTileClientRpc(ulong userID, int pieceIndex, bool activate)
     {
          var tile = pieceTiles[pieceIndex];

          if (userID == NetworkManager.Singleton.LocalClientId)
          {

               if (activate == true)
               {
                    selectedPieceIndex = pieceIndex;
                    var ability = tile.PieceData.piecePrefab.GetComponent<ParchisAbilityBase>();
                    SetAbilityInfo(ability.abilityName, ability.GetDesc());
               }
               else
               {
                    selectedPieceIndex = -1;
                    RemoveAbilityInfo();
               }
          }

          tile.ToggleTakenTile(ParchisPlayersManager.Instance.UserToColor(userID), activate);
     }

     public int PlayerSelectedCount()
     {
          return userToTileIndex.Count;
     }

     public void SetAbilityInfo(string abilityName, string abilityDesc)
     {
          abilityNameText.text = $"<color=purple>{abilityName}</color>";
          abilityDescriptionText.text = abilityDesc;
     }

     public void RemoveAbilityInfo()
     {
          if(selectedPieceIndex >= 0 && selectedPieceIndex < pieceTiles.Count)
          {
               var ability = pieceTiles[selectedPieceIndex].PieceData.piecePrefab.GetComponent<ParchisAbilityBase>();
               SetAbilityInfo(ability.abilityName, ability.GetDesc());
          }
          else
          {
               SetAbilityInfo("", "");
          }
     }
}
