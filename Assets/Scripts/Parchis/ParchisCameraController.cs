using MilkShake;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParchisCameraController : MonoBehaviour
{

     [SerializeField]
     private float mouseSensitivity = 3.0f;
     [SerializeField]
     private float cameraSpeed = 75.0f;

     private float rotationX;
     private float rotationY;

     [SerializeField]
     private Transform target;
     private Transform prevTarget;

     [SerializeField]
     private float distanceFromTarget = 3.0f;
     private float prevDistance;
     private bool lockDistance = false;

     private Vector3 currentRotation;
     private Vector3 smoothVelocity = Vector3.zero;

     [SerializeField]
     private float smoothTime = 3.0f;

     [SerializeField]
     private int minAngle = 15, maxAngle = 80;
     [SerializeField]
     private int minDistance = 1, maxDistance = 10;

     private bool isMoving = false;

     public ShakePreset shakePreset;

     //The saved shake instance we will be modifying
     private ShakeInstance myShakeInstance;

     private void Start()
     {
          prevTarget = target;
          prevDistance = distanceFromTarget;
          myShakeInstance = Shaker.ShakeAll(shakePreset);
          myShakeInstance.Stop(0f, false);

          rotationX = transform.localEulerAngles.x;
          rotationY = transform.localEulerAngles.y;
          currentRotation = transform.localEulerAngles;
     }

     private void Update()
     {
          if(Input.GetMouseButtonDown(1))
          {
               isMoving = true;
               Cursor.visible = false;
          }
          if (isMoving)
          {
               float mouseX = Input.GetAxis("Mouse Y") * -mouseSensitivity;
               float mouseY = Input.GetAxis("Mouse X") * mouseSensitivity;

               rotationX += mouseX;
               rotationY += mouseY;

               rotationX = Mathf.Clamp(rotationX, minAngle, maxAngle);

               Vector3 nextRotation = Vector3.zero;
               nextRotation.x = rotationX;
               nextRotation.y = rotationY;

               currentRotation = Vector3.SmoothDamp(currentRotation, nextRotation, ref smoothVelocity, smoothTime);

               transform.localEulerAngles = currentRotation;


          }

          if (lockDistance == false)
          {
               float delta = Input.mouseScrollDelta.y * 1.5f;
               distanceFromTarget -= delta;
               distanceFromTarget = Mathf.Clamp(distanceFromTarget, minDistance, maxDistance);
          }

          transform.position = Vector3.Slerp(transform.position, target.position - transform.forward * distanceFromTarget, cameraSpeed * Time.deltaTime);

          if (Input.GetMouseButtonUp(1))
          {
               isMoving = false;
               Cursor.visible = true;
          }
     }

     public void FocusOn(Transform newTarget)
     {
          prevTarget = target;
          target = newTarget;

          prevDistance = distanceFromTarget;
          distanceFromTarget = 5f;
          lockDistance = true;
     }

     public void EndFocus()
     {
          target = prevTarget;
          distanceFromTarget = prevDistance;
          lockDistance = false;
     }

     public void ShakeStart()
     {
          myShakeInstance.Start(1f);
     }

     public void ShakeEnd()
     {
          myShakeInstance.Stop(1f, false);
     }
}
