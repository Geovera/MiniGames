using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(HCLWAbility))]
public class HCLWAbilityEffect : AbilityEffects
{

     private CoinThrowOverlay coinOverlay;

     public AudioClip tossCoinAudio, tossCoinWitcherAudio, failureAudio, successAudio;

     public override void Start()
     {
          base.Start();
          coinOverlay = GameObject.FindGameObjectWithTag("CoinOverlay").GetComponent<CoinThrowOverlay>();
     }

     public override void EndEffect()
     {
          EndEffectClientRpc();
     }

     [ClientRpc]
     private void EndEffectClientRpc()
     {
          coinOverlay.coin.Reset();
          coinOverlay.successText.SetActive(false);
          coinOverlay.failureText.SetActive(false);
          coinOverlay.panel.SetActive(false);
     }

     public override void StartEffect(EffectStage stage, ParchisPiece self, ParchisPiece other)
     {
          StartEffectClientRpc(self.NetworkObject, other.NetworkObject, stage == EffectStage.START ? 0 : 1);

     }

     [ClientRpc]
     private void StartEffectClientRpc(NetworkObjectReference self, NetworkObjectReference other, int index)
     {
          StartCoroutine(EffectCoroutine(index));
     }

     IEnumerator EffectCoroutine(int index)
     {
          coinOverlay.coin.Reset();
          coinOverlay.panel.SetActive(true);
          audioSource.PlayOneShot(tossCoinAudio);
          audioSource.PlayOneShot(tossCoinWitcherAudio);
          coinOverlay.coin.flipCoin2(index);

          yield return new WaitForSeconds(2.5f);

          if (index == 0)
          {
               coinOverlay.successText.SetActive(true);
               audioSource.PlayOneShot(successAudio, .25f);
          }
          else
          {

               coinOverlay.failureText.SetActive(true);
               audioSource.PlayOneShot(failureAudio, .1f);
          }
     }

}
