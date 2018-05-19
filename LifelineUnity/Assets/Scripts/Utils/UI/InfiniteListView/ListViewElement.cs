using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI_2_SuperScroll
{

    public class ListViewElement : MonoBehaviour
    {
        List<UIInGroupElement> _inGroupElementList;
        public List<UIInGroupElement> InGroupElementList
        {
            get
            {
                if (_inGroupElementList == null)
                {
                    _inGroupElementList = new List<UIInGroupElement>();
                    var list = new List<UIInGroupElement>(GetComponentsInChildren<UIInGroupElement>(true));
                    foreach(var ele in list)
                    {
                        // filter the ele that is not belong to this group
                        if(ele.transform.parent == transform || ele.transform == transform)
                        {
                            _inGroupElementList.Add(ele);
                            ele.Init();
                        }
                    }
                }
                return _inGroupElementList;
            }
        }
 
        string _itemPrefabName;
        public string ItemPrefabName
        {
            get { return _itemPrefabName; }
            set { _itemPrefabName = value; }
        }

        float _startPosOffset;
        public float StartPosOffset
        {
            get { return _startPosOffset; }
            set { _startPosOffset = value; }
        }

        RectTransform _rectTransform;
        public RectTransform CachedRectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        int _elementIndex;
        public int ElementIndex
        {
            get { return _elementIndex; }
            set { _elementIndex = value; }
        }

        float _padding;
        public float Padding
        {
            get { return _padding; }
            set { _padding = value; }
        }

        ListViewBase _parentListView;
        public ListViewBase ParentListView
        {
            get { return _parentListView; }
            set { _parentListView = value; }
        }

        public virtual float ElementSize
        {
            get
            {
                if (ParentListView.IsVertical)
                    return CachedRectTransform.rect.height;
                else
                    return CachedRectTransform.rect.width;
            }
        }

        public float ElementSizeWithPadding
        {
            get { return ElementSize + _padding; }
        }


        public float TopY
        {
            get
            {
                var arrangeType = _parentListView.ArrangeType;
                if (arrangeType == ListElementArrangeType.TopToBottom)
                    return _rectTransform.localPosition.y;
                else if (arrangeType == ListElementArrangeType.BottomToTop)
                    return _rectTransform.localPosition.y + _rectTransform.rect.height;
                else
                    return 0;
            }
        }

        public float BottomY
        {
            get
            {
                var arrangeType = _parentListView.ArrangeType;
                if (arrangeType == ListElementArrangeType.TopToBottom)
                    return _rectTransform.localPosition.y - _rectTransform.rect.height;
                else if (arrangeType == ListElementArrangeType.BottomToTop)
                    return _rectTransform.localPosition.y;
                else
                    return 0;
            }
        }

        public float LeftX
        {
            get
            {
                var arrangeType = _parentListView.ArrangeType;
                if (arrangeType == ListElementArrangeType.LeftToRight)
                    return _rectTransform.localPosition.x;
                else if (arrangeType == ListElementArrangeType.RightToLeft)
                    return _rectTransform.localPosition.x - _rectTransform.rect.width;

                return 0;
            }
        }

        public float RightX
        {
            get
            {
                var arrangeType = _parentListView.ArrangeType;
                if (arrangeType == ListElementArrangeType.LeftToRight)
                    return _rectTransform.localPosition.x + _rectTransform.rect.width;
                else if (arrangeType == ListElementArrangeType.RightToLeft)
                    return _rectTransform.localPosition.x;

                return 0;
            }
        }

        public void SetPosition(Vector3 localPos)
        {
            CachedRectTransform.localPosition = localPos;


            // unity 2017.4f1 bug , need read anchoredPos first before modify position
            // https://issuetracker.unity3d.com/issues/setting-the-size-of-a-rect-transform-resets-all-children-if-their-anchored-position-has-not-been-read-first
            CachedRectTransform.anchoredPosition = CachedRectTransform.anchoredPosition;
        }

        public virtual void SetVisible(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}
