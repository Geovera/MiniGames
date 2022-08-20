using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

[System.Serializable]
public class ColorToMatData
{
     public PlayerColor color;
     public Material baseColorMat;
     public Material homeColorMat;
     public Material startBaseColorMat;
}

[System.Serializable]
public class ColorToMatDictionary : SerializableDictionaryBase<PlayerColor, ColorToMatData> { }

[CreateAssetMenu(menuName = "Colors/Colors Manager")]
public class PlayerColorManager : ScriptableObject
{


     [SerializeField]
     private ColorToMatDictionary colorsDic = new ColorToMatDictionary();

     public ColorToMatData GetColorData(PlayerColor color)
     {
          if (colorsDic.ContainsKey(color))
          {
               return colorsDic[color];
          }
          throw new System.Exception("No Pose with that state id found");
     }

}