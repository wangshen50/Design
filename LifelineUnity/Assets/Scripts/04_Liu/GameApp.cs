using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;


public class GameApp : MonoBehaviour {

    #region Singleton
    static GameApp _instance;
    public static GameApp Instance
    {
        get { return _instance; }
    }
    #endregion Singleton

    #region UI
    public MainUI mainUI;
    #endregion UI

    #region Message
    public MessageManager messageManager;

    [Tooltip("每隔多少秒弹出下一条信息，如果为负数，表示不会根据时间弹出")]
    public float timerInterval = -1.5f;
    public float clickThrehold = 10.0f;
    #endregion Message

    #region Data
    public JSONNode status = new JSONClass(); // JSONNode 结构的 存档数据
    public JSONNode scenes = new JSONClass();   // JSONNode结构的  剧情配置
    public JSONNode choices = new JSONClass(); // JSONNode结构的  选项配置
    #endregion Data

    #region Sound
    private SoundManager _soundManager;
    public SoundManager SoundManager
    {
        get {
            if (_soundManager == null)
                _soundManager = FindObjectOfType<SoundManager>();
            return _soundManager;
        }
    }
    #endregion  Sound

    private string status_savePath;
    public bool m_isGameOver = false;

    private void Awake()
    {
        _instance = this;

        messageManager = new MessageManager();

        LoadStoryData();

        status_savePath = Application.persistentDataPath + "/status.json";

        mainUI.Init();
    }

    // 
    void Update()
    {
        if (status["atScene"] != null)
        {
            ParseText.AtScene(this, status["atScene"]);     // set: status["atScene"] = null
        }
        else
        {
            if (m_isGameOver)
            {
                m_isGameOver = false;
                StartCoroutine(GameOver(0.3f));
            }
        }
    }

    void LoadStoryData()
    {
        // 以下方式通过 Adroid, Mac 测试, iOS不通过
        // string choices_file_path = Resources.Load("Data/choices_cn").ToString();
        // string scenes_file_path = Resources.Load("Data/waypoints_cn").ToString();
        // 以下方式通过 Adroid, Mac，iOS测试
        TextAsset choices_file = Resources.Load("Data/choices_cn") as TextAsset;
        TextAsset scenes_file = Resources.Load("Data/scenes_cn") as TextAsset;
        // Debug.Log("scenes_file = " + scenes_file);

        // 将文本解析成 JSONNode
        choices = JSONNode.Parse(choices_file.text);
        scenes = JSONNode.Parse(scenes_file.text);
    }

    #region StatusData
    public void SaveStatusData(string scene)
    {
        status["atScene"] = scene;
        status.SaveToFile(status_savePath);
    }

    /// <summary>
    /// 如果有存档数据，则读取存档数据
    /// 如果没有，则从 Start 开始剧情
    /// </summary>
    void LoadStatusData()
    {
        if (File.Exists(status_savePath))
        {
            status = JSONNode.LoadFromFile(status_savePath);
        }
        else
        {
            status["atScene"] = "Start";
        }

    }
    #endregion StatusData

    public void PlayGame()
    {
        LoadStatusData();

        mainUI.OnPlayGame();
    }
    public void RePlayGame()
    {
        m_isGameOver = false;
        SaveStatusData("Start");

        mainUI.OnReplayGame();
    }
    IEnumerator GameOver(float duration)
    {
        yield return new WaitForSeconds(duration);
        mainUI.replayButton.gameObject.SetActive(true);
    }

    public void OnMessagetListChange()
    {
        //mainUI.mainListView.SetListElementCount(messageManager.historyDataList.Count, false);
    }


}
