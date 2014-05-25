﻿using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public class AutoCreatableObjectAttribute : Attribute
    {
        private bool autoCreatable = true;
        private ViewEditMode viewEditMode = ViewEditMode.Edit;
        public AutoCreatableObjectAttribute()
        {
        }
        public AutoCreatableObjectAttribute(bool autoCreatable)
        {
            this.autoCreatable = autoCreatable;
        }
        public bool AutoCreatable
        {
            get { return autoCreatable; }
        }
        public ViewEditMode ViewEditMode
        {
            get { return viewEditMode; }
            set { viewEditMode = value; }
        }
    }
}
