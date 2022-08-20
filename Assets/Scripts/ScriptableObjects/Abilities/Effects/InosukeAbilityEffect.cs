using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(InosukeAbility))]
public class InosukeAbilityEffect : AbilityEffects
{
     public AudioClip slashingSound;
     public AudioClip slashSound;
     public GameObject slashVisualEffectPrefab;
     public GameObject slashOverlayPrefab;

     GameObject overlay;
     GameObject visualEffect = null;

     public override void EndEffect()
     {
          EndEffectClientRpc();
     }

     [ClientRpc]
     private void EndEffectClientRpc()
     {
          Destroy(overlay);
          cameraController.EndFocus();

     }

     public override void StartEffect(EffectStage stage, ParchisPiece self, ParchisPiece other)
     {
          if (stage == EffectStage.START)
          {
               var attackDirection = other.transform.position - self.transform.position;
               FirstEffectClientRpc(self.NetworkObject, other.NetworkObject);
          }
          else
          {
               SecondEffectClientRpc();
          }
     }

     [ClientRpc]
     private void FirstEffectClientRpc(NetworkObjectReference self, NetworkObjectReference other)
     {
          NetworkObject selfObject = self;
          NetworkObject otherObject = other;
          cameraController.FocusOn(selfObject.transform);
          visualEffect = Instantiate(slashVisualEffectPrefab, selfObject.transform.position, selfObject.transform.rotation);
          visualEffect.transform.LookAt(otherObject.transform);

          audioSource.PlayOneShot(slashingSound);
     }

     [ClientRpc]
     private void SecondEffectClientRpc()
     {
          Destroy(visualEffect);

          audioSource.PlayOneShot(slashSound);
          audioSource.PlayOneShot(slashSound);
          overlay = Instantiate(slashOverlayPrefab);
     }

}
