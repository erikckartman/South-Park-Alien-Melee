using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Fusion;
using System;

public class Player : NetworkBehaviour
{
    [SerializeField] private NetworkTransform networkTransform;
    [SerializeField] private GameObject cameraTransform;
    private float jumpDistance = 2f;  
    private float jumpHeight = 0.25f;
    private float jumpDuration = 0.2f;
    private float tiltAngle = 10f;

    private bool isJumping = false;
    private bool tiltToLeft = true;

    [Networked]public Vector3 playerPosition { get; private set; }
    [Networked]public Quaternion playerQuaterion{ get; private set; }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            cameraTransform.SetActive(true);
        }
        else
        {
            cameraTransform.SetActive(false);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            if (!isJumping)
            {
                MovePlayer();
            }

            playerPosition = transform.position;
            playerQuaterion = transform.rotation;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, playerPosition, Time.deltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, playerQuaterion, Time.deltaTime * 10);
        }
    }

    private void MovePlayer()
    {
        if (GetInput(out NetworkInputData inputData))
        {
            Vector3 moveDirection = cameraTransform.transform.forward * inputData.moveDirection.y +
                                    cameraTransform.transform.right * inputData.moveDirection.x;

            moveDirection.y = 0;
            moveDirection.Normalize();

            if (moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
                StartCoroutine(Jump(moveDirection));
            }
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

            Quaternion currentRotation = Quaternion.Lerp(startRotation, tiltRotation, Mathf.Sin(Mathf.PI * progress));

            transform.position = currentPosition;
            transform.rotation = currentRotation;

            playerPosition = currentPosition;
            playerQuaterion = currentRotation;

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = startRotation;

        playerPosition = targetPosition;
        playerQuaterion = startRotation;

        isJumping = false;
    }

    public struct NetworkInputData : INetworkInput
    {
        public Vector2 moveDirection;
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInputData inputData = new NetworkInputData
        {
            moveDirection = new Vector2(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical")
            )
        };

        input.Set(inputData);
    }
}