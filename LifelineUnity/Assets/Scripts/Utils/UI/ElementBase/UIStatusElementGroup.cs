using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI_2_SuperScroll
{
    public abstract class UIStatusElementGroup : MonoBehaviour
    {
        protected Dictionary<string, int> _status2ElementIndexDict = new Dictionary<string, int>();

        public int GetCurrentElementIndexWithStatus(string statusTag)
        {
            int idx;
            if (_status2ElementIndexDict.TryGetValue(statusTag, out idx))
                return idx;
            else
                return -1;
        }

        public void ClearStatus()
        {
            _status2ElementIndexDict.Clear();
        }

        public abstract UIInGroupElement GetShownElementByIndex(int dataIndex);

        public bool GetStatusActive(string statusTag, int index)
        {
            int idx;
            if(_status2ElementIndexDict.TryGetValue(statusTag, out idx))
                return index == idx;
            return false;
        }

        public void ChangeExclusiveStatus(string statusTag, bool active, int dataIndex, bool forceRefresh)
        {
            int oldIndex;
            if (_status2ElementIndexDict.TryGetValue(statusTag, out oldIndex))
            {
                if (!active && oldIndex == dataIndex)
                    _status2ElementIndexDict[statusTag] = -1;

                if(active && oldIndex != dataIndex)
                {
                    var ele = GetShownElementByIndex(oldIndex);
                    if(ele != null)
                        ele.ChangeOneStatus(statusTag, false, false, forceRefresh);

                    _status2ElementIndexDict[statusTag] = dataIndex;
                }

            }else
            {
                if (active)
                    _status2ElementIndexDict.Add(statusTag, dataIndex);
            }
        }
    }
}
