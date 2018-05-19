using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI_2_SuperScroll
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FieldVisibleInEditorAttribute : Attribute
    {

        public FieldVisibleInEditorAttribute(string name, object value)
        {

        }
    }
}
