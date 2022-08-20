using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InosukeAbility : ParchisAbilityBase
{

     public int maxTilesToAttack = 2;
     Tile attackableTile = null;

     public override bool CanUseSelf()
     {
          if(base.CanUseSelf() == false)
          {
               return false;
          }

          attackableTile = FindAttackableTile();

          return attackableTile != null;
     }

     private Tile FindAttackableTile()
     {
          Tile attackTile = null;
          int count = 0;
          Tile t = Piece.curTile;
          while(attackTile == null && count < maxTilesToAttack)
          {
               t = t.Next(Piece);
               if(t.isOccupied())
               {
                    var temp = t.currentPiece;
                    if (temp.isSameColor(Piece) == false && temp.canKill == true)
                    {
                         attackTile = t;
                    }
               }
               count++;
          }
          return attackTile;
     }

     public override bool CanUseToOther(ParchisPiece other) { return false; }

     public override bool Use(ParchisPiece target, ParchisGameManager.GameState state)
     {
          if (state != ParchisGameManager.GameState.CHOOSING_ACTION) return false;

          StartCoroutine(UseCoroutine());
          return true;
     }

     IEnumerator UseCoroutine()
     {
          effects.StartEffect(AbilityEffects.EffectStage.START, Piece, attackableTile.currentPiece);
          yield return new WaitForSeconds(effects.timeToExecuteAction);
          attackableTile.KillFirst();
          hasUsedAbility = true;
          effects.StartEffect(AbilityEffects.EffectStage.END, Piece, attackableTile.currentPiece);
          yield return new WaitForSeconds(effects.timeToEndEffect);
          effects.EndEffect();

          OnAbilityEnd?.Invoke();
          ParchisGameManager.Instance.ActionDoneServerRpc();

     }

     public override bool IsValidTarget(ParchisPiece target)
     {
          return true;
     }

     public override string GetDesc()
     {
          return $@"Inosuke slashes forward!
<color=yellow>Condition:</color> There's an attackable piece within {maxTilesToAttack} spaces
<color=blue>Effect:</color> Inouske kills an enemy piece wihtin {maxTilesToAttack} spaces";
     }
}
