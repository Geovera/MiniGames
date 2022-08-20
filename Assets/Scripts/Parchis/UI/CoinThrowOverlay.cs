using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinThrowOverlay : MonoBehaviour
{
     public GameObject successText, failureText, panel;

     public CoinScript coin;
    // Start is called before the first frame update
    void Start()
    {
          successText.SetActive(false);
          failureText.SetActive(false);
    }

}
