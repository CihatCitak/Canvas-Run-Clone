using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagement : MonoBehaviour
{
    #region Singleton
    public static GameManagement Instance { get { return instance; } }
    private static GameManagement instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    #endregion

    #region GameStates

    public enum GameStates
    {
        INMENU,
        START,
        WIN,
        LOSE
    }

    public GameStates GameState { get { return gameState; } }
    private GameStates gameState = GameStates.INMENU;

    [SerializeField] GameObject confettis;

    private int buildIndex;

    public void StartTheGame()
    {
        buildIndex = SceneManager.GetActiveScene().buildIndex;

        gameState = GameStates.START;

        CanvasController.Instance.StartRun();
    }

    private void WinTheGame()
    {
        gameState = GameStates.WIN;

        CanvasController.Instance.WinTheGame();

        CameraManager.Instance.WinTheGame();

        UIManager.Instance.WinTheGame();

        confettis.SetActive(true);
    }

    public void LoseTheGame()
    {
        gameState = GameStates.LOSE;

        CanvasController.Instance.LoseTheGame();

        CameraManager.Instance.LoseTheGAme();

        UIManager.Instance.LoseTheGame();
    }

    public void TryAgain()
    {
        SceneManager.LoadScene(buildIndex);
    }

    public void NextLevel()
    {
        int totalIndex = SceneManager.sceneCountInBuildSettings;

        if (buildIndex + 1 < totalIndex)
            SceneManager.LoadScene(buildIndex + 1);
        else
            SceneManager.LoadScene(0);
    }

    #endregion


    //Player enter the GameEndArea position
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CanvasRoot"))
        {
            WinTheGame();
        }
    }
}
