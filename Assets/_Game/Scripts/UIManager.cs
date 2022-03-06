using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    #region Singleton
    public static UIManager Instance { get { return instance; } }
    private static UIManager instance;

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

    [BoxGroup("Panels")]
    [SerializeField] GameObject TapToStartPanel, GameInPanel, WinPanel, LosePanel;

    public void StartTheGame()
    {
        GameManagement.Instance.StartTheGame();

        TapToStartPanel.SetActive(false);

        GameInPanel.SetActive(true);
    }

    public void WinTheGame()
    {
        WinPanel.SetActive(true);

        GameInPanel.SetActive(false);
    }

    public void LoseTheGame()
    {
        LosePanel.SetActive(true);

        GameInPanel.SetActive(false);
    }

    public void NextLevel()
    {
        GameManagement.Instance.NextLevel();
    }

    public void TryAgain()
    {
        GameManagement.Instance.TryAgain();
    }
}
