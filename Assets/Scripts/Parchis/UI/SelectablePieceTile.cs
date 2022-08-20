using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SelectablePieceTile : MonoBehaviour
{

     [SerializeField]
     private RawImage pieceImage;
     [SerializeField]
     private TextMeshProUGUI pieceName;
     [SerializeField]
     private GameObject tileTaken;

     private ParchisPieceData pieceData;
     public int tileIndex;

     // Network Data
     private ulong selectedUser = 256;
     public bool isTaken;

     public ulong SelectedUser { get { return selectedUser; } }
     public ParchisPieceData PieceData { get { return pieceData; } }

     public void SetPieceData(ParchisPieceData data, int index)
     {
          pieceData = data;
          tileIndex = index;

          pieceName.text = pieceData.pieceName;
          pieceImage.texture = pieceData.pieceTexture;
     }
     public void SeverUserSelect(ulong userID)
     {
          selectedUser = userID;
          isTaken = true;
     }

     public void ServerSelectClear()
     {
          selectedUser = 10;
          isTaken = false;
     }

     public void ToggleTakenTile(Color color, bool activate)
     {
          tileTaken.GetComponent<Image>().color = color;
          tileTaken.SetActive(activate);
     }

     public void TrySelectTile()
     {
         PieceSelectionManager.Instance.UserSelectPieceServerRpc(NetworkManager.Singleton.LocalClientId, tileIndex);
     }

     public void OnTileHoverEnter()
     {
          var ability = pieceData.piecePrefab.GetComponent<ParchisAbilityBase>();
          PieceSelectionManager.Instance.SetAbilityInfo(ability.abilityName, ability.GetDesc());
     }

     public void OnTileHoverExit()
     {
          PieceSelectionManager.Instance.RemoveAbilityInfo();
     }
}
