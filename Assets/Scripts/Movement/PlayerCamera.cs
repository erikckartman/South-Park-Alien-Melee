using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerCamera : NetworkBehaviour
{
    public Transform target; 
    private float distance = 5.0f;
    private float sensitivityX = 3.0f;
    private float sensitivityY = 1.5f;
    private float minY = -40f;
    private float maxY = 80f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    [Networked] private float networkRotationX { get; set; }
    [Networked] private float networkRotationY { get; set; }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasInputAuthority && target != null)
        {
            HandleCameraInput();
            networkRotationX = rotationX;
            networkRotationY = rotationY;
        }
        else if (target != null)
        {
            rotationX = Mathf.Lerp(rotationX, networkRotationX, Time.deltaTime * 10);
            rotationY = Mathf.Lerp(rotationY, networkRotationY, Time.deltaTime * 10);
        }

        if (target != null)
        {
            UpdateCameraPosition();
        }
    }

    private void HandleCameraInput()
    {
        rotationX += Input.GetAxis("Mouse X") * sensitivityX;
        rotationY -= Input.GetAxis("Mouse Y") * sensitivityY;
        rotationY = Mathf.Clamp(rotationY, minY, maxY);
    }

    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);
        Vector3 targetPosition = target.position - rotation * Vector3.forward * distance;

        transform.position = targetPosition;
        transform.LookAt(target.position);
    }
}
