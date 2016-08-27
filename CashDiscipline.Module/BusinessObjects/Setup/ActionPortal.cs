using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.BusinessObjects.Setup
{
    [DefaultClassOptions]
    [ModelDefault("ImageName", "BO_List")]
    public class ActionPortal : BaseObject
    {
        public ActionPortal()
        {

        }
        public ActionPortal(Session session)
            : base(session)
        {

        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            this.ActionPortalType = ActionPortalType.View;
        }

        private ActionPortalType _ActionPortalType;
        public ActionPortalType ActionPortalType
        {
            get
            {
                return _ActionPortalType;
            }
            set
            {
                SetPropertyValue("ActionPortalType", ref _ActionPortalType, value);
            }
        }

        private string _ActionDescription;
        public string ActionDescription
        {
            get
            {
                return _ActionDescription;
            }
            set
            {
                SetPropertyValue("ActionDescription", ref _ActionDescription, value);
            }
        }

        private string _ObjectType;
        public string ObjectType
        {
            get
            {
                return _ObjectType;
            }
            set
            {
                SetPropertyValue("ObjectType", ref _ObjectType, value);
            }
        }

        private string _ControllerType;
        public string ControllerType
        {
            get
            {
                return _ControllerType;
            }
            set
            {
                SetPropertyValue("ControllerType", ref _ControllerType, value);
            }
        }

        private string _ActionName;
        public string ActionName
        {
            get
            {
                return _ActionName;
            }
            set
            {
                SetPropertyValue("ActionName", ref _ActionName, value);
            }
        }

        private string _ActionPath;
        [Size(255)]
        public string ActionPath
        {
            get
            {
                return _ActionPath;
            }
            set
            {
                SetPropertyValue("ActionPath", ref _ActionPath, value);
            }
        }

    }

    public enum ActionPortalType
    {
        Internal,
        View,
        [XafDisplayName("Choice Action")]
        ChoiceAction,
        [XafDisplayName("Simple Action")]
        SimpleAction
    }
}
