using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

[Serializable]
public class MessageBase
{

}

public class ChoiceAction
{
    public string choice;
    public string identifier;
}

public class ChooseMessageData : MessageBase
{
    public int selectedIndex = -1; // -1 means not selected 

    public string identifier;
    public ChoiceAction[] actions;

    public JSONNode choiceNode;

}

public class NormalMessageData : MessageBase
{
    public string message;
}

public class MessageManager  {

    public List<MessageBase> historyDataList = new List<MessageBase>();

    public void AddOneNormalMessage(string str)
    {
        NormalMessageData message = new NormalMessageData();
        message.message = str;

        historyDataList.Add(message);
        GameApp.Instance.OnMessagetListChange();
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
        GameApp.Instance.OnMessagetListChange();
    }

    public void AddOneSelectedChoice(JSONNode choices, int selectedIndex)
    {
        ChooseMessageData data = new ChooseMessageData();
        data.choiceNode = choices;

        data.identifier = choices["identifier"];

        data.selectedIndex = selectedIndex;

        var choiceArr = choices["actions"].AsArray;
        data.actions = new ChoiceAction[2];
        data.actions[0] = new ChoiceAction();
        data.actions[0].choice = choiceArr[0]["choice"];
        data.actions[0].identifier = choiceArr[0]["identifier"];

        data.actions[1] = new ChoiceAction();
        data.actions[1].choice = choiceArr[1]["choice"];
        data.actions[1].identifier = choiceArr[1]["identifier"];

        historyDataList.Add(data);
        GameApp.Instance.OnMessagetListChange();
    }
}
