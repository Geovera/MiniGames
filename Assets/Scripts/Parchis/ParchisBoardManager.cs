using EasyButtons;
using GV.Core.Singletons;
using GV.Shared.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParchisBoardManager : Singleton<ParchisBoardManager>
{
     [SerializeField]
     private TileBuilder[] colorSections;

     private PlayerColor[] colors = { PlayerColor.RED, PlayerColor.BLUE, PlayerColor.GREEN, PlayerColor.YELLOW };
     [SerializeField]
     private PlayerColorManager data;
     [SerializeField]
     private ParchisPrefabData prefabData;
     // Start is called before the first frame update
     void Start()
     {
          SetInstance(this);
     }

    // Update is called once per frame
    void Update()
    {
        
    }

     [Button]
     public void Clear()
     {
          for (int i = 0; i < colorSections.Length; i++)
          {
               colorSections[i].Clear();
          }
     }

     [Button]
     public void CreateBoard()
     {
          if (colorSections.Length < 1) return;

          for(int i = 0; i < colorSections.Length; i++)
          {
               colorSections[i].Initialize(data, prefabData, colors[i]);
               colorSections[i].BuildTiles();
          }

          for (int i = 0; i < colorSections.Length; i++)
          {
               var data = colorSections[i].sectionData;
               int nextIndex = (i + 1) % colorSections.Length;
               var nextData = colorSections[nextIndex].sectionData;

               data.head.next = nextData.tail;
               nextData.tail.prev = data.head;
          }

          var firstData = colorSections[0].sectionData;
          var lastData = colorSections[colorSections.Length - 1].sectionData;
          firstData.tail.prev = lastData.head.next;
          lastData.head.next = firstData.tail;
     }

     [Button]
     public void CreatePawns(CircularList<ParchisPlayerController> players)
     {
          for (int i = 0; i < colorSections.Length; i++)
          {
               var controller = players.Current();
               colorSections[i].CreatePawns(controller, controller.selectedPieceData.piecePrefab);
               controller.startTile = colorSections[i].sectionData.start;

               players.Next();
          }
     }
}
