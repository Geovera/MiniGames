using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public delegate void AbilityNotify();

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(ParchisPiece))]
public abstract class ParchisAbilityBase : NetworkBehaviour
{
     public string abilityName;

     public bool hasUsedAbility = false;

     public AbilityEffects effects;
     public AbilityNotify OnAbilityEnd;

     protected ParchisPiece piece;
     public ParchisPiece overridePiece;

     protected ParchisPiece Piece { get { return overridePiece != null ? overridePiece : piece; } }

     public void Start()
     {
          piece = GetComponent<ParchisPiece>();
          effects = GetComponent<AbilityEffects>();
     }

     public virtual void Init()
     {
          piece = GetComponent<ParchisPiece>();
          effects = GetComponent<AbilityEffects>();
     }
     public virtual bool CanUseSelf()
     {
          return !(true == hasUsedAbility || null == Piece.curTile);
     }
     public abstract bool CanUseToOther(ParchisPiece other);
     public abstract bool Use(ParchisPiece target, ParchisGameManager.GameState state);
     public virtual void ResetPiece()
     {
          hasUsedAbility = false;
     }

     public abstract bool IsValidTarget(ParchisPiece target);

     public abstract string GetDesc();
}
