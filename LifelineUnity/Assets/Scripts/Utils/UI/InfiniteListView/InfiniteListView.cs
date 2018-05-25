using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI_2_SuperScroll
{
    [RequireComponent(typeof(ScrollRect))]
    public class InfiniteListView : ListViewBase, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        ScrollRect _scrollRect;
        public ScrollRect scrollRect { get { return _scrollRect; } }
        RectTransform _scrollRectTransform;
        RectTransform _viewportRectTransform;

        [SerializeField]
        float _distanceForRecycleHeadEle = 300;
        [SerializeField]
        float _distanceForNewHeadEle = 200;
        [SerializeField]
        float _distanceForRecycleTailEle = 300;
        [SerializeField]
        float _distanceForNewTailEle = 200;

        Vector3[] _viewportRectLocalCorners = new Vector3[4];
        Vector3[] _elementWorldCorners = new Vector3[4];

        public float ViewportSize
        {
            get
            {
                return _isVertical ? _viewportRectTransform.rect.height : _viewportRectTransform.rect.width;
            }
        }

        public override void InitListView(int itemTotalCount, Func<ListViewBase, int, ListViewElement> getItemByIndex)
        {
            if (_isInited)
                return;
            _isInited = true;

            if (_distanceForRecycleHeadEle <= _distanceForNewHeadEle)
                Debug.LogError("error " + _distanceForRecycleHeadEle + " is smaller than " + _distanceForNewHeadEle);
            if (_distanceForRecycleTailEle <= _distanceForNewTailEle)
                Debug.LogError("error " + _distanceForRecycleTailEle + " is smaller than " + _distanceForNewTailEle);

            _isVertical = (ArrangeType == ListElementArrangeType.TopToBottom) || (ArrangeType == ListElementArrangeType.BottomToTop);
            _scrollRect = gameObject.GetComponent<ScrollRect>();

            _scrollRect.horizontal = !_isVertical;
            _scrollRect.vertical = _isVertical;

            if (_scrollRect.horizontalScrollbar && _scrollRect.horizontalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport)
            {
                Debug.LogError("[LoopListView]Not support AutoHideAndExpandViewport");
                return;
            }
            if (_scrollRect.verticalScrollbar && _scrollRect.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport)
            {
                Debug.LogWarning("[LoopListView]Not support AutoHideAndExpandViewport");
                return;
            }

            _scrollRectTransform = _scrollRect.GetComponent<RectTransform>();
            _contentTrans = _scrollRect.content;
            _viewportRectTransform = _scrollRect.viewport;

            InitElementPool();

            AdjustPivot(_viewportRectTransform);
            AdjustPivot(_contentTrans);
            AdjustAnchor(_contentTrans);

            getElementByIndex = getItemByIndex;

            ResetListView();
            SetListElementCount(itemTotalCount);
        }


        protected override void ResetListView()
        {
            _viewportRectTransform.GetLocalCorners(_viewportRectLocalCorners);
            SetContentPosition(Vector3.zero);
        }

        void CheckAndFitContentPos()
        {
            var count = _activeElementList.Count;
            if (count <= 0)
                return;

            UpdateAllShownElementPos();
            var viewportSize = ViewportSize;
            var contentSize = CalculateTotalElementSize();

            if(_arrangeType == ListElementArrangeType.TopToBottom)
            {
                if(contentSize <= viewportSize)
                {
                    var pos = _contentTrans.localPosition;
                    pos.y = 0;
                    SetContentPosition(pos);
                    _activeElementList[0].SetPosition(new Vector3(_activeElementList[0].StartPosOffset, 0, 0));
                    UpdateAllShownElementPos();
                    return;
                }

                var headEle = _activeElementList[0];
                headEle.CachedRectTransform.GetWorldCorners(_elementWorldCorners);
                var headEleTopPos = _viewportRectTransform.InverseTransformPoint(_elementWorldCorners[1]).y;
                var viewportTopPos = _viewportRectLocalCorners[1].y;
                if(headEleTopPos < viewportTopPos)
                {
                    var pos = _contentTrans.localPosition;
                    pos.y = 0;
                    SetContentPosition(pos);
                    headEle.SetPosition(new Vector3(headEle.StartPosOffset, 0));
                    UpdateAllShownElementPos();
                    return;
                }

                var tailEle = _activeElementList[count - 1];
                tailEle.CachedRectTransform.GetWorldCorners(_elementWorldCorners);
                var tailEleBottomPos = _viewportRectTransform.InverseTransformPoint(_elementWorldCorners[0]).y;
                var viewportBottomPos = _viewportRectLocalCorners[0].y;
                var offset = tailEleBottomPos - viewportBottomPos;
                if(offset > 0)
                {
                    var pos = _contentTrans.localPosition;
                    pos.y = pos.y - offset;
                    SetContentPosition(pos);
                    UpdateAllShownElementPos();
                    return;
                }
            }else if(_arrangeType == ListElementArrangeType.BottomToTop)
            {

            }else if(_arrangeType == ListElementArrangeType.LeftToRight)
            {
                if(contentSize <= viewportSize)
                {
                    var pos = _contentTrans.localPosition;
                    pos.x = 0;
                    SetContentPosition(pos);
                    _activeElementList[0].SetPosition(new Vector3(0, _activeElementList[0].StartPosOffset));
                    UpdateAllShownElementPos();
                    return;
                }

                var headEle = _activeElementList[0];
                headEle.CachedRectTransform.GetWorldCorners(_elementWorldCorners);
                var headEleLeftPos = _viewportRectTransform.InverseTransformPoint(_elementWorldCorners[1]).x;
                var viewportLeftPos = _viewportRectLocalCorners[1].x;
                if(headEleLeftPos > viewportLeftPos)
                {
                    var pos = _contentTrans.localPosition;
                    pos.x = 0;
                    SetContentPosition(pos);
                    _activeElementList[0].SetPosition(new Vector3(0, _activeElementList[0].StartPosOffset));
                    UpdateAllShownElementPos();
                    return;
                }

                var tailEle = _activeElementList[count - 1];
                tailEle.CachedRectTransform.GetWorldCorners(_elementWorldCorners);
                var tailEleRightPos = _viewportRectTransform.InverseTransformPoint(_elementWorldCorners[2]).x;
                var viewportRightPos = _viewportRectLocalCorners[2].x;
                var offset = viewportRightPos - tailEleRightPos;
                if(offset > 0)
                {
                    var pos = _contentTrans.localPosition;
                    pos.x = pos.x + offset;
                    SetContentPosition(pos);
                    UpdateAllShownElementPos();
                    return;
                }


            }else if (_arrangeType == ListElementArrangeType.RightToLeft)
            {

            }
        }

        public override void SetListElementCount(int eleCount, bool resetPos = true)
        {
            /*
            if (_totalElementCount == eleCount)
                return;
                */
            _totalElementCount = eleCount;

            if(_totalElementCount == 0)
            {
                ReleaseAllElementToPool();
                ClearTmpCache();
                UpdateContentSize();
                return;
            }

            if (_activeElementList.Count == 0)
            {
                MovePanelToElementIndex(0, 0);
                return;
            }

            if (resetPos)
            {
                MovePanelToElementIndex(0, 0);
                return;
            }
        }

        public bool IsContentTouchEnd(float allowOffset = 5.0f)
        {
            var count = _activeElementList.Count;
            if (count <= 0)
                return true;

            var tailEle = _activeElementList[count - 1];
            var finalIndex = tailEle.ElementIndex;
            if(finalIndex == _totalElementCount - 1)
            {
                if(_arrangeType == ListElementArrangeType.TopToBottom)
                {
                    tailEle.CachedRectTransform.GetWorldCorners(_elementWorldCorners);
                    var bottomPos = _viewportRectTransform.InverseTransformPoint( _elementWorldCorners[0]).y;
                    var viewportBottomPos = _viewportRectLocalCorners[0].y;
                    if(bottomPos + allowOffset >= viewportBottomPos) // allow little offset
                        return true;
                }

            }

            return false;
        }

        public void MovePanelToElementIndex(int elementIndex, float offset)
        {
            _scrollRect.StopMovement();

            if (elementIndex < 0 || _totalElementCount == 0)
                return;
            if (elementIndex >= _totalElementCount)
                elementIndex = _totalElementCount - 1;

            if (offset < 0)
                offset = 0;

            var firstElePos = Vector2.zero;
            float viewportSize = ViewportSize;
            if (offset > viewportSize)
                offset = viewportSize;

            if (_arrangeType == ListElementArrangeType.TopToBottom)
            {
                var contentPos = _contentTrans.localPosition.y;
                if (contentPos < 0)
                    contentPos = 0;

                firstElePos.y = -contentPos - offset;
            }
            else if (_arrangeType == ListElementArrangeType.BottomToTop)
            {

            }else if(_arrangeType == ListElementArrangeType.LeftToRight)
            {
                var contentPos = _contentTrans.localPosition.x;
                if (contentPos > 0)
                    contentPos = 0;

                firstElePos.x = -contentPos + offset;

            }else if(_arrangeType == ListElementArrangeType.RightToLeft)
            {

            }

            ReleaseAllElementToPool();
            ListViewElement newEle = GetNewElementByIndex(elementIndex);
            if (newEle == null)
            {
                ClearTmpCache();
                return;
            }

            if (_isVertical)
                firstElePos.x = newEle.StartPosOffset;
            else
                firstElePos.y = newEle.StartPosOffset;
            newEle.SetPosition(firstElePos);

            _activeElementList.Add(newEle);
            UpdateContentSize();
            UpdateListView(viewportSize + 100, viewportSize + 100, viewportSize, viewportSize);
            CheckAndFitContentPos();

            ClearTmpCache();
        }

        #region ListViewUpdate
        int _listUpdateCheckFrameCount = 0;
        void UpdateListView(float distanceForRecycleHeadEle, float distanceForRecycleTailEle, float distanceForNewHeadEle, float distanceForNewTailEle)
        {
            _listUpdateCheckFrameCount++;

            if (_isVertical)
            {
                bool needContinueCheck = true;
                int checkCount = 0;
                int maxCount = 9999;
                while (needContinueCheck)
                {
                    checkCount++;
                    if (checkCount >= maxCount)
                    {
                        Debug.LogError("error");
                        break;
                    }

                    needContinueCheck = UpdateForVerticalList(distanceForRecycleHeadEle, distanceForRecycleTailEle, distanceForNewHeadEle, distanceForNewTailEle);
                }
            }
            else
            {
                bool needContinueCheck = true;
                int checkCount = 0;
                int maxCount = 9999;
                while (needContinueCheck)
                {
                    checkCount++;
                    if (checkCount >= maxCount)
                    {
                        Debug.LogError("error");
                        break;
                    }

                    needContinueCheck = UpdateForHorizontalList(distanceForRecycleHeadEle, distanceForRecycleTailEle, distanceForNewHeadEle, distanceForNewTailEle);
                }
            }
        }

        bool UpdateForHorizontalList(float distanceForRecycleHeadEle, float distanceForRecyclTailEle, float distanceForNewHeadEle, float distanceForNewTailEle)
        {
            if (_totalElementCount == 0)
            {
                if (_activeElementList.Count > 0)
                    ReleaseAllElementToPool();
                return false;
            }

            if(_arrangeType == ListElementArrangeType.LeftToRight)
            {
                int activeEleCount = _activeElementList.Count;
                if(activeEleCount == 0)
                {
                    var firstEle = GetNewElementByIndex(0);
                    if (firstEle == null)
                        return false;

                    var contentPos = _contentTrans.localPosition.x;
                    if (contentPos > 0)
                        contentPos = 0;

                    var y = firstEle.StartPosOffset;
                    var x = -contentPos;

                    _activeElementList.Add(firstEle);
                    firstEle.SetPosition(new Vector3(x, y, 0));
                    UpdateContentSize();
                    return true;
                }

                var headEle = _activeElementList[0];
                headEle.CachedRectTransform.GetWorldCorners(_elementWorldCorners);
                var headEleLeftPos = _viewportRectTransform.InverseTransformPoint(_elementWorldCorners[1]).x;
                var headEleRightPos = _viewportRectTransform.InverseTransformPoint(_elementWorldCorners[2]).x;

                var viewportLeftPos = _viewportRectLocalCorners[1].x;
                var viewportRightPos = _viewportRectLocalCorners[2].x;
                if(!_isDraging &&  viewportLeftPos - headEleRightPos > distanceForRecycleHeadEle  )
                {
                    _activeElementList.RemoveAt(0);
                    ReleaseOneElementToTmp(headEle);
                    UpdateContentSize();
                    CheckIfNeedUpdateElementPos();
                    return true;
                }

                var tailEle = _activeElementList[activeEleCount - 1];
                tailEle.CachedRectTransform.GetWorldCorners(_elementWorldCorners);
                var tailEleLeftPos = _viewportRectTransform.InverseTransformPoint(_elementWorldCorners[1]).x;
                var tailEleRightPos = _viewportRectTransform.InverseTransformPoint(_elementWorldCorners[2]).x;

                if(!_isDraging && tailEleLeftPos - viewportRightPos > distanceForRecyclTailEle)
                {
                    _activeElementList.RemoveAt(activeEleCount - 1);
                    ReleaseOneElementToTmp(tailEle);
                    UpdateContentSize();
                    CheckIfNeedUpdateElementPos();
                    return true;
                }

                if(tailEleRightPos - viewportRightPos < distanceForNewTailEle)
                {
                    var nextIdx = tailEle.ElementIndex + 1;
                    var ele = GetNewElementByIndex(nextIdx);
                    if (ele != null)
                    {
                        ele.SetPosition(new Vector3(tailEle.CachedRectTransform.localPosition.x + tailEle.ElementSizeWithPadding, ele.StartPosOffset));

                        _activeElementList.Add(ele);
                        UpdateContentSize();
                        CheckIfNeedUpdateElementPos();
                        return true;
                    }
                }

                if(viewportLeftPos - headEleLeftPos < distanceForNewHeadEle)
                {
                    var beforeIdx = headEle.ElementIndex - 1;
                    var ele = GetNewElementByIndex(beforeIdx);
                    if (ele != null)
                    {
                        ele.SetPosition(new Vector3(headEle.CachedRectTransform.localPosition.x - ele.ElementSizeWithPadding, ele.StartPosOffset));

                        _activeElementList.Insert(0, ele);
                        UpdateContentSize();
                        CheckIfNeedUpdateElementPos();
                        return true;
                    }
                }

            }else if(_arrangeType == ListElementArrangeType.RightToLeft)
            {

            }

            return false;
        }

        bool UpdateForVerticalList(float distanceForRecycleHeadEle, float distanceForRecycleTailEle, float distanceForNewHeadEle, float distanceForNewTailEle)
        {
            if (_totalElementCount == 0)
            {
                if (_activeElementList.Count > 0)
                    ReleaseAllElementToPool();
                return false;
            }

            if (_arrangeType == ListElementArrangeType.TopToBottom)
            {
                int eleListCount = _activeElementList.Count;
                if (eleListCount == 0)
                {
                    var firstEle = GetNewElementByIndex(0);
                    if (firstEle == null)
                        return false;

                    var contentPos = _contentTrans.localPosition.y;
                    if (contentPos < 0)
                        contentPos = 0;

                    var x = firstEle.StartPosOffset;
                    var y = -contentPos;
                    firstEle.SetPosition(new Vector3(x, y));
                    _activeElementList.Add(firstEle);

                    UpdateContentSize();

                    return true;
                }

                var viewportTopPos = _viewportRectLocalCorners[1].y;
                var viewportBottomPos = _viewportRectLocalCorners[0].y;

                var headElement = _activeElementList[0];
                headElement.CachedRectTransform.GetWorldCorners(_elementWorldCorners);
                float headElementTopPos = _viewportRectTransform.InverseTransformPoint(_elementWorldCorners[1]).y;
                float headElementBottomPos = _viewportRectTransform.InverseTransformPoint(_elementWorldCorners[0]).y;

                if (!_isDraging && headElementBottomPos - viewportTopPos > distanceForRecycleHeadEle)
                {
                    ReleaseOneElementToTmp(headElement);
                    _activeElementList.RemoveAt(0);
                    UpdateContentSize();
                    CheckIfNeedUpdateElementPos();

                    return true;
                }

                var tailElement = _activeElementList[_activeElementList.Count - 1];
                tailElement.CachedRectTransform.GetWorldCorners(_elementWorldCorners);
                float tailElementTopPos = _viewportRectTransform.InverseTransformPoint(_elementWorldCorners[1]).y;
                float tailElementBottomPos = _viewportRectTransform.InverseTransformPoint(_elementWorldCorners[0]).y;

                if (!_isDraging && viewportBottomPos - tailElementTopPos > distanceForRecycleTailEle)
                {
                    ReleaseOneElementToTmp(tailElement);
                    _activeElementList.RemoveAt(_activeElementList.Count - 1);
                    UpdateContentSize();
                    CheckIfNeedUpdateElementPos();

                    return true;
                }

                if (viewportBottomPos - tailElementBottomPos < distanceForNewTailEle)
                {
                    var tailIndex = tailElement.ElementIndex;
                    var nextIndex = tailIndex + 1;
                    var nextEle = GetNewElementByIndex(nextIndex);
                    if (nextEle != null)
                    {
                        var y = tailElement.CachedRectTransform.localPosition.y - tailElement.ElementSizeWithPadding;
                        nextEle.SetPosition(new Vector3(nextEle.StartPosOffset, y));
                        _activeElementList.Add(nextEle);
                        UpdateContentSize();
                        CheckIfNeedUpdateElementPos();
                        return true;
                    }
                }

                if (headElementTopPos - viewportTopPos < distanceForNewHeadEle)
                {
                    var headIndex = headElement.ElementIndex;
                    var preIndex = headIndex - 1;
                    var preEle = GetNewElementByIndex(preIndex);
                    if (preEle != null)
                    {
                        var y = headElement.CachedRectTransform.localPosition.y + preEle.ElementSizeWithPadding;
                        preEle.SetPosition(new Vector3(preEle.StartPosOffset, y));
                        _activeElementList.Insert(0, preEle);
                        UpdateContentSize();
                        CheckIfNeedUpdateElementPos();
                        return true;
                    }
                }
            }else if(_arrangeType == ListElementArrangeType.BottomToTop)
            {

            }

            return false;
        }

        #endregion ListViewUpdate

        void CheckIfNeedUpdateElementPos()
        {
            var elementCount = _activeElementList.Count;
            if (elementCount == 0)
                return;

            if (_arrangeType == ListElementArrangeType.TopToBottom)
            {
                var headEle = _activeElementList[0];
                var tailEle = _activeElementList[_activeElementList.Count - 1];
                var contentSize = CalculateTotalElementSize();

                if (headEle.TopY > 0 || (headEle.ElementIndex == 0 && headEle.TopY != 0))
                {
                    UpdateAllShownElementPos();
                    return;
                }

                if (tailEle.BottomY < -contentSize || (tailEle.ElementIndex == _totalElementCount - 1 && tailEle.BottomY != -contentSize))
                {
                    UpdateAllShownElementPos();
                    return;
                }
            }
            else if (_arrangeType == ListElementArrangeType.BottomToTop)
            {

            }
            else if (_arrangeType == ListElementArrangeType.LeftToRight)
            {
                var headEle = _activeElementList[0];
                var tailEle = _activeElementList[_activeElementList.Count - 1];
                var contentSize = CalculateTotalElementSize();

                if (headEle.LeftX < 0 || (headEle.ElementIndex == 0 && headEle.LeftX != 0))
                {
                    UpdateAllShownElementPos();
                    return;
                }

                if (tailEle.RightX > contentSize || (tailEle.ElementIndex == _totalElementCount - 1 && tailEle.RightX != contentSize))
                {
                    UpdateAllShownElementPos();
                    return;
                }

            }
            else if (_arrangeType == ListElementArrangeType.RightToLeft)
            {

            }
        }

        void SetContentPosition(Vector3 localPos)
        {
            _contentTrans.localPosition = localPos;

            // unity 2017.4f1 bug , need read anchoredPos first before modify position
            // https://issuetracker.unity3d.com/issues/setting-the-size-of-a-rect-transform-resets-all-children-if-their-anchored-position-has-not-been-read-first
            _contentTrans.anchoredPosition = _contentTrans.anchoredPosition;
        }

        PointerEventData _pointerEventData = null;

        Vector3 _lastFrameContentPos;
        void UpdateAllShownElementPos()
        {
            var elementCount = _activeElementList.Count;
            if (elementCount == 0)
                return;

            var oldVelocity = (_contentTrans.localPosition - _lastFrameContentPos) / Time.deltaTime;

            if (_arrangeType == ListElementArrangeType.TopToBottom)
            {
                float pos = 0;
                float headElePos = _activeElementList[0].CachedRectTransform.localPosition.y;
                float offset = pos - headElePos;
                float curY = pos;
                for (int i = 0; i < elementCount; i++)
                {
                    var ele = _activeElementList[i];
                    ele.SetPosition(new Vector3(ele.StartPosOffset, curY));
                    curY = curY - ele.ElementSizeWithPadding;
                }

                if (offset != 0)
                {
                    var p = _contentTrans.localPosition;
                    p.y = p.y - offset;
                    SetContentPosition(p);
                }
            }else if(_arrangeType == ListElementArrangeType.BottomToTop)
            {

            }else if(_arrangeType == ListElementArrangeType.LeftToRight)
            {
                float pos = 0;
                float headElePos = _activeElementList[0].CachedRectTransform.localPosition.x;
                float offset = pos - headElePos;
                float curX = pos;
                for(int i = 0; i < elementCount; i++)
                {
                    var ele = _activeElementList[i];
                    ele.SetPosition(new Vector3(curX, ele.StartPosOffset));
                    curX = curX + ele.ElementSizeWithPadding;
                }

                if(offset != 0)
                {
                    var p = _contentTrans.localPosition;
                    p.x = p.x - offset;
                    SetContentPosition(p);
                }

            }else if(_arrangeType == ListElementArrangeType.RightToLeft)
            {

            }

            if (_isDraging)
            {
                _scrollRect.OnBeginDrag(_pointerEventData);
                _scrollRect.Rebuild(CanvasUpdate.PostLayout);
                _scrollRect.velocity = oldVelocity;
            }
        }

        #region Draging
        bool _isDraging = false;
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            _isDraging = true;
            _pointerEventData = eventData;

        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            _isDraging = false;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {

        }
        #endregion Draging

        private void Update()
        {
            if (!_isInited)
                return;

            UpdateListView(_distanceForRecycleHeadEle, _distanceForRecycleTailEle, _distanceForNewHeadEle, _distanceForNewTailEle);
            ClearTmpCache();
            _lastFrameContentPos = _contentTrans.localPosition;
        }

        public override void OnPrefabSizeChange(int elementIndex)
        {
            var shownElement = GetShownElementByIndex(elementIndex);
            if (shownElement == null)
                return;

            UpdateContentSize();
            UpdateAllShownElementPos();
        }
    }
}
