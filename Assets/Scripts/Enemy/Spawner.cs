using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.AI;

public class Spawner : MonoBehaviour
{
    public static bool canSpawnEnemies = false;
    [SerializeField] private NetworkRunner runner;

    [SerializeField] private GameObject enemyPrefab;
    private float spawnInterval = 5f;
    [HideInInspector] public int currentEnemies = 0;
    [HideInInspector] public int allEnemies = 50;

    [SerializeField] private Vector2 minPos;
    [SerializeField] private Vector2 maxPos;
    [Networked] private Vector3 spawnPos { get; set; }
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI enemyCurrentText;
    [SerializeField] private TextMeshProUGUI enemyAllText;

    private void Start()
    {
        InvokeRepeating("Spawn", 0, spawnInterval);
    }

    private void Update()
    {
        if (currentEnemies <= 0 && allEnemies <= 0)
        {
            SceneManager.LoadScene("WinScreen");
        }
    }

    private void Spawn()
    {
        if(runner != null)
        {
            if (runner.IsServer && allEnemies > 0 && currentEnemies < 5 && canSpawnEnemies)
            {
                Debug.Log($"Spawning enemy... (AllEnemies: {allEnemies}, CurrentEnemies: {currentEnemies})");

                Vector3 randPos = new Vector3(UnityEngine.Random.Range(minPos.x, maxPos.x), 50f, UnityEngine.Random.Range(minPos.y, maxPos.y));

                if (Physics.Raycast(randPos, Vector3.down, out RaycastHit hit, Mathf.Infinity))
                {
                    NavMeshHit navHit;
                    if (NavMesh.SamplePosition(hit.point, out navHit, 1f, NavMesh.AllAreas))
                    {
                        if (runner.IsServer)
                        {
                            spawnPos = navHit.position + Vector3.up * 2f;
                            var enemyInstance = runner.Spawn(enemyPrefab, spawnPos, Quaternion.identity);
                            if (enemyInstance != null)
                            {
                                Debug.Log("Enemy spawned successfully!");
                                allEnemies--;
                                currentEnemies++;
                                UpdateUI();
                            }
                            else
                            {
                                Debug.LogError("Failed to spawn enemy!");
                            }
                        }
                    }
                }
                    
            }
            else
            {
                Debug.Log("Spawn conditions not met.");
            }
        }
        else if(allEnemies <= 0 && currentEnemies <= 0)
        {
            SceneManager.LoadScene("WinScreen");
        }
        else
        {
            Debug.Log($"Runner = {runner}");
        }
    }

    public void UpdateUI()
    {
        enemyCurrentText.text = "Current: " + currentEnemies.ToString();
        enemyAllText.text = "All: " + allEnemies.ToString();
    }
}