using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MarinAbility : ParchisAbilityBase
{

     public uint spacesBackToCheck = 5;
     public uint maxTurnsInCosplay = 3;

     private uint curTurnsInCosplay = 0;
     private ParchisAbilityBase copiedAbility;
     private Coroutine effectCoroutine;

     private List<ParchisPiece> possibleTargets = new List<ParchisPiece>();

     public override void Init()
     {
          base.Init();
          ParchisGameManager.Instance.OnNextTurn += NewTurn;
     }

     public override bool CanUseSelf()
     {
          if (Piece.curTile == null)
          {
               return false;
          }
          if (hasUsedAbility == true && copiedAbility != null)
          {
               return copiedAbility.CanUseSelf();
          }

          possibleTargets.Clear();

          Tile t = Piece.curTile;
          for (int i = 0; i < spacesBackToCheck; i++)
          {
               t = t.prev;
               if (t.currentPiece != null && t.currentPiece.color != Piece.color)
               {
                    possibleTargets.Add(t.currentPiece);
               }
          }

          return possibleTargets.Count > 0;
     }

     public override bool CanUseToOther(ParchisPiece other)
     {
          return false;
     }

     public override bool IsValidTarget(ParchisPiece target)
     {
          if (copiedAbility != null) return copiedAbility.IsValidTarget(target);

          return possibleTargets.Exists(x => x == target);
     }

     public override bool Use(ParchisPiece target, ParchisGameManager.GameState state)
     {
          if (copiedAbility != null) return copiedAbility.Use(target, state);

          switch (state)
          {
               case ParchisGameManager.GameState.CHOOSING_ACTION:
                    OutlineTargets();
                    hasUsedAbility = true;
                    break;
               case ParchisGameManager.GameState.PERFORMING_ABILITY:
                    if (effectCoroutine == null)
                    {
                         effectCoroutine = StartCoroutine(UseCoroutine(target));
                    }
                    break;
               default:
                    // Do nothing
                    break;
          }

          return true;
     }
     private void OutlineTargets()
     {
          Piece.controller.RemovePiecesEffect();

          foreach(var target in possibleTargets)
          {
               target.SetOutlineClientRpc(1);
          }
     }

     IEnumerator UseCoroutine(ParchisPiece target)
     {
          ClearOutlines();
          hasUsedAbility = true;
          Piece.hasSecondaryColor = true;
          Piece.secondaryColor = target.color;

          effects.StartEffect(AbilityEffects.EffectStage.START, Piece, target);
          yield return new WaitForSeconds(effects.timeToExecuteAction);

          CopyPiece(target);
          effects.StartEffect(AbilityEffects.EffectStage.MIDDLE, Piece, target);
          yield return new WaitForSeconds(effects.timeToEndEffect);
          effects.EndEffect(AbilityEffects.EffectStage.START, Piece, target);

          OnAbilityEnd?.Invoke();
          ParchisGameManager.Instance.ActionDoneServerRpc();

     }

     void CopyPiece(ParchisPiece target)
     {
          var go = Instantiate(target.controller.selectedPieceData.piecePrefab);

          copiedAbility = go.GetComponent<ParchisAbilityBase>();
          copiedAbility.overridePiece = Piece;
          copiedAbility.OnAbilityEnd += ResetPiece;

          go.GetComponent<NetworkObject>().Spawn();
          go.transform.SetParent(Piece.transform);
          go.GetComponent<Collider>().enabled = false;
          go.transform.localPosition = Vector3.zero;

          DisableColliderClientRpc(go.GetComponent<NetworkObject>());

     }

     [ClientRpc]
     void DisableColliderClientRpc(NetworkObjectReference target)
     {
          NetworkObject targetObject = target;

          Debug.Log(targetObject.gameObject.name);

          targetObject.gameObject.GetComponent<Collider>().enabled = false;
     }

     private void ClearOutlines()
     {
          foreach(var p in possibleTargets)
          {
               p.SetOutlineClientRpc(0);
          }
     }

     public override void ResetPiece()
     {
          base.ResetPiece();

          if (copiedAbility != null)
          {
               Piece.hasSecondaryColor = false;
               var go = copiedAbility.gameObject;
               Destroy(go);
               effects.EndEffect(AbilityEffects.EffectStage.MIDDLE, Piece, Piece);

               curTurnsInCosplay = 0;
               copiedAbility = null;
               possibleTargets.Clear();
          }
     }

     public void NewTurn(ulong id)
     {
          if(id == Piece.PlayerID && copiedAbility != null)
          {
               if(++curTurnsInCosplay >= maxTurnsInCosplay)
               {
                    ResetPiece();
               }
          }
     }

     public override string GetDesc()
     {
          return $@"Marin gets to cosplay!
<color=yellow>Condition:</color> There's an enemy piece within {spacesBackToCheck} spaces back
<color=blue>Effect:</color> Marin selects enemy piece to cosplay as, which makes her untargetable to that color and can use their ability.
Cosplay only lasts for {maxTurnsInCosplay} turns";
     }
}
