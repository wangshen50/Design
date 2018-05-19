using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI_2_SuperScroll
{
    public class ListViewElementPool
    {
        GameObject _prefabObj;
        string _prefabName;
        RectTransform _elementContainer;
        float _padding;
        float _startPosOffset;

        Stack<ListViewElement> _tmpElementCache = new Stack<ListViewElement>();
        Stack<ListViewElement> _elementCache = new Stack<ListViewElement>();

        public void Init(GameObject prefabObj, float padding, float startPosOffset, int createCount, RectTransform parent)
        {
            _prefabObj = prefabObj;
            _prefabName = prefabObj.name;
            _elementContainer = parent;
            _padding = padding;
            _startPosOffset = startPosOffset;
            _prefabObj.SetActive(false);

            for (int i = 0; i < createCount; i++)
            {
                var ele = CreateElement();
                ReleaseOneElementToCache(ele);
            }
        }

        ListViewElement CreateElement()
        {
            var go = GameObject.Instantiate(_prefabObj, _elementContainer);
            go.SetActive(true);

            var element = go.GetComponent<ListViewElement>();
            element.Padding = _padding;
            element.StartPosOffset = _startPosOffset;
            element.ItemPrefabName = _prefabName;

            return element;
        }

        public void ReleaseOneElementToTmp(ListViewElement ele)
        {
            _tmpElementCache.Push(ele);
        }

        public void ClearTmpElementCache()
        {
            foreach (var ele in _tmpElementCache)
                ReleaseOneElementToCache(ele);
            _tmpElementCache.Clear();
        }

        void ReleaseOneElementToCache(ListViewElement ele)
        {
            ele.gameObject.SetActive(false);
            _elementCache.Push(ele);
        }

        public ListViewElement GetElement()
        {
            ListViewElement ele;
            if (_tmpElementCache.Count > 0)
            {
                ele = _tmpElementCache.Pop();
                ele.gameObject.SetActive(true);
            }
            else
            {
                if (_elementCache.Count > 0)
                {
                    ele = _elementCache.Pop();
                    ele.gameObject.SetActive(true);
                }
                else
                {
                    ele = CreateElement();
                }
            }
            ele.Padding = _padding;
            return ele;
        }

        public void DestroyAllElement()
        {
            ClearTmpElementCache();
            foreach(var e in _elementCache)
                GameObject.Destroy(e);
            _elementCache.Clear();
        }
    }
}
