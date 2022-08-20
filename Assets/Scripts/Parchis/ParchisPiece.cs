using EasyButtons;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class ParchisPiece : NetworkBehaviour
{

     public const string DEFAULT_LAYER = "Default";
     public const string OUTLINE_LAYER = "Outline";
     public const string SECONDARY_OUTLINE_LAYER = "SecondaryOutline";

     public Tile curTile;
     public PlayerColor color;
     public bool hasSecondaryColor;
     public PlayerColor secondaryColor;
     public bool canKill = true;
     public bool inEnd = false;
     public Vector3 startPosition;
     public ParchisPlayerController controller;
     public NetworkVariable<int> pieceId = new NetworkVariable<int>();
     NetworkVariable<ulong> playerId = new NetworkVariable<ulong>();

     private AudioSource audioSource;
     private ParchisAbilityBase ability;

     public ulong PlayerID { get { return playerId.Value; } set { playerId.Value = value; } }
     public int PieceID { get { return pieceId.Value; } set { pieceId.Value = value; } }
     public ParchisAbilityBase Ability { get { return ability; }}

     private Coroutine moveCorouine = null;
     bool inHome = true;

     public bool InHome { get { return inHome; } set { inHome = value; } }

     private void Start()
     {
          audioSource = GetComponent<AudioSource>();
          ability = GetComponent<ParchisAbilityBase>();
     }

     public void Init()
     {
          audioSource = GetComponent<AudioSource>();
          ability = GetComponent<ParchisAbilityBase>();
          ability.Init();
     }

     [Button]
     public void MovePiece(uint steps)
     {
          bool canMovePiece = CanMove(steps);
          if(canMovePiece && moveCorouine == null)
          {
               moveCorouine = StartCoroutine(MoveToTarget(steps, 3f));
          }
     }

     public void PlacePiece(Tile startTile)
     {
          if(true == inHome)
          {
               inHome = false;
               StartCoroutine(MoveToStart(startTile));
          }
     }

     IEnumerator MoveToStart(Tile target)
     {
          Vector3 targetPos = target.GetPlacePosition();
          yield return StartCoroutine(MoveToTarget(targetPos, 3f));
          curTile = target;
          if (curTile.Occupy(this))
          {
               ParchisGameManager.Instance.ActionDoneServerRpc();
          }
     }

     public IEnumerator MoveToTarget(Vector3 targetPos, float speed)
     {
          while (transform.position != targetPos)
          {
               transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

               yield return null;
          }
     }

     public IEnumerator MoveToTarget(uint steps, float speed)
     {
          Tile target = curTile;
          target.Leave(this);
          for (int i = 0; i < steps; i++)
          {
               target = target.Next(this);
               Vector3 targetPos = target.GetPlacePosition();
               yield return StartCoroutine(MoveToTarget(targetPos, speed));
          }
          curTile = target;
          if (curTile.Occupy(this))
          {
               ParchisGameManager.Instance.ActionDoneServerRpc();
          }  
          moveCorouine = null;
     }

     public bool CanMove(uint steps)
     {
          if (true == inHome) return false;

          int stepsTaken = 0;
          Tile temp = curTile;
          while(temp.Next(this) != null && stepsTaken < steps)
          {
               stepsTaken++;
               temp = temp.Next(this);
               if(false == temp.CanTransverse())
               {
                    return false;
               }
          }

          return stepsTaken == steps && temp.CanOccupy(this);
     }

     public bool CanPlace(Tile startTile, uint dieRoll)
     {
          return inHome && dieRoll == 6 && startTile.CanOccupy(this) && startTile.CanTransverse();
     }

     [ClientRpc]
     public void SetOutlineClientRpc(int index)
     {
          switch(index)
          {
               case 0:
                    gameObject.layer = LayerMask.NameToLayer(DEFAULT_LAYER);
                    break;
               case 1:
                    gameObject.layer = LayerMask.NameToLayer(OUTLINE_LAYER);
                    break;
               case 2:
                    gameObject.layer = LayerMask.NameToLayer(SECONDARY_OUTLINE_LAYER);
                    break;
               default:
                    gameObject.layer = LayerMask.NameToLayer(DEFAULT_LAYER);
                    break;
          }

          SetOutlineToChildren(gameObject.layer);
     }

     void SetOutlineToChildren(int layer)
     {
          foreach (Transform t in gameObject.GetComponentsInChildren<Transform>(true))
          {
               t.gameObject.layer = layer;
          }
     }

     public void MoveTo(Vector3 position)
     {
          transform.position = position;
     }

     public void Reset()
     {
          curTile = null;
          inHome = true;

          transform.position = startPosition;
          ability.ResetPiece();
     }

     public void PlayOneShot(AudioClip audioClip, float volume = 1f)
     {
          audioSource.PlayOneShot(audioClip, volume);
     }

     public bool CanUseAbilitySelf()
     {
          return ability.CanUseSelf();
     }

     public void UseAbility(ParchisPiece target, ParchisGameManager.GameState state)
     {
          ability.Use(target, state);
     }

     public bool IsValidAbilityTarget(ParchisPiece target)
     {
          return ability.IsValidTarget(target);
     }

     public void SealAbility()
     {
          ability.hasUsedAbility = true;
     }

     public bool isSameColor(ParchisPiece other)
     {
          if (hasSecondaryColor == false && other.hasSecondaryColor == false)
          {
               return color == other.color;
          }
          else if(hasSecondaryColor == false && other.hasSecondaryColor == true)
          {
               return color == other.color || color == other.secondaryColor;
          }
          else if(hasSecondaryColor == true && other.hasSecondaryColor == false)
          {
               return color == other.color || secondaryColor == other.color;
          }
          else
          {
               return color == other.color || secondaryColor == other.color || color == other.secondaryColor || secondaryColor == other.secondaryColor;
          }
     }

}
