using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class AbilityEffects : NetworkBehaviour
{
     public enum EffectStage
     {
          START,
          MIDDLE,
          END
     }
     public float timeToExecuteAction = 1f;
     public float timeToEndEffect = 2f;

     protected AudioSource audioSource;
     protected ParchisCameraController cameraController;

     public virtual void Start()
     {
          audioSource = GetComponent<AudioSource>();
          cameraController = Camera.main.transform.parent.GetComponent<ParchisCameraController>();
     }

     public abstract void StartEffect(EffectStage stage, ParchisPiece self, ParchisPiece other);

     public virtual void EndEffect(EffectStage stage, ParchisPiece self, ParchisPiece other) { }
     public abstract void EndEffect();
}
