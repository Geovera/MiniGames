using EasyButtons;
using GV.Core.Singletons;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class DiceScript : Singleton<DiceScript>
{
     [SerializeField]
     private TextMeshPro numberOutput;
     [SerializeField]
     private GameHUD hud;

     System.Random r;

     bool isRolling = false;
     uint currentValue = 0;

     float timeSinceChange = 0f;
     public float changeDelay = .15f;

     [SerializeField]
     private AudioClip diceThrowAudio;
     private AudioSource audioSource;
     // Start is called before the first frame update
     void Start()
     {
          r = new System.Random();
          audioSource = GetComponent<AudioSource>();

          SetInstance(this);
     }

     [ClientRpc]
     public void StartRollClientRpc()
     {
          isRolling = true;
     }

     public uint StopRoll()
     {
          currentValue = (uint)GetRandomRoll();
          StopRollClientRpc(currentValue);
          return currentValue;
     }

     [ClientRpc]
     private void StopRollClientRpc(uint finalValue)
     {
          isRolling = false;
          currentValue = finalValue;
          numberOutput.text = currentValue.ToString();
          audioSource.PlayOneShot(diceThrowAudio);
          hud.ChangeRollDie(currentValue);
     }

     int GetRandomRoll()
     {
          return r.Next(1, 7);
     }

     private void Update()
     {
          if(true == isRolling && timeSinceChange > changeDelay)
          {
               timeSinceChange = 0f;
               currentValue = (uint)GetRandomRoll();
               numberOutput.text = currentValue.ToString();
          }
          timeSinceChange += Time.deltaTime;
     }
}
