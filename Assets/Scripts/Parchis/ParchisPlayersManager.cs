using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GV.Core.Singletons;
using Unity.Netcode;


public delegate void Notify(ulong id);

public class ParchisPlayersManager : Singleton<ParchisPlayersManager>
{
     private NetworkVariable<int> playersInGame = new NetworkVariable<int>();

     public event Notify OnPlayerCountChanged;
     public NetworkVariable<int> PlayersInGameNet
     {
          get { return playersInGame; }
     }
     public int PlayersInGame
     {
          get
          {
               return playersInGame.Value;
          }
     }
    // Start is called before the first frame update
    void Start()
    {
          SetInstance(this);
          NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
          {
               if(IsServer)
               {
                    playersInGame.Value++;
                    OnPlayerCountChanged?.Invoke(id);
               }
          };

          NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
          {
               if (IsServer)
               {
                    playersInGame.Value--;
                    OnPlayerCountChanged?.Invoke(id);
               }
          };
     }

     public Color UserToColor(ulong userID)
     {
          switch (userID)
          {
               case 0:
                    return Color.red;
               case 1:
                    return Color.blue;
               case 2:
                    return Color.yellow;
               case 3:
                    return Color.green;
               default:
                    return Color.black;
          }
     }

}
