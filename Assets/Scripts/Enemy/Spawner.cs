using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Spawner : MonoBehaviour
{
    public static bool canSpawnEnemies = false;
    [SerializeField] private NetworkRunner runner;

    [SerializeField] private GameObject enemyPrefab;
    private float spawnInterval = 5f;
    private int currentEnemies = 0;
    private int allEnemies = 50;

    [SerializeField] private Vector2 minPos;
    [SerializeField] private Vector2 maxPos;
    [Networked] private Vector3 spawnPos { get; set; }

    private void Start()
    {
        InvokeRepeating("Spawn", 0, spawnInterval);
    }

    private void Spawn()
    {
        if(runner != null)
        {
            if (runner.IsServer && allEnemies > 0 && currentEnemies < 5 && canSpawnEnemies)
            {
                Debug.Log($"Spawning enemy... (AllEnemies: {allEnemies}, CurrentEnemies: {currentEnemies})");

                spawnPos = new Vector3(UnityEngine.Random.Range(minPos.x, maxPos.x), 50f, UnityEngine.Random.Range(minPos.y, maxPos.y));

                if (runner.IsServer)
                {
                    var enemyInstance = runner.Spawn(enemyPrefab, spawnPos, Quaternion.identity);
                    if (enemyInstance != null)
                    {
                        Debug.Log("Enemy spawned successfully!");
                        allEnemies--;
                        currentEnemies++;
                    }
                    else
                    {
                        Debug.LogError("Failed to spawn enemy!");
                    }
                }
            }
            else
            {
                Debug.Log("Spawn conditions not met.");
            }
        }
        else
        {
            Debug.Log($"Runner = {runner}");
        }
    }
}