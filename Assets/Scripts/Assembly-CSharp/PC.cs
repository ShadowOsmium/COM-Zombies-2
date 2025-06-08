using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PC : MonoBehaviour
{
    private GameSceneCoopController coopController;
    private GameSceneController regularController;

    public bool isPaused = false;
    public bool isCursorLocked = false;
    public bool pausedFromFocusLoss = false;
    public static bool ForceUnlockCursor = false;

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button quitButton;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        coopController = FindObjectOfType<GameSceneCoopController>();
        regularController = FindObjectOfType<GameSceneController>();

        UnlockCursor();
        ApplyCursorState(SceneManager.GetActiveScene().name);

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                UnpauseGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        coopController = FindObjectOfType<GameSceneCoopController>();
        regularController = FindObjectOfType<GameSceneController>();
        ApplyCursorState(scene.name);
    }

    void PauseGame(bool fromFocusLoss = false)
    {
        if (!IsGamePlaying() || isPaused) return;

        isPaused = true;
        pausedFromFocusLoss = fromFocusLoss;
        Time.timeScale = 0f;

        if (regularController != null)
            regularController.OnGamePause();
        else if (coopController != null)
            coopController.OnGamePause();

        if (pausePanel != null)
            pausePanel.SetActive(true);

        UnlockCursor();
    }

    void UnpauseGame()
    {
        if (!IsGamePlaying() || !isPaused) return;

        isPaused = false;
        pausedFromFocusLoss = false;
        Time.timeScale = 1f;

        if (regularController != null)
            regularController.OnGameResume();
        else if (coopController != null)
            coopController.OnGameResume();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        StartCoroutine(DelayedCursorApply());
    }

    private IEnumerator DelayedCursorApply()
    {
        float timeout = 1f;
        float elapsed = 0f;

        while (!IsGamePlaying() && elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        ApplyCursorState(SceneManager.GetActiveScene().name);
    }



    void ApplyCursorState(string sceneName)
    {
        Debug.Log(string.Format("ApplyCursorState called. Scene: {0}, isPaused: {1}, IsGamePlaying: {2}, ShouldLockCursorForScene: {3}",
            sceneName,
            isPaused,
            IsGamePlaying(),
            ShouldLockCursorForScene(sceneName)));

        if (coopController != null)
            Debug.Log(string.Format("CoopController state: {0}", coopController.GamePlayingState));
        if (regularController != null)
            Debug.Log(string.Format("RegularController state: {0}", regularController.GamePlayingState));

        if (ShouldLockCursorForScene(sceneName) && IsGamePlaying() && !isPaused)
        {
            Debug.Log("Locking cursor");
            LockCursor();
        }
        else
        {
            Debug.Log("Unlocking cursor");
            UnlockCursor();
        }
    }


    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
    }

    public void UpdateCursorState()
    {
        if (isPaused || !IsGamePlaying())
        {
            UnlockCursor();
        }
        else
        {
            string sceneName = SceneManager.GetActiveScene().name;

            if (ShouldLockCursorForScene(sceneName))
            {
                LockCursor();
            }
            else
            {
                UnlockCursor();
            }
        }
    }

    bool ShouldLockCursorForScene(string sceneName)
    {
        if (sceneName.StartsWith("COMZ2_") || sceneName.StartsWith("COM2_") || sceneName.StartsWith("Lab_"))
            return true;

        switch (sceneName)
        {
            case "Church":
            case "Depot":
            case "GameTutorial":
            case "Junkyard":
                return true;
        }

        return false;
    }


    public bool IsGamePlaying()
    {
        if (coopController != null && coopController.GamePlayingState == CoMZ2.PlayingState.Gaming)
            return true;

        if (regularController != null && regularController.GamePlayingState == CoMZ2.PlayingState.Gaming)
            return true;

        return false;
    }   

    public void OnQuitButtonClicked()
    {
        Debug.Log("Quit button clicked, unpausing and quitting...");

        if (pausePanel != null)
        {
            Debug.Log("Hiding pause panel.");
            pausePanel.SetActive(false);
        }

        UnlockCursor();

        if (regularController != null)
        {
            Debug.Log("Calling OnGameQuit on regularController.");
            regularController.OnGameQuit();
        }
        else if (coopController != null)
        {
            Debug.Log("Calling OnGameQuit on coopController.");
            coopController.OnGameQuit();
        }
        else
        {
            Debug.Log("No controller found, loading MainMenu.");
            SceneManager.LoadScene("MainMenu");
        }
    }
}