using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MarinAbilityEffect : AbilityEffects
{
     public override void EndEffect(EffectStage stage, ParchisPiece self, ParchisPiece other)
     {
          if (stage == EffectStage.START)
          {
               EndEffectClientRpc();
          }
          else
          {
               SetPieceRendererClientRpc(self.NetworkObject, true);
          }
     }

     [ClientRpc]
     private void EndEffectClientRpc()
     {
          cameraController.EndFocus();
     }

     public override void StartEffect(EffectStage stage, ParchisPiece self, ParchisPiece other)
     {
          if (stage == EffectStage.START)
          {
               FirstEffectClientRpc(self.NetworkObject);
          }
          else
          {
               SetPieceRendererClientRpc(self.NetworkObject, false);
          }
     }

     [ClientRpc]
     private void SetPieceRendererClientRpc(NetworkObjectReference self, bool enabled)
     {
          NetworkObject selfObject = self;
          SetPieceRenderer(selfObject.gameObject, enabled); ;
     }

     private void SetPieceRenderer(GameObject go, bool enabled)
     {
          var renderers = go.transform.Find("Pivot").GetComponentsInChildren<Renderer>();

          foreach (var r in renderers)
          {
               r.enabled = enabled;
          }
     }

     [ClientRpc]
     private void FirstEffectClientRpc(NetworkObjectReference self)
     {
          NetworkObject selfObject = self;
          cameraController.FocusOn(selfObject.transform);
     }

     public override void EndEffect()
     {
          // Do nothing
     }
}
