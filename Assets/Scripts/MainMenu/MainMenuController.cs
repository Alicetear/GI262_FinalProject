using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject menuFrame;
    public GameObject optionsPanel;

    [Header("Scene Settings")]
    [SerializeField] public string gameSceneName = "GameScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [SerializeField] private GameObject continueButton;
    [SerializeField] private RectTransform[] menuActions;

    private void Awake()
    {
        if (continueButton == null) return;
        bool hasSave = RunCheckpoint.TryLoad(out _);
        continueButton.SetActive(hasSave);

        if (hasSave || menuActions == null || menuActions.Length == 0) return;
        var continueRect = (RectTransform)continueButton.transform;
        float gap = continueRect.anchoredPosition.y - menuActions[0].anchoredPosition.y;
        foreach (var action in menuActions)
            action.anchoredPosition += Vector2.up * gap;
    }

    public void ContinueGame()
    {
        if (!RunCheckpoint.TryLoad(out _)) return;
        RunCheckpoint.ResumeRequested = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    private void Start()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    public void StartGame()
    {
        RunCheckpoint.ResumeRequested = false;
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenOptions()
    {
        if (optionsPanel != null)
        {
            if (menuFrame != null) menuFrame.SetActive(false);
            optionsPanel.SetActive(true);
        }
    }

    public void CloseOptions()
    {
        if (optionsPanel != null)
        {
            if (menuFrame != null) menuFrame.SetActive(true);
            optionsPanel.SetActive(false);
        }
    }

    public void ExitGame()
    {
        Debug.Log("Exit Game");
        Application.Quit();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Restore normal game speed.
        SceneManager.LoadScene(mainMenuSceneName);
    }


}