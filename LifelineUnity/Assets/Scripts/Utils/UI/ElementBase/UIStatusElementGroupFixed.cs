using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI_2_SuperScroll
{
    public class UIStatusElementGroupFixed : UIStatusElementGroup
    {
        UIInGroupElement[] _uiInGroupElementList;
        

        private void Awake()
        {
            _uiInGroupElementList = GetComponentsInChildren<UIInGroupElement>();
            for(int i = 0; i < _uiInGroupElementList.Length; i++)
            {
                var ele = _uiInGroupElementList[i];
                ele.Init();
                ele.DataIndex = i;
            }
        }

        protected UIInGroupElement[] GetElementList()
        {
            return _uiInGroupElementList;
        }

        public override UIInGroupElement GetShownElementByIndex(int dataIndex)
        {
            if (dataIndex < 0 || dataIndex >= _uiInGroupElementList.Length)
                return null;
            return _uiInGroupElementList[dataIndex];
        }
    }
}
