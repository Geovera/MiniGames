using Unity.Netcode;
using UnityEngine;

namespace GV.Core.Singletons
{
     [RequireComponent(typeof(NetworkObject))]
     public class Singleton<T> : NetworkBehaviour
          where T : Component
     {
          private static T _instance;
          public static T Instance
          {
               get
               {
                    if(_instance == null)
                    {
                         var objs = FindObjectOfType(typeof(T)) as T[];
                         if (objs != null && objs.Length > 0)
                              _instance = objs[0];
                         if(objs != null && objs.Length > 1)
                         {
                              Debug.LogError("There is more than one " + typeof(T).Name + " in the scene.");
                         }
                         /*if(_instance == null)
                         {
                              GameObject obj = new GameObject();
                              obj.name = string.Format("_{0}", typeof(T).Name);
                              _instance = obj.AddComponent<T>();
                         }*/
                    }
                    return _instance;
               }
          }
          protected static void SetInstance(T value)
          {
               _instance = value;
          }
     }
}