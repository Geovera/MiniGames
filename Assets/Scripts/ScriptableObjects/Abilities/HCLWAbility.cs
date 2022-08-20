using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HCLWAbility : ParchisAbilityBase
{
     public uint addToRoll = 6;
     public override bool CanUseSelf()
     {
          if (base.CanUseSelf() == false)
          {
               return false;
          }

          uint moves = addToRoll + Piece.controller.currentRoll;
          return Piece.CanMove(moves);
     }

     public override bool CanUseToOther(ParchisPiece other)
     {
          return false;
     }

     public override string GetDesc()
     {
          return $@"Hardcode Leveling warrior throws a lucky coin!
<color=yellow>Condition:</color> HCLW can occupy a tile that's current roll + {addToRoll} spaces away
<color=blue>Effect:</color> A coin is thrown. If it lands of face, piece advances current roll + {addToRoll} spaces";
     }

     public override bool IsValidTarget(ParchisPiece target)
     {
          return true;
     }

     public override bool Use(ParchisPiece target, ParchisGameManager.GameState state)
     {
          StartCoroutine(UseCoroutine(target));

          return true;
     }

     IEnumerator UseCoroutine(ParchisPiece target)
     {
          uint moves = addToRoll + Piece.controller.currentRoll;
          hasUsedAbility = true;

          int val = Random.Range(0, 2);
          if(val == 0)
          {
               effects.StartEffect(AbilityEffects.EffectStage.START, Piece, target);
          }
          else
          {
               effects.StartEffect(AbilityEffects.EffectStage.MIDDLE, Piece, target);
          }
          yield return new WaitForSeconds(effects.timeToExecuteAction);
          effects.EndEffect();
          if (val == 0)
          {
               yield return StartCoroutine(Piece.MoveToTarget(moves, 3f));
          }

          OnAbilityEnd?.Invoke();
          ParchisGameManager.Instance.ActionDoneServerRpc();
     }
}
