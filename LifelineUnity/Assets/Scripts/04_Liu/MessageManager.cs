using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.Text.RegularExpressions;

[Serializable]
public abstract class MessageBase
{
    public abstract JSONNode ToJSONNode();
    public abstract void SetNodeData(string nodeStr);

    public static MessageBase ParseFromJSONNode(JSONNode node)
    {
        if(node is JSONData)
        {
            var str = node.Value;
            Regex r = new Regex(ChooseMessageData.strPattern);
            Match m = r.Match(str);

            Debug.Log(str);
            if(!m.Success)
            {
                NormalMessageData d = new NormalMessageData();
                d.SetNodeData(str);
                return d;
            }else
            {
                ChooseMessageData d = new ChooseMessageData();
                d.SetNodeData(str);
                return d;
            }
        }
        return null;
    }
}

public class ChoiceAction
{
    public string choice;
    public string identifier;

}

public class ChooseMessageData : MessageBase
{
    public const string strPattern = @"^choice\((\d)\)\[\[(.*)\]\]$";
    public const string strFormat = "choice({0})[[{1}]]";

    public int selectedIndex = -1; // -1 means not selected 

    public string identifier;
    public ChoiceAction[] actions;

    public JSONNode choiceNode;


    public override JSONNode ToJSONNode()
    {
        var selected = selectedIndex.ToString();
        var conbineStr = string.Format(strFormat, selected, identifier);

        JSONData data = new JSONData(conbineStr);
        return data;
    }

    public override void SetNodeData(string nodeStr)
    {
        Regex r = new Regex(ChooseMessageData.strPattern);
        Match m = r.Match(nodeStr);
        if(m.Success)
        {
            selectedIndex = int.Parse(m.Groups[1].ToString());
            identifier = m.Groups[2].ToString();

            var choiceIndex= int.Parse(identifier.Substring(8, identifier.Length - 8));
            choiceNode = GameApp.Instance.choices[choiceIndex];

            var choiceArr = choiceNode["actions"].AsArray;
            actions = new ChoiceAction[2];
            actions[0] = new ChoiceAction();
            actions[0].choice = choiceArr[0]["choice"];
            actions[0].identifier = choiceArr[0]["identifier"];

            actions[1] = new ChoiceAction();
            actions[1].choice = choiceArr[1]["choice"];
            actions[1].identifier = choiceArr[1]["identifier"];
        }
    }
}

public class NormalMessageData : MessageBase
{
    public string message;
    public string belongingScene;

    public override JSONNode ToJSONNode()
    {
        JSONData data = new JSONData(message);
        return data;
    }

    public override void SetNodeData(string nodeStr)
    {
        message = nodeStr;
    }
}

public class RevertMessageData : MessageBase
{
    public string toScene;
    public override void SetNodeData(string nodeStr)
    {
        throw new NotImplementedException();
    }
    public override JSONNode ToJSONNode()
    {
        throw new NotImplementedException();
    }

}

public class MessageManager  {

    public List<MessageBase> historyDataList = new List<MessageBase>();

    public void OnLoadHistoryData(JSONNode history)
    {
        historyDataList.Clear();

        var arr = history.AsArray;
        for(int i = 0; i < arr.Count; i++)
        {
            var node = arr[i];
            var m = MessageBase.ParseFromJSONNode(node);
            if (m != null)
                historyDataList.Add(m);
        }
    }

    public void DoSaveHistoryData()
    {
        JSONArray a = new JSONArray();
        foreach(var h in historyDataList)
        {
            var node = h.ToJSONNode();
            a.Add(node);
        }

        a.SaveToFile(GameApp.Instance.history_savePath);
    }

    public void AddOneNormalMessage(string str, string scene)
    {
        NormalMessageData message = new NormalMessageData();
        message.message = str;
        message.belongingScene = scene;

        historyDataList.Add(message);
    }

    /// <summary>
    /// choices with 
    /// 1. identifier
    /// 2. actions
    /// </summary>
    /// <param name="choices"></param>
    public void AddOneChoice(JSONNode choices)
    {
        ChooseMessageData data = new ChooseMessageData();
        data.choiceNode = choices;

        data.identifier = choices["identifier"];
        data.selectedIndex = -1;

        var choiceArr = choices["actions"].AsArray;
        data.actions = new ChoiceAction[2];
        data.actions[0] = new ChoiceAction();
        data.actions[0].choice = choiceArr[0]["choice"];
        data.actions[0].identifier = choiceArr[0]["identifier"];

        data.actions[1] = new ChoiceAction();
        data.actions[1].choice = choiceArr[1]["choice"];
        data.actions[1].identifier = choiceArr[1]["identifier"];

        historyDataList.Add(data);
    }

    public void AddOneRevertMessage(string toScene)
    {
        RevertMessageData data = new RevertMessageData();
        data.toScene = toScene;

        historyDataList.Add(data);
    }

    public void RevertTo(string scene)
    {
        Debug.Log("revertTo " + scene);
        bool find = false;
        for(int i = historyDataList.Count - 1; i >= 0; i--)
        {
            var message = historyDataList[i] as NormalMessageData;
            if (message != null)
            {
                if(message.belongingScene == scene)
                {
                    find = true;
                    historyDataList.RemoveAt(i);
                }else 
                {
                    if (find)
                        break;

                    historyDataList.RemoveAt(i);
                }
            }else {

                historyDataList.RemoveAt(i);
            }
        }

        GameApp.Instance.status["atScene"] = scene;
        GameApp.Instance.OnRevertToMessage();
    }

}
