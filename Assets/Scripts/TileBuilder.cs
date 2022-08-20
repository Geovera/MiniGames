using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using UnityEngine;
using System.Linq;
using Unity.Netcode;

public enum PlayerColor
{
     RED,
     BLUE,
     GREEN,
     YELLOW
}

[RequireComponent(typeof(NetworkObject))]
public class TileBuilder : NetworkBehaviour
{

     enum TileType
     {
          DEFAULT,
          SAFE,
          COLOR,
          START,
          HOME
     }

     TileType[,] tileSequence =
     {
          { TileType.DEFAULT, TileType.DEFAULT,   TileType.SAFE,      TileType.DEFAULT,   TileType.DEFAULT    },
          { TileType.COLOR,   TileType.COLOR,     TileType.COLOR,     TileType.COLOR,     TileType.HOME       },
          { TileType.DEFAULT, TileType.DEFAULT,   TileType.SAFE,      TileType.DEFAULT,   TileType.START      }
     };

     public struct SectionTileData
     {
          public Tile tail;
          public Tile head;
          public Tile start;
          public Tile toHome;
          public PlayerColor sectionColor;
     }


     private PlayerColorManager colorManager;
     private ParchisPrefabData prefabData;
     private PlayerColor sectionColor;

     private ParchisStartPlace startPlace;
     public SectionTileData sectionData;

     [SerializeField]
     private GameObject[] piecePrefabs;

     // Start is called before the first frame update
     void Start()
     {
        
     }

     // Update is called once per frame
     void Update()
     {
        
     }

     [Button]
     public void Clear()
     {
          int childs = transform.childCount;

          startPlace?.Clear();
          for (int i = childs - 1; i >= 0; i--)
          {
               GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
          }
     }

     public void Initialize(PlayerColorManager colorManager, ParchisPrefabData prefabData, PlayerColor sectionColor)
     {
          this.colorManager = colorManager;
          this.prefabData = prefabData;
          this.sectionColor = sectionColor;
     }

     [Button]
     public void BuildTiles()
     {
          float initialX = -1.5f, initialZ = 2.5f;
          int offsetX = 1, offsetZ = 1;
          Vector3 positionOffset = Vector3.zero;
          positionOffset.x = initialX; positionOffset.z = initialZ;

          ColorToMatData matsData = colorManager.GetColorData(sectionColor);

          GameObject go = Instantiate(prefabData.arrowHeadPrefab, transform);
          go.GetComponent<Renderer>().material = matsData.homeColorMat;

          Tile prevTile = null;
          Tile curTile = null;

          // First row
          Tile tail = prevTile = CreateTile(TileType.DEFAULT, prevTile, ref positionOffset, offsetZ, 1, matsData.baseColorMat, prefabData);
          CreateRow(CustomArray<TileType>.GetRow(tileSequence, 0).ToList(), matsData, ref positionOffset, offsetZ, offsetX, 1, ref curTile, ref prevTile);

          // Second row
          positionOffset.z -= offsetZ;
          Tile toHomeTile = CreateTile(TileType.HOME, prevTile, ref positionOffset, offsetZ, -1, matsData.baseColorMat, prefabData);
          prevTile.next = toHomeTile;
          prevTile = toHomeTile;
          var middleTiles = CustomArray<TileType>.GetRow(tileSequence, 1).Reverse().ToList();
          CreateRow(middleTiles, matsData, ref positionOffset, offsetZ, offsetX, -1, ref curTile, ref prevTile);

          // Third row
          positionOffset.z += offsetZ * (middleTiles.Count);
          Tile startTile =  CreateTile(TileType.START, prevTile, ref positionOffset, offsetZ, -1, matsData.baseColorMat, prefabData);
          startTile.prev = toHomeTile;
          toHomeTile.next = startTile;
          prevTile = startTile;
          var leftTiles = CustomArray<TileType>.GetRow(tileSequence, 2).Reverse().ToList();

          CreateRow(leftTiles, matsData, ref positionOffset, offsetZ, offsetX, -1, ref curTile, ref prevTile);

          // Leftover
          positionOffset.z = initialZ;
          curTile = CreateTile(TileType.DEFAULT, prevTile, ref positionOffset, offsetZ, -1, null, prefabData);
          prevTile.next = curTile;

          // Set data
          sectionData.tail = tail;
          sectionData.head = curTile;
          sectionData.toHome = toHomeTile;
          sectionData.start = startTile;
          sectionData.sectionColor = sectionColor;

          CreateStartPlace(matsData);
     }

     Tile CreateRow(List<TileType> row, ColorToMatData matsData, ref Vector3 positionOffset, float offsetZ, float offsetX, int direction, ref Tile curTile, ref Tile prevTile)
     {
          for (int i = 1; i < row.Count; i++)
          {
               curTile = CreateTile(row[i], prevTile, ref positionOffset, offsetZ, direction, matsData.baseColorMat, prefabData);

               if (prevTile == null)
               {
                    prevTile = curTile;
                    continue;
               }
               prevTile.next = curTile;
               prevTile = curTile;
          }
          positionOffset.x += offsetX;
          return curTile;
     }

     Tile CreateTile(TileType tileType, Tile prevTile, ref Vector3 position, float offsetZ, int direction, Material colorMat, ParchisPrefabData prefabData)
     {
          Tile curTile = null;
          switch (tileType)
          {
               case TileType.START:
                    curTile = Instantiate(prefabData.startTilePrefab, transform).GetComponent<Tile>();
                    curTile.ChangeMaterial(colorMat);
                    break;
               case TileType.HOME:
                    curTile = Instantiate(prefabData.toHomeTilePrefab, transform).GetComponent<Tile>();
                    break;
               case TileType.SAFE:
                    curTile = Instantiate(prefabData.safeTilePrefab, transform).GetComponent<Tile>();
                    break;
               case TileType.COLOR:
                    curTile = Instantiate(prefabData.tilePrefab, transform).GetComponent<Tile>();
                    curTile.isColored = true;
                    curTile.ChangeMaterial(colorMat);
                    break;
               case TileType.DEFAULT:
               default:
                    curTile = Instantiate(prefabData.tilePrefab, transform).GetComponent<Tile>();
                    break;
          }

          if(prevTile is ToHomeTile toHomeTile)
          {
               toHomeTile.toHome = curTile;
               toHomeTile.playerColor = sectionColor;
          }

          curTile.prev = prevTile;
          curTile.transform.localPosition = position;
          position.z += offsetZ * direction;
          return curTile;
     }

     public void CreatePawns(ParchisPlayerController controller, GameObject piecePrefab)
     {
          startPlace.Init(controller, piecePrefab, sectionColor);
     }

     void CreateStartPlace(ColorToMatData matsData)
     {
          GameObject gb = Instantiate(prefabData.startPlacePrefab, transform);
          gb.GetComponent<Renderer>().material = matsData.startBaseColorMat;
          Vector3 asd = Vector3.zero;
          asd.x = 6.5f;
          asd.z = 6.5f;
          gb.transform.localPosition = asd;
          startPlace = gb.GetComponent<ParchisStartPlace>();
     }
}

public class CustomArray<T>
{
     public static T[] GetColumn(T[,] matrix, int columnNumber)
     {
          return Enumerable.Range(0, matrix.GetLength(0))
                  .Select(x => matrix[x, columnNumber])
                  .ToArray();
     }

     public static T[] GetRow(T[,] matrix, int rowNumber)
     {
          return Enumerable.Range(0, matrix.GetLength(1))
                  .Select(x => matrix[rowNumber, x])
                  .ToArray();
     }
}