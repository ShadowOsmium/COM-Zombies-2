using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using CoMZ2;

public class GameInitUIController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage videoImage;
    public RenderTexture renderTexture;
    public string defaultNextScene = "GameCover";

    private bool videoSkippedOrEnded = false;

    private void Awake()
    {
        GameConfig.CheckGameConfig();
        GameData.CheckGameData();
    }

    private IEnumerator Start()
    {
        OpenClikPlugin.Initialize("A36F6C65-C1E3-47D4-AD07-AA8A6C90132C");

        if (videoPlayer == null || videoImage == null || renderTexture == null)
        {
            Debug.LogError("Missing components: VideoPlayer, RawImage, or RenderTexture.");
            yield break;
        }

        videoPlayer.targetTexture = renderTexture;
        videoImage.texture = renderTexture;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        Debug.Log("Preparing video...");
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        Debug.Log("Video is prepared. Playing...");
        videoPlayer.Play();

        // Register event and fallback check
        videoPlayer.loopPointReached += OnVideoFinished;
        StartCoroutine(CheckIfVideoDone());
    }

    private void Update()
    {
        if (videoSkippedOrEnded)
            return;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            SkipVideo("Mouse click (PC)");
        }
#elif UNITY_IOS || UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            SkipVideo("Touch input (Mobile)");
        }
#endif
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (!videoSkippedOrEnded)
        {
            SkipVideo("Video finished (event)");
        }
    }

    private IEnumerator CheckIfVideoDone()
    {
        while (!videoSkippedOrEnded)
        {
            if (videoPlayer.frame >= (long)(videoPlayer.frameCount - 1))
            {
                SkipVideo("Video finished (manual check)");
                yield break;
            }
            yield return null;
        }
    }

    private void SkipVideo(string reason)
    {
        if (videoSkippedOrEnded) return;

        Debug.Log("Skipping video: " + reason);
        videoSkippedOrEnded = true;
        videoPlayer.Stop();

        PushNotification.ReSetNotifications();

        if (GameData.Instance.is_enter_tutorial)
        {
            if (GameData.Instance.cur_quest_info == null)
                GameData.Instance.cur_quest_info = new QuestInfo();

            GameData.Instance.cur_quest_info.mission_type = MissionType.Tutorial;
            GameData.Instance.cur_quest_info.mission_day_type = MissionDayType.Tutorial;
            GameData.Instance.loading_to_scene = "GameTutorial";
            SceneManager.LoadScene("Loading");
        }
        else
        {
            SceneManager.LoadScene(defaultNextScene);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (videoPlayer == null || videoSkippedOrEnded)
            return;

        if (hasFocus)
        {
            if (!videoPlayer.isPlaying && videoPlayer.isPrepared)
            {
                Debug.Log("Resuming video on focus.");
                videoPlayer.Play();
            }
        }
        else
        {
            if (videoPlayer.isPlaying)
            {
                Debug.Log("Pausing video on focus lost.");
                videoPlayer.Pause();
            }
        }
    }
}
