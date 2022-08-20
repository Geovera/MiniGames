using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(RumblingBojjiAbility))]
public class RumblingBojjiAbilityEffect : AbilityEffects
{

     public AudioClip rumblingSound;
     public GameObject rumblingVisualEffectPrefab;

     private GameObject visualEffect;
     public override void EndEffect()
     {
          //throw new System.NotImplementedException();
          EndEffectClientRpc();
     }

     [ClientRpc]
     private void EndEffectClientRpc()
     {
          cameraController.EndFocus();
          cameraController.ShakeEnd();
     }

     public override void StartEffect(EffectStage stage, ParchisPiece self, ParchisPiece other)
     {
          if (stage == EffectStage.START)
          {
               FirstEffectClientRpc(other.NetworkObject);
          }
          else
          {
               SecondEffectClientRpc();
          }
     }

     [ClientRpc]
     private void FirstEffectClientRpc(NetworkObjectReference target)
     {
          NetworkObject targetObject = target;
          cameraController.FocusOn(targetObject.transform);
          cameraController.ShakeStart();
          visualEffect = Instantiate(rumblingVisualEffectPrefab, targetObject.transform);

          audioSource.PlayOneShot(rumblingSound);
     }

     [ClientRpc]
     private void SecondEffectClientRpc()
     {
          Destroy(visualEffect);
     }
}
