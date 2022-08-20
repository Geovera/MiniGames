using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RumblingBojjiAbility : ParchisAbilityBase
{

     private Coroutine placeCoroutine;
     public override bool CanUseSelf()
     {
          if(base.CanUseSelf() == false)
          {
               return false;
          }

          foreach(var targetPiece in Piece.controller.pieces)
          {
               if(targetPiece.CanPlace(Piece.controller.startTile, 6))
               {
                    return true;
               }
          }

          return false;
     }

     public override bool CanUseToOther(ParchisPiece other)
     {
          return false;
     }

     public override string GetDesc()
     {
          return $@"Bojji starts the Rumbling and summons an ally
<color=yellow>Condition:</color> There's a unit in home that can be placed on the start tile. Can't use if this piece was summoned with the ability
<color=blue>Effect:</color> Bojji selects an ally piece in home to move to start";
     }

     public override bool IsValidTarget(ParchisPiece target)
     {
          return Piece.PlayerID == target.PlayerID && target.InHome;
     }

     public override bool Use(ParchisPiece target, ParchisGameManager.GameState state)
     {
          switch(state)
          {
               case ParchisGameManager.GameState.CHOOSING_ACTION:
                    OutlineTargets();
                    hasUsedAbility = true;
                    break;
               case ParchisGameManager.GameState.PERFORMING_ABILITY:
                    if (placeCoroutine == null)
                    {
                         placeCoroutine = StartCoroutine(UseCoroutine(target));
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

          foreach (var targetPiece in Piece.controller.pieces)
          {
               if (true == targetPiece.InHome)
               {
                    targetPiece.SetOutlineClientRpc(1);
               }
          }

     }

     IEnumerator UseCoroutine(ParchisPiece target)
     {
          Piece.controller.RemovePiecesEffect();
          target.SetOutlineClientRpc(2);
          target.InHome = false;
          var startTile = Piece.controller.startTile;
          var distance = Vector3.Distance(target.transform.position, startTile.GetPlacePosition());
          var something = distance * .02f;
          var moveCoroutine = StartCoroutine(target.MoveToTarget(startTile.GetPlacePosition(), something));
          effects.StartEffect(AbilityEffects.EffectStage.START, Piece, target);
          yield return new WaitForSeconds(effects.timeToExecuteAction);


          StopCoroutine(moveCoroutine);
          distance = Vector3.Distance(target.transform.position, startTile.GetPlacePosition());
          something = distance * .5f;
          StartCoroutine(target.MoveToTarget(startTile.GetPlacePosition(), something));
          effects.StartEffect(AbilityEffects.EffectStage.END, Piece, null);
          yield return new WaitForSeconds(effects.timeToEndEffect);
          effects.EndEffect();

          target.curTile = startTile;
          target.SealAbility();
          startTile.Occupy(target);

          placeCoroutine = null;
          OnAbilityEnd?.Invoke();
          ParchisGameManager.Instance.ActionDoneServerRpc();
     }

}
