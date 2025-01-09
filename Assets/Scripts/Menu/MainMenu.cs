using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    [Header("Menu pages")]
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private GameObject characterSelectCanvas;
    [SerializeField] private GameObject gameIntro;
    public static GameObject playerPrefab;
    [SerializeField] private GameObject loadingScreen;

    [Header("Characters prefab")]
    [SerializeField] private GameObject[] selectedCharacter;
    [Header("Other")]
    [SerializeField] private VideoPlayer introSP;
    [SerializeField] private AudioSource introAudio;
    
    private int ee;
    [Header("Win Screen Stuff")]
    [SerializeField] private VideoPlayer introAD;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject introADgd;

    private void Awake()
    {
        if(introAD == null || introADgd == null || winScreen == null) return;

        ee = Random.Range(1, 5);
        if(ee == 2){
            introADgd.SetActive(true);
            introAD.loopPointReached += OnADVideoEnded;
        } else 
        {
            winScreen.SetActive(true);
        }
    }
    private void Start()
    {
        if(introSP == null) return;

        introSP.loopPointReached += OnVideoEnded;
    }

    private void Update()
    {
        if(introSP != null){
            if (introSP.isPlaying && Input.GetKeyDown(KeyCode.S))
            {
                introSP.Stop();
                OnVideoEnded(introSP);
            }
        }
    }
    public void GoToCharacterSelect()
    {
        menuCanvas.SetActive(false);
        characterSelectCanvas.SetActive(true);
        loadingScreen.SetActive(false);
    }

    public void GoToMainMenu()
    {
        characterSelectCanvas.SetActive(false);
        menuCanvas.SetActive(true);
        introAudio.Play();
    }

    public void GoToGame()
    {
        loadingScreen.SetActive(true);
        SceneManager.LoadScene("SampleScene");
    }

    public void SelectCharacter(int choice)
    {
        playerPrefab = selectedCharacter[choice];
    }

    public void GoToMenuScene()
    {
        SceneManager.LoadScene("Menu");
    }

    private void OnVideoEnded(VideoPlayer vp)
    {
        gameIntro.SetActive(false);
        GoToMainMenu();
    }

    private void OnADVideoEnded(VideoPlayer vp)
    {
        introADgd.SetActive(false);
        winScreen.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
