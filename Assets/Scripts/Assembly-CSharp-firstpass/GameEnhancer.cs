using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnhancer : MonoBehaviour
{
    void Awake()
    {
        if (Application.isPlaying)
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }

    void OnEnable()
    {
        if (!Application.isPlaying) return;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        if (!Application.isPlaying) return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (IsRelevantScene(currentScene))
        {
            ClampToLimits();
        }

        StartCoroutine(CheckAndClampCurrencyLoop());
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsRelevantScene(scene.name))
        {
            ClampToLimits();
        }
    }

    private bool IsRelevantScene(string sceneName)
    {
        return sceneName == "UIMap" ||
               sceneName == "UICoopHall" ||
               sceneName == "UICoopRoom" ||
               sceneName == "UILottery" ||
               sceneName == "UIShop" ||
               sceneName == "InitScene" ||
               sceneName == "Loading";
    }

    private IEnumerator CheckAndClampCurrencyLoop()
    {
        while (true)
        {
            ClampToLimits();
            yield return new WaitForSeconds(0.01f);
        }
    }

    private void ClampToLimits()
    {
        if (GameData.Instance.total_cash > 1000000)
            GameData.Instance.total_cash = new GameDataInt(1000000);

        if (GameData.Instance.total_crystal > 800)
            GameData.Instance.total_crystal = new GameDataInt(800);

        if (GameData.Instance.total_voucher > 1250)
            GameData.Instance.total_voucher = new GameDataInt(1250);
    }
}
