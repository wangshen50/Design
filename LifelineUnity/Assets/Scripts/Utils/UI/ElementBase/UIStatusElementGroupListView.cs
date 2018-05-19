using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI_2_SuperScroll
{
    [RequireComponent(typeof(ListViewBase))]
    public class UIStatusElementGroupListView : UIStatusElementGroup
    {
        ListViewBase _listView;
        public ListViewBase ListView
        {
            get
            {
                if(_listView == null)
                    _listView = GetComponent<ListViewBase>();
                return _listView;
            }
        }

        public override UIInGroupElement GetShownElementByIndex(int dataIndex)
        {
            var listViewEle = ListView.ActiveElementList;
            foreach(var lve in listViewEle)
            {
                foreach(var ele in lve.InGroupElementList)
                {
                    if (ele.DataIndex == dataIndex)
                        return ele;
                }
            }
            return null;
        }
    }
}
