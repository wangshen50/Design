using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UI_2_SuperScroll;

public class MessageUIElement : UIInGroupElement<NormalMessageData> {
    public Text messageText;

    protected override void OnDataChange()
    {
        base.OnDataChange();

        messageText.text = Data.message;
    }
}
