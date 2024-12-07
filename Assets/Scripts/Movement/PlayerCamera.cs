﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerCamera : MonoBehaviour
{
    public Transform target; 
    private float distance = 5.0f;
    private float sensitivityX = 3.0f;
    private float sensitivityY = 1.5f;
    private float minY = -40f;
    private float maxY = 80f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    private void LateUpdate()
    {
        if(target != null)
        {
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;
            rotationY -= Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minY, maxY);

            Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);
            Vector3 targetPosition = target.position - rotation * Vector3.forward * distance;

            transform.position = targetPosition;
            transform.LookAt(target.position);
        }
    }


}
