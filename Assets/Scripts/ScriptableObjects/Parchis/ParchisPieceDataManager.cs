using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Parchis/ParchisPieceDataManager")]
public class ParchisPieceDataManager : ScriptableObject
{
     public List<ParchisPieceData> piecesData = new List<ParchisPieceData>();
}
