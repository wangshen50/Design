﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI_2_SuperScroll;

public class ChooseUIElement : UIInGroupElement<ChooseMessageData> {
    public Text choice1Text;
    public Button choice1Btn;

    public Text choice2Text;
    public Button choice2Btn;

    public string selectedPrefix;

    protected override void OnDataChange()
    {
        base.OnDataChange();

        var data = Data;

        choice1Text.text = data.actions[0].choice;
        choice2Text.text = data.actions[1].choice;

        if(data.selectedIndex == -1) // unselected 
        {
            choice1Btn.onClick.AddListener(()=>OnSelect(0));
            choice2Btn.onClick.AddListener(()=>OnSelect(1));

        }else if(data.selectedIndex == 0)
        {
            choice1Text.text = selectedPrefix + data.actions[0].choice;
            choice1Btn.onClick.RemoveAllListeners();
            choice2Btn.onClick.RemoveAllListeners();

        }else if(data.selectedIndex == 1)
        {
            choice2Text.text = selectedPrefix + data.actions[1].choice;
            choice1Btn.onClick.RemoveAllListeners();
            choice2Btn.onClick.RemoveAllListeners();
        }
    }

    private void OnSelect(int selectedIndex)
    {
        var action = Data.actions[selectedIndex];

        Data.selectedIndex = selectedIndex;
        OnDataChange();

        var cm = GameApp.Instance;
        cm.status["atScene"] = action.identifier;
        cm.SaveStatusData(action.identifier);

        ParseText.AtScene(GameApp.Instance, GameApp.Instance.status["atScene"]);     // set: status["atScene"] = null
        cm.mainUI.ShowNextMessage();
    }
}