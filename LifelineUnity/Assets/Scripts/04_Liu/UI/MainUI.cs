using System.Collections;
using System.Collections.Generic;
using UI_2_SuperScroll;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 主界面UI
/// </summary>
public class MainUI : MonoBehaviour {

    public InfiniteListView mainListView;  // 滚动聊天框部件
    public Button replayButton;
    public Button playButton;

    int _nextToShowMessageIndex = 0;
    float _currentTimer = 0.0f;


    private void Start()
    {
        playButton.gameObject.SetActive(true);
        replayButton.gameObject.SetActive(false);

        mainListView.InitListView(0, OnGetElement);
    }

    public void Init()
    {
        playButton.onClick.AddListener(GameApp.Instance.PlayGame);
        replayButton.onClick.AddListener(GameApp.Instance.RePlayGame);
    }

    void OnDestroy()
    {
        playButton.onClick.RemoveAllListeners();
        replayButton.onClick.RemoveAllListeners();
    }

    public void OnPlayGame()
    {
        playButton.gameObject.SetActive(false);
        replayButton.gameObject.SetActive(false);


        SetToNewestMessage();
    }
    public void OnReplayGame()
    {
        playButton.gameObject.SetActive(false);
        replayButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!GameApp.Instance)
            return;
        var lst = GameApp.Instance.messageManager.historyDataList;
        if(_nextToShowMessageIndex < lst.Count)
        {
            _currentTimer += Time.deltaTime;

            var interval = GameApp.Instance.timerInterval;
            if(interval >= 0 && _currentTimer >= interval)
            {
                ShowNextMessage();
            }
        }
    }

    public void OnClickScroll(BaseEventData data)
    {
        float threhold = GameApp.Instance.clickThrehold;
        var ped = data as PointerEventData;
        var curPos = ped.position;
        var startPos = ped.pressPosition;
        if (Mathf.Abs(curPos.x - startPos.x) >= threhold || Mathf.Abs(curPos.y - startPos.y) >= threhold)
            return;

        ShowNextMessage();
    }

    public void ShowNextMessage()
    {
        var lst = GameApp.Instance.messageManager.historyDataList;
        if (_nextToShowMessageIndex >= lst.Count)
            return;

        if( lst[_nextToShowMessageIndex] is RevertMessageData)
        {
            var rd = lst[_nextToShowMessageIndex] as RevertMessageData;
            GameApp.Instance.messageManager.RevertTo(rd.toScene);
            return;
        }

        bool reachEnd = mainListView.IsContentTouchEnd(GameApp.Instance.allowOffset);

        mainListView.SetListElementCount(_nextToShowMessageIndex + 1, false);

        if (reachEnd)
            mainListView.MovePanelToElementIndex(_nextToShowMessageIndex, 0);

        _currentTimer = 0;
        _nextToShowMessageIndex++;
    }

    public void SetToNewestMessage()
    {
        var lst = GameApp.Instance.messageManager.historyDataList;
        _nextToShowMessageIndex = lst.Count - 1;

        mainListView.SetListElementCount(lst.Count);
        mainListView.MovePanelToElementIndex(_nextToShowMessageIndex, 0);

        _currentTimer = 0;
        _nextToShowMessageIndex++;
    }

    /// <summary>
    /// 每次刷新聊天框里的element的时候，调用该函数
    /// 有两种类型的element，选项 和 普通message
    /// 
    /// </summary>
    /// <param name="listView"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    ListViewElement OnGetElement(ListViewBase listView, int index)
    {

        var lst = GameApp.Instance.messageManager.historyDataList;
        var count = lst.Count;
        if (index < 0 || index >= count)
            return null;

        var data = lst[index];
        if(data is ChooseMessageData)
        {
            var ele = listView.NewListViewElement("ChoosePrefab", index);
            ele.InGroupElementList[0].SetData(data, index);
            return ele;

        }else if(data is NormalMessageData)
        {
            var ele = listView.NewListViewElement("MessagePrefab", index);
            ele.InGroupElementList[0].SetData(data, index);
            return ele;
        }

        return null;
    }

    public void OnRevertToMessage()
    {
        var lst = GameApp.Instance.messageManager.historyDataList;

        mainListView.SetListElementCount(lst.Count, false);
        mainListView.MovePanelToElementIndex(lst.Count - 1, 0);

        _nextToShowMessageIndex = lst.Count;
        _currentTimer = 0;
    }

}
