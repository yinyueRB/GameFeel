using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIController : MonoBehaviour
{
    [Header("UI 面板引用")]
    public GameObject developerPanel;
    public string gameSceneName = "GameScene";

    [Header("音效设置")]
    public AudioSource uiAudioSource;
    public AudioClip buttonClickClip;
    
    [Header("延迟设置")]
    public float soundDelay = 0.15f; // 音效播放延迟，以确保按钮音效播放完毕

    void Start()
    {
        if (developerPanel != null)
        {
            developerPanel.SetActive(false);
        }

        if (uiAudioSource == null)
        {
            //Debug.LogWarning("未赋值UI音频源，音效将无法播放！");
        }
    }

    private void PlayButtonClickSound()
    {
        if (uiAudioSource != null && buttonClickClip != null)
        {
            uiAudioSource.PlayOneShot(buttonClickClip);
            //Debug.Log("音效播放成功");
        }
        else
        {
            //Debug.LogError("音效播放失败：AudioSource 或 AudioClip 为空！");
        }
    }

    // 开始游戏，延迟加载场景，确保音效播放完毕
    public void OnStartGameButtonClick()
    {
        PlayButtonClickSound(); // 先播放音效
        // 延迟 soundDelay 秒加载场景
        Invoke("LoadGameScene", soundDelay);
    }

    // 退出游戏，延迟退出，确保音效播放完毕
    public void OnQuitGameButtonClick()
    {
        PlayButtonClickSound(); // 先播放音效
        // 延迟 soundDelay 秒退出
        Invoke("QuitGame", soundDelay);
    }

    // 显示制作团队面板逻辑
    public void ShowDeveloperPanel()
    {
        PlayButtonClickSound();
        if (developerPanel != null)
        {
            developerPanel.SetActive(true);
        }
    }

    // 隐藏制作团队面板逻辑
    public void HideDeveloperPanel()
    {
        PlayButtonClickSound();
        if (developerPanel != null)
        {
            developerPanel.SetActive(false);
        }
    }

    // 封装加载场景逻辑，供延迟调用
    private void LoadGameScene()
    {
        try
        {
            SceneManager.LoadScene(gameSceneName);
            //Debug.Log("开始游戏，加载场景：" + gameSceneName);
        }
        catch (System.Exception e)
        {
            //Debug.LogError("加载场景失败：" + e.Message);
        }
    }

    // 封装退出逻辑，供延迟调用
    private void QuitGame()
    {
        //Debug.Log("退出游戏");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}