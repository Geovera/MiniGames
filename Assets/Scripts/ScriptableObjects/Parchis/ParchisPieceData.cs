using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Parchis/PrefabPawnData")]
public class ParchisPieceData : ScriptableObject
{
     public string pieceName;
     public Texture2D pieceTexture;
     public GameObject piecePrefab;
     public AudioClip deathClip, victoryClip;
}
