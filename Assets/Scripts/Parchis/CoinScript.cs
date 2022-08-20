using EasyButtons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinScript : MonoBehaviour
{
	[SerializeField]
	private float jumpForce = 5f;

	Rigidbody rb;

	private float headsVel = 5f, tailVel = 6f;
	private Vector3 initialPos, initialRot;

     void Start()
	{
		rb = GetComponent<Rigidbody>();
		initialPos = transform.position;
		initialRot = transform.eulerAngles;
	}

     public void Reset()
     {
		transform.position = initialPos;
		transform.eulerAngles = initialRot;
     }

     [Button]
	// Update is called once per frame
	public void flipCoin2(int side)
	{
		float rotVel = headsVel;
		if (side == 1)
		{ //tail
			rotVel = tailVel;
		}



		//transform.position = new Vector3(transform.position.x, height, transform.position.z);

		rb.velocity = (Vector3.up * jumpForce);
		rb.angularVelocity = (Vector3.right * rotVel);
	}
}
