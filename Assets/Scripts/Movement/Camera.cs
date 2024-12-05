using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    [SerializeField]private Transform target; 
    private float distance = 5.0f;
    private float sensitivityX = 3.0f;
    private float sensitivityY = 1.5f;
    private float minY = -40f;
    private float maxY = 80f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void LateUpdate()
    {
        if (target == null) return;

        rotationX += Input.GetAxis("Mouse X") * sensitivityX;
        rotationY -= Input.GetAxis("Mouse Y") * sensitivityY;
        rotationY = Mathf.Clamp(rotationY, minY, maxY);

        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);
        Vector3 targetPosition = target.position - rotation * Vector3.forward * distance;

        transform.position = targetPosition;
        transform.LookAt(target.position);
    }
}
