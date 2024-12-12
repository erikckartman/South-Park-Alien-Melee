using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Menu pages")]
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private GameObject characterSelectCanvas;
    public static GameObject playerPrefab;

    [Header("Characters prefab")]
    [SerializeField] private GameObject[] selectedCharacter;

    public void GoToCharacterSelect()
    {
        menuCanvas.SetActive(false);
        characterSelectCanvas.SetActive(true);
    }

    public void GoToMainMenu()
    {
        characterSelectCanvas.SetActive(false);
        menuCanvas.SetActive(true);
    }

    public void GoToGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void SelectCharacter(int choice)
    {
        playerPrefab = selectedCharacter[choice];
    }
}
