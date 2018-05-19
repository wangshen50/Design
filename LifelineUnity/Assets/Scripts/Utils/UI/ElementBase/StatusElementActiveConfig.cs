using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;


namespace UI_2_SuperScroll
{
    [Serializable]
    public sealed class StatusElementActiveConfig
    {
        #region SubObject
        public interface StatusActiveObjectBaseClass
        {
            void SetObjectStatus(bool active);
        }

        #region SelectableObj Config
        [Serializable]
        public class StatusActiveSelectable : StatusActiveObjectBaseClass
        {
            public Selectable selectableObj;
            public bool activeInterable;

            public void SetObjectStatus(bool active)
            {
                if (active)
                    selectableObj.interactable = activeInterable;
                else
                    selectableObj.interactable = !activeInterable;
            }
        }
        #endregion SelectableObj Config

        #region Graphic Config
        [Serializable]
        public class GraphicColorConfig
        {
            public Color activeColor = Color.white;
            public Color unactiveColor = Color.white;
            public bool useTweening;
            [FieldVisibleInEditor("useTweening", true)]
            public float changeDuration;
            [FieldVisibleInEditor("useTweening", true)]
            public Ease easeType = Ease.Linear;

            public void SetColorGraphic(Graphic graphic, bool active)
            {
                if (useTweening)
                {
                    Tweener tweener;
                    if (active)
                        tweener = graphic.DOColor(activeColor, changeDuration);
                    else
                        tweener = graphic.DOColor(unactiveColor, changeDuration);
                    tweener.SetEase(easeType);
                }
                else
                {
                    graphic.color = active ? activeColor : unactiveColor;
                }
            }
        }

        [Serializable]
        public class StatusActiveGraphic : StatusActiveObjectBaseClass
        {
            public Graphic graphic;
            public bool useColor;
            [FieldVisibleInEditor("useColor", true)]
            public GraphicColorConfig colorConfig;

            public void SetObjectStatus(bool active)
            {
                if (useColor)
                    colorConfig.SetColorGraphic(graphic, active);
            }
        }
        #endregion Graphic Config

        #region RectTransfrom Config
        [Serializable]
        public class TransformPosConfig
        {
            public Vector2 activePosition;
            public Vector2 unactivePosition;
            public bool useTweening;
            [FieldVisibleInEditor("useTweening", true)]
            public float changeDuration;
            [FieldVisibleInEditor("useTweening", true)]
            public Ease easeType;

            public void SetPosTrans(RectTransform trans, bool active)
            {
                if (useTweening)
                {
                    Tweener tweener;
                    if (active)
                        tweener = trans.DOAnchorPos(activePosition, changeDuration);
                    else
                        tweener = trans.DOAnchorPos(unactivePosition, changeDuration);
                    tweener.SetEase(easeType);
                }
                else
                {
                    trans.anchoredPosition = active ? activePosition : unactivePosition;
                }
            }
        }

        [Serializable]
        public class TransformScaleConfig
        {
            public Vector3 activeScale;
            public Vector3 unactiveScale;
            public bool useTweening;
            [FieldVisibleInEditor("useTweening", true)]
            public float changeDuration;
            [FieldVisibleInEditor("useTweening", true)]
            public Ease easeType;

            public void SetScaleTrans(RectTransform trans, bool active)
            {
                if (useTweening)
                {
                    Tweener tweener;
                    if (active)
                        tweener = trans.DOScale(activeScale, changeDuration);
                    else
                        tweener = trans.DOScale(unactiveScale, changeDuration);
                    tweener.SetEase(easeType);
                }
                else
                {
                    trans.localScale = active ? activeScale : unactiveScale;
                }
            }
        }

        [Serializable]
        public class TransformSizeConfig
        {
            public Vector2 activeSize;
            public Vector2 unactiveSize;
            public bool useTweening;
            [FieldVisibleInEditor("useTweening", true)]
            public float changeDuration;
            [FieldVisibleInEditor("useTweening", true)]
            public Ease easeType;

            public void SetSizeTrans(RectTransform trans, bool active)
            {
                if (useTweening)
                {
                    if (active)
                        trans.DOSizeDelta(activeSize, changeDuration);
                    else
                        trans.DOSizeDelta(unactiveSize, changeDuration);
                }
                else
                    trans.sizeDelta = active ? activeSize : unactiveSize;
            }
        }

        [Serializable]
        public class StatusActiveTransform : StatusActiveObjectBaseClass
        {
            public RectTransform transform;
            public bool usePosition;
            [FieldVisibleInEditor("usePosition", true)]
            public TransformPosConfig posConfig;
            public bool useScale;
            [FieldVisibleInEditor("useScale", true)]
            public TransformScaleConfig scaleConfig;
            public bool useSize;
            [FieldVisibleInEditor("useSize", true)]
            public TransformSizeConfig sizeConfig;

            public void SetObjectStatus(bool active)
            {
                if (usePosition)
                    posConfig.SetPosTrans(transform, active);
                if (useScale)
                    scaleConfig.SetScaleTrans(transform, active);
                if (useSize)
                    sizeConfig.SetSizeTrans(transform, active);
            }
        }
        #endregion RectTransform

        #region Object Active Config
        [Serializable]
        public class StatusActiveObj : StatusActiveObjectBaseClass
        {
            public GameObject obj;
            public bool showInActive;

            public void SetObjectStatus(bool active)
            {
                if (active)
                    obj.SetActive(showInActive);
                else
                    obj.SetActive(!showInActive);
            }
        }
        #endregion Object Active Config

        #region Text Config
        [Serializable]
        public class TextStringConfig
        {
            public string activeString;
            public string unactiveString;

            public void SetStringText(Text text, bool active)
            {
                text.text = active ? activeString : unactiveString;
            }
            /*

            public void SetStringText(TextMeshProUGUI text, bool active)
            {
                text.text = active ? activeString : unactiveString;
            }
            */
        }

        [Serializable]
        public class StatusActiveText : StatusActiveObjectBaseClass
        {
            public bool isTextMeshPro = true;
            //[FieldVisibleInEditor("isTextMeshPro", false)]
            public Text text;
            //[FieldVisibleInEditor("isTextMeshPro", true)]
            //public TextMeshProUGUI textMesh;
            public bool useString;
            //[FieldVisibleInEditor("useString", true)]
            public TextStringConfig stringConfig;

            public void SetObjectStatus(bool active)
            {
                if (isTextMeshPro)
                {
                    if (useString)
                    {
                        //stringConfig.SetStringText(textMesh, active);
                    }
                }
                else
                {
                    if (useString)
                        stringConfig.SetStringText(text, active);
                }
            }
        }
        #endregion Text Config

        #endregion SubObject

        #region Serialized Field
        public string statusTag;

        public StatusActiveObj[] objects;
        public StatusActiveTransform[] rectTransforms;
        public StatusActiveGraphic[] graphics;
        public StatusActiveSelectable[] selectables;
        public StatusActiveText[] texts;

        public bool defaultActive;
        public bool isExclusive;
        public bool clickToChange;
        #endregion Serialized Field

        List<StatusActiveObjectBaseClass> _allStatusObject;
        List<StatusActiveObjectBaseClass> allStatusObject
        {
            get
            {
                if(_allStatusObject == null)
                {
                    _allStatusObject = new List<StatusActiveObjectBaseClass>();
                    var allFields = GetType().GetFields();
                    foreach(var f in allFields)
                    {
                        if(f.FieldType.IsArray && typeof(StatusActiveObjectBaseClass).IsAssignableFrom( f.FieldType.GetElementType() ))
                        {
                            StatusActiveObjectBaseClass[] arr = (StatusActiveObjectBaseClass[])f.GetValue(this);
                            _allStatusObject.AddRange(arr);
                        }
                    }
                }
                return _allStatusObject;
            }
        }

        UnityAction<StatusElementActiveConfig, bool> onStatusChanged;

        private bool _currentActive;
        public bool CurrentActive
        {
            get
            {
                return _currentActive;
            }
            private set
            {
                _currentActive = value; // don't check if(_currentActive==value) here, case it's checked in SetStatusActive()
                DoStatusChange();
            }
        }

        bool isInited = false;
        public void Init()
        {
            if (isInited)
                return;
            isInited = true;

            CurrentActive = defaultActive;
        }

        void DoStatusChange()
        {
            foreach (var obj in allStatusObject)
                obj.SetObjectStatus(_currentActive);

            if (onStatusChanged != null)
                onStatusChanged.Invoke(this, _currentActive);
        }

        public void RegisterStatusChangeListener(UnityAction<StatusElementActiveConfig, bool> listener)
        {
            onStatusChanged += listener;
        }

        public void UnRegisterStatusChangeListener(UnityAction<StatusElementActiveConfig, bool> listener)
        {
            onStatusChanged -= listener;
        }

        #region Public Method
        public void ResetStatus()
        {
            SetStatusActive(defaultActive, false);
        }

        /// <summary>
        /// set status active , if status not change
        /// </summary>
        /// <param name="isActive"></param>
        /// <param name="forceRefresh"></param>
        public void SetStatusActive(bool isActive, bool forceRefresh = false)
        {
            if (!isInited)
            {
                Debug.LogError("should call Init() first!");
                return;
            }
                // Init(); // must call this to set all the statusObj to defaultStatus before call SetStatusActive();

            if (CurrentActive == isActive && !forceRefresh)
                return;

            CurrentActive = isActive;
        }

        #endregion Public Method
    }
}


