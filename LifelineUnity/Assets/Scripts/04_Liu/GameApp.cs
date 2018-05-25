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
    [Tooltip("鼠标点击时，如果鼠标位移超过这个值，则判断为点击无效")]
    public float clickThrehold = 10.0f;
    [Tooltip("有新消息出现时，自动上滑的阈值")]
    public float allowOffset = 50.0f;
    [Tooltip("回溯时，一条消息需要滚多少次才滚完")]
    public int revertDisappearCount = 10;
    [Tooltip("回溯时，滚一次的事件间隔  ")]
    public float revertDisappearInterval = 0.5f;
    [Tooltip("按钮默认颜色")]
    public Color normalColor = Color.white;
    [Tooltip("按钮刚被选择后的颜色")]
    public Color selectedColor = Color.white;
    [Tooltip("按钮已选择的颜色")]
    public Color hasChoosedColor = Color.white;
    [Tooltip("两次有效点击屏幕的时间间隔")]
    public float doubleClickInterval = 0.5f;
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

    [NonSerialized]
    public string status_savePath = "/status.json";

    [NonSerialized]
    public string history_savePath = "/history.json";
    public bool m_isGameOver = false;

    private void Awake()
    {
        _instance = this;

        messageManager = new MessageManager();

        LoadStoryData();

        status_savePath = Application.persistentDataPath + status_savePath;
        history_savePath = Application.persistentDataPath + history_savePath;

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

        JSONArray a = new JSONArray();
        foreach(var h in messageManager.historyDataList)
        {
            var node = h.ToJSONNode();
            a.Add(node);
        }

        JSONClass saveData = new JSONClass();
        saveData["status"] = status;
        saveData["history"] = a;
        saveData.SaveToFile(status_savePath);

        //status.SaveToFile(status_savePath);
    }

    /// <summary>
    /// 如果有存档数据，则读取存档数据
    /// 如果没有，则从 Start 开始剧情
    /// </summary>
    void LoadStatusData()
    {
        if (File.Exists(status_savePath))
        {
            Debug.Log(status_savePath);
            var saveData = JSONNode.LoadFromFile(status_savePath);
            status = saveData["status"];

            var history = saveData["history"];
            messageManager.OnLoadHistoryData(history);
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
        ParseText.AtScene(this, status["atScene"]);

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

}
