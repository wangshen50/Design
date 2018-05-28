using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI_2_SuperScroll;

public class ChooseUIElement : UIInGroupElement<ChooseMessageData> {
    public Text choice1Text;
    public Button choice1Btn;
    public GameObject selectedText1;

    public Text choice2Text;
    public Button choice2Btn;
    public GameObject selectedText2;

    public string selectedPrefix;

    Color defaultChoice1TextColor = Color.white;
    Color defaultChoice1BtnColor = Color.white;
    Color defaultChoice2TextColor = Color.white;
    Color defaultChoice2BtnColor = Color.white;

    private void Awake()
    {
        selectedText1.SetActive(false);
        selectedText2.SetActive(false);

        defaultChoice1TextColor = choice1Text.color;
        defaultChoice1BtnColor = choice1Btn.image.color;
        defaultChoice2TextColor = choice2Text.color;
        defaultChoice2BtnColor = choice2Btn.image.color;
    }

    protected override void OnDataChange()
    {
        base.OnDataChange();

        var data = Data;

        choice1Text.text = data.actions[0].choice;
        choice2Text.text = data.actions[1].choice;

        choice1Btn.image.color = defaultChoice1BtnColor;
        choice1Text.color = defaultChoice1TextColor;
        choice2Btn.image.color = defaultChoice2BtnColor;
        choice2Text.color = defaultChoice2TextColor;

        if (data.selectedIndex == -1) // unselected 
        {
            choice1Btn.onClick.AddListener(()=>OnSelect(0));
            choice2Btn.onClick.AddListener(()=>OnSelect(1));
        }

        if(GameApp.Instance.status[data.actions[0].identifier].AsBool)
        {
            choice1Btn.image.color = GameApp.Instance.hasChoosedColor;
            choice1Text.color = GameApp.Instance.hasChoosedColor;

            //selectedText1.SetActive(true);
            choice1Btn.onClick.RemoveAllListeners();
        }else
        {
            //selectedText1.SetActive(false);
        }

        if(GameApp.Instance.status[data.actions[1].identifier].AsBool)
        {
            choice2Btn.image.color = GameApp.Instance.hasChoosedColor;
            choice2Text.color = GameApp.Instance.hasChoosedColor;
            //selectedText2.SetActive(true);
            choice2Btn.onClick.RemoveAllListeners();
        }else
        {
            //selectedText2.SetActive(false);
        }

        if (data.selectedIndex == 0)
        {
            choice1Btn.image.color = GameApp.Instance.selectedColor;
            choice1Text.color = GameApp.Instance.selectedColor;

            //choice1Text.text = selectedPrefix + data.actions[0].choice;
            choice1Btn.onClick.RemoveAllListeners();
            choice2Btn.onClick.RemoveAllListeners();

        }else if(data.selectedIndex == 1)
        {
            choice2Btn.image.color = GameApp.Instance.selectedColor;
            choice2Text.color = GameApp.Instance.selectedColor;

            //choice2Text.text = selectedPrefix + data.actions[1].choice;
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
