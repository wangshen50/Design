using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI_2_SuperScroll
{
    [Serializable]
    public class ElementPrefabConfig
    {
        public GameObject elementPrefab;
        public float padding;
        public int initCreateCount;
        public float startPosOffset;
    }

    public enum ListElementArrangeType
    {
        TopToBottom,
        BottomToTop,
        LeftToRight,
        RightToLeft,
    }

    public class ListViewBase : MonoBehaviour
    {
        #region SerializableField
        [SerializeField]
        protected ListElementArrangeType _arrangeType = ListElementArrangeType.TopToBottom;
        public ListElementArrangeType ArrangeType
        {
            get { return _arrangeType; }
            set { _arrangeType = value; }
        }

        public ElementPrefabConfig[] elementPrefabConfigs;
        public RectTransform content;

        #endregion SerializableField

        protected List<ListViewElement> _activeElementList = new List<ListViewElement>();
        public List<ListViewElement> ActiveElementList { get { return _activeElementList; } }

        protected Func<ListViewBase, int, ListViewElement> getElementByIndex;

        protected RectTransform _contentTrans;

        protected int _totalElementCount = 0;
        public int TotalElementCount
        {
            get { return _totalElementCount; }
        }

        protected bool _isVertical;
        public bool IsVertical
        {
            get { return _isVertical; }
        }

        protected bool _isInited = false;
        public virtual void InitListView(int itemCount, Func<ListViewBase, int, ListViewElement> getElement)
        {
            if (_isInited)
                return;
            _isInited = true;

            _isVertical = (ArrangeType == ListElementArrangeType.TopToBottom) || (ArrangeType == ListElementArrangeType.BottomToTop);

            if (content == null)
                _contentTrans = GetComponent<RectTransform>();
            else
                _contentTrans = content;

            InitElementPool();

            AdjustPivot(_contentTrans);

            getElementByIndex = getElement;

            SetListElementCount(itemCount);
        }


        /// <summary>
        /// use to create element from other class
        /// </summary>
        /// <returns></returns>
        public virtual ListViewElement NewListViewElement(string prefabName, int index)
        {
            if (string.IsNullOrEmpty(prefabName))
                return null;

            ListViewElementPool pool;
            if (_elementPoolDict.TryGetValue(prefabName, out pool))
            {
                var ele = pool.GetElement();
                var rt = ele.CachedRectTransform;
                rt.SetParent(_contentTrans);
                rt.localScale = Vector3.one;
                //rt.anchoredPosition = Vector2.zero;
                //rt.localPosition = Vector3.zero;
                rt.localEulerAngles = Vector3.zero;

                ele.ParentListView = this;
                ele.ElementIndex = index;
                return ele;
            }
            return null;
        }

        #region ElementPool
        protected Dictionary<string, ListViewElementPool> _elementPoolDict = new Dictionary<string, ListViewElementPool>();

        protected void InitElementPool()
        {
            _elementPoolDict.Clear();

            foreach (var ele in elementPrefabConfigs)
            {
                var prefab = ele.elementPrefab;
                if (prefab == null)
                {
                    Debug.LogError("prefab should not be null");
                    continue;
                }

                var prefabName = prefab.name;
                if (_elementPoolDict.ContainsKey(prefabName))
                {
                    Debug.LogError("duplicate prefab name ");
                    continue;
                }

                RectTransform rf = prefab.GetComponent<RectTransform>();
                AdjustAnchor(rf);
                AdjustPivot(rf);

                var loopEle = prefab.GetComponent<ListViewElement>();
                if (loopEle == null)
                    loopEle = prefab.AddComponent<ListViewElement>();

                var elementPool = new ListViewElementPool();
                elementPool.Init(prefab, ele.padding, ele.startPosOffset, ele.initCreateCount, _contentTrans);
                _elementPoolDict.Add(prefabName, elementPool);
            }
        }
        #endregion ElementPool

        protected void AdjustPivot(RectTransform rectTrans)
        {
            Vector2 pivot = rectTrans.pivot;
            if (ArrangeType == ListElementArrangeType.TopToBottom)
                pivot.y = 1;
            else if (ArrangeType == ListElementArrangeType.BottomToTop)
                pivot.y = 0;
            else if (ArrangeType == ListElementArrangeType.LeftToRight)
                pivot.x = 0;
            else if (ArrangeType == ListElementArrangeType.RightToLeft)
                pivot.x = 1;
            rectTrans.pivot = pivot;
        }

        protected void AdjustAnchor(RectTransform rectTrans)
        {
            Vector2 anchorMin = rectTrans.anchorMin;
            Vector2 anchorMax = rectTrans.anchorMax;
            if (ArrangeType == ListElementArrangeType.TopToBottom)
            {
                anchorMin.y = 1;
                anchorMax.y = 1;
            }
            else if (ArrangeType == ListElementArrangeType.BottomToTop)
            {
                anchorMin.y = 0;
                anchorMax.y = 0;
            }
            else if (ArrangeType == ListElementArrangeType.LeftToRight)
            {
                anchorMin.x = 0;
                anchorMax.x = 0;
            }
            else if (ArrangeType == ListElementArrangeType.RightToLeft)
            {
                anchorMin.x = 1;
                anchorMax.x = 1;
            }
            rectTrans.anchorMin = anchorMin;
            rectTrans.anchorMax = anchorMax;
        }

        protected virtual void ResetListView()
        {
            _contentTrans.localPosition = Vector3.zero;
        }

        protected float CalculateTotalElementSize()
        {
            int count = _activeElementList.Count;
            if (count == 0)
                return 0;
            float result = 0;
            for (int i = 0; i < count - 1; i++)
                result += _activeElementList[i].ElementSizeWithPadding;
            result += _activeElementList[count - 1].ElementSize;

            return result;
        }

        protected void UpdateContentSize()
        {
            var totalEleSize = CalculateTotalElementSize();
            _contentTrans.SetSizeWithCurrentAnchors(_isVertical ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal, totalEleSize);
        }

        public virtual void SetListElementCount(int eleCount, bool resetPos = true)
        {
            /*
            if (_totalElementCount == eleCount)
                return;
                */
            _totalElementCount = eleCount;

            if (_totalElementCount == 0)
            {
                ReleaseAllElementToPool();
                ClearTmpCache();
                UpdateContentSize();
                return;
            }

            ReleaseAllElementToPool();

            if(_arrangeType == ListElementArrangeType.TopToBottom)
            {
                float curPos = 0;
                for(int i = 0; i < _totalElementCount; i++)
                {
                    var ele = GetNewElementByIndex(i);
                    if (ele == null)
                        return;

                    _activeElementList.Add(ele);

                    var pos = ele.CachedRectTransform.localPosition;
                    pos.y = curPos;
                    ele.SetPosition(pos);
                    curPos = curPos - ele.ElementSizeWithPadding;

                    UpdateContentSize();
                    ClearTmpCache();
                }

            }else if(_arrangeType == ListElementArrangeType.BottomToTop)
            {
                float curPos = 0;
                for(int i = 0; i < _totalElementCount; i++)
                {
                    var ele = GetNewElementByIndex(i);
                    if (ele == null)
                        return;

                    _activeElementList.Add(ele);

                    var pos = ele.CachedRectTransform.localPosition;
                    pos.y = curPos;
                    ele.SetPosition(pos);
                    curPos = curPos + ele.ElementSizeWithPadding;

                    UpdateContentSize();
                    ClearTmpCache();
                }

            }else if(_arrangeType == ListElementArrangeType.LeftToRight)
            {
                float curPos = 0;
                for(int i = 0; i < _totalElementCount; i++)
                {
                    var ele = GetNewElementByIndex(i);
                    if (ele == null)
                        return;

                    _activeElementList.Add(ele);

                    var pos = ele.CachedRectTransform.localPosition;
                    pos.x = curPos;
                    pos.y = ele.StartPosOffset;
                    ele.SetPosition(pos);
                    curPos = curPos + ele.ElementSizeWithPadding;

                    UpdateContentSize();
                    ClearTmpCache();
                }

            }else if(_arrangeType == ListElementArrangeType.RightToLeft)
            {
                float curPos = 0;
                for(int i = 0; i < _totalElementCount; i++)
                {
                    var ele = GetNewElementByIndex(i);
                    if (ele == null)
                        return;

                    _activeElementList.Add(ele);

                    var pos = ele.CachedRectTransform.localPosition;
                    pos.x = curPos;
                    pos.y = ele.StartPosOffset;
                    ele.SetPosition(pos);
                    curPos = curPos - ele.ElementSizeWithPadding;

                    UpdateContentSize();
                    ClearTmpCache();
                }
            }
        }

        #region ElementPool

        protected ListViewElement GetNewElementByIndex(int elementIndex)
        {
            if (elementIndex < 0 || elementIndex >= _totalElementCount)
                return null;

            var ele = getElementByIndex(this, elementIndex);
            return ele;
        }

        protected void ReleaseOneElementToTmp(ListViewElement ele)
        {
            if (ele == null)
                return;

            var prefabName = ele.ItemPrefabName;
            if (string.IsNullOrEmpty(prefabName))
                return;

            ListViewElementPool pool;
            if (_elementPoolDict.TryGetValue(prefabName, out pool))
                pool.ReleaseOneElementToTmp(ele);
        }

        protected void ReleaseAllElementToPool()
        {
            foreach (var ele in _activeElementList)
                ReleaseOneElementToTmp(ele);
            _activeElementList.Clear();
        }

        protected void ClearTmpCache()
        {
            foreach (var pair in _elementPoolDict)
            {
                var pool = pair.Value;
                pool.ClearTmpElementCache();
            }
        }

        #endregion ElementPool

        public ListViewElement GetShownElementByIndex(int index)
        {
            var count = _activeElementList.Count;
            if (count <= 0)
                return null;

            if (index < _activeElementList[0].ElementIndex || index > _activeElementList[count - 1].ElementIndex)
                return null;

            var ind = index - _activeElementList[0].ElementIndex;
            return _activeElementList[ind];
        }

        public virtual void OnPrefabSizeChange(int elementIndex)
        {
            var shownElement = GetShownElementByIndex(elementIndex);
            if (shownElement == null)
                return;

            UpdateContentSize();
            //UpdateAllShownElementPos();
        }
    }
}
