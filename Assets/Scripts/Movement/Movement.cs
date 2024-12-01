﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    private float jumpDistance = 2f;  
    private float jumpHeight = 0.25f;
    private float jumpDuration = 0.2f;
    private float tiltAngle = 10f;

    private bool isJumping = false;
    private bool tiltToLeft = true;

    public static Vector3 playerPosition { get; private set; }

    private void Update()
    {
        playerPosition = transform.position;

        if (!isJumping)
        {
            MovePlayer();
        }
    }

    private void MovePlayer()
    {
       float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * moveZ + right * moveX;

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);

            StartCoroutine(Jump(moveDirection));
        }
    }

    private IEnumerator Jump(Vector3 direction)
    {
        isJumping = true;

        Vector3 startPosition = transform.position;

        Vector3 targetPosition = startPosition + direction.normalized * jumpDistance;

        Quaternion startRotation = transform.rotation;
        Quaternion tiltRotation = tiltToLeft
            ? Quaternion.Euler(transform.eulerAngles + new Vector3(0, 0, tiltAngle))
            : Quaternion.Euler(transform.eulerAngles + new Vector3(0, 0, -tiltAngle));

        tiltToLeft = !tiltToLeft;

        float elapsedTime = 0;

        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / jumpDuration;

            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, progress);

            currentPosition.y = startPosition.y + jumpHeight * Mathf.Sin(Mathf.PI * progress);

            transform.rotation = Quaternion.Lerp(startRotation, tiltRotation, Mathf.Sin(Mathf.PI * progress));

            transform.position = currentPosition;

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = startRotation;
        isJumping = false;
    }
}
