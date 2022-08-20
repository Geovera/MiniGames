using EasyButtons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardBuilder : MonoBehaviour
{

     public ParchisBoardManager boardManager;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

     [Button]
     public void BuildBoard()
     {
          boardManager.CreateBoard();
     }

     [Button]
     void Clear()
     {
          boardManager.Clear();
     }
}
