using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI_2_SuperScroll
{
    /// <summary>
    /// all UI element in group should use this class 
    /// such as the element in PosBaseScroll, InfiniteScrollView, UIElementGroup...
    /// </summary>
    public class UIInGroupElement : MonoBehaviour
    {
        [SerializeField]
        public StatusElementActiveConfig[] statusConfigs;

        RectTransform _rectTransform;
        public virtual RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        UIStatusElementGroup _group;
        public UIStatusElementGroup StatusElementGroup
        {
            get
            {
                if (_group == null)
                    _group = GetComponentInParent<UIStatusElementGroup>();
                return _group;
            }
        }

        #region Datas
        object data;
        public object Data
        {
            get
            {
                return data;
            }
            set
            {
                if (data == value)
                    return;
                data = value;
                OnDataChange();
            }
        }

        public virtual void SetData(object newValue, int dataIndex)
        {
            if (data == newValue && _dataIndex == dataIndex)
                return;

            _dataIndex = dataIndex;
            Data = newValue;
        }

        protected virtual void OnDataChange()
        {

        }

        int _dataIndex = -1;
        public int DataIndex
        {
            get { return _dataIndex; }
            set { _dataIndex = value; }
        }
        #endregion Datas

        #region Status
        Dictionary<string, StatusElementActiveConfig> _name2StatusConfig;
        Dictionary<string, StatusElementActiveConfig> name2StatusConfig
        {
            get
            {
                if (_name2StatusConfig == null)
                {
                    _name2StatusConfig = new Dictionary<string, StatusElementActiveConfig>();
                    var count = statusConfigs.Length;
                    for(int i = 0; i < count; i++)
                    {
                        var config = statusConfigs[i];
                        var statusTag = config.statusTag;
                        if(string.IsNullOrEmpty(statusTag))
                        {
                            Debug.LogError(name + "'s statusTag should not be null or empty! statusIndex : " + i.ToString());
                            continue;
                        }

                        if(_name2StatusConfig.ContainsKey(statusTag))
                        {
                            Debug.LogError(name + "'s statusTag Duplicate! statusIndex : " + i.ToString());
                            continue;
                        }
                        _name2StatusConfig.Add(statusTag, config);
                    }
                }
                return _name2StatusConfig;
            }
        }

        /// <summary>
        /// change element status by Name
        /// </summary>
        /// <param name="statusName"></param>
        /// <param name="isActive"></param>
        public void ChangeOneStatus(string statusName, bool isActive, bool checkExclusive = true, bool forceRefresh = false)
        {
            StatusElementActiveConfig config;
            if(name2StatusConfig.TryGetValue(statusName, out config))
            {
                if(checkExclusive && config.isExclusive)
                    StatusElementGroup.ChangeExclusiveStatus(statusName, isActive, _dataIndex, forceRefresh);

                config.SetStatusActive(isActive, forceRefresh);
            }
            else
                Debug.LogWarning(statusName + "not exists!");
        }

        protected virtual void OnStatusChange(StatusElementActiveConfig status, bool isActive)
        {

        }

        #endregion Status

        #region Init
        bool _isInited = false;
        /// <summary>
        /// call this after Instantiate this gameOjbect
        /// </summary>
        public void Init()
        {
            if (_isInited)
                return;
            _isInited = true;

            foreach (var config in statusConfigs)
            {
                config.Init();
                config.RegisterStatusChangeListener(OnStatusChange);
            }
        }

        private void Awake()
        {
            Init();
        }

        private void OnDestroy()
        {
            foreach(var config in statusConfigs)
                config.UnRegisterStatusChangeListener(OnStatusChange);
        }
        #endregion Init

        /// <summary>
        /// reset element data and status
        /// </summary>
        public void ResetElement()
        {
            data = null;
            foreach (var config in statusConfigs)
                config.ResetStatus();
        }

        #region ClickEvent
        List<StatusElementActiveConfig> _clickToChangeStatus;
        List<StatusElementActiveConfig> clickToChangeStatus
        {
            get
            {
                if(_clickToChangeStatus == null)
                {
                    _clickToChangeStatus = new List<StatusElementActiveConfig>();
                    foreach(var status in statusConfigs)
                        if (status.clickToChange)
                            _clickToChangeStatus.Add(status);
                }
                return _clickToChangeStatus;
            }
        }

        public virtual void OnClickElement()
        {
            foreach (var status in clickToChangeStatus)
                ChangeOneStatus(status.statusTag, !status.CurrentActive);
        }
        #endregion ClickEvent

        protected virtual void DoChangeVisible(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }

    /// <summary>
    /// Generic version of UIInGroupElement
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UIInGroupElement<T> : UIInGroupElement
    {
        public new T Data
        {
            get
            {
                return (T)base.Data;
            }
            set
            {
                base.Data = value;
            }
        }
    }
}
