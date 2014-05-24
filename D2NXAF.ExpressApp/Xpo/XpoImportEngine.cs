using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.ExpressApp;
using D2NXAF.Utils;
using System.Collections;
using DevExpress.Persistent.Base;

namespace CTMS.Module.HelperClasses.Xpo
{
    // Note: Lookup objects only work with strings as the Default property
    public class XpoImportEngine
    {
        protected XafApplication Application;

        public XpoImportEngine(XafApplication application)
        {
            Application = application;
            _Options = new ImportOptions();   
            _XpObjectsNotFound = new Dictionary<Type, List<string>>();
            _CachedXpObjects = new Dictionary<Type, IList>();
        }
        // List<string> contains object default values
        private Dictionary<Type, List<string>> _XpObjectsNotFound;
        public Dictionary<Type, List<string>> XpObjectsNotFound
        {
            get { return _XpObjectsNotFound; }
        }

        private Dictionary<Type, IList> _CachedXpObjects;
        public Dictionary<Type, IList> CachedXpObjects
        {
            get { return _CachedXpObjects; }
        }

        private ImportOptions _Options;
        public ImportOptions Options
        {
            get
            {
                return _Options;
            }
        }

        protected void CacheXpObjects(ITypeInfo objTypeInfo, string[] memberNames, XPObjectSpace objSpace)
        {
            foreach (var memberInfo in objTypeInfo.Members)
            {
                if (!typeof(IXPObject).IsAssignableFrom(memberInfo.MemberType)
                    || memberInfo.IsKey || !memberNames.Contains(memberInfo.Name))
                    continue;

                // add objects to cache dictionary
                IList objs;
                if (!CachedXpObjects.TryGetValue(memberInfo.MemberType, out objs))
                {
                    objs = objSpace.GetObjects(memberInfo.MemberType);
                    CachedXpObjects.Add(memberInfo.MemberType, objs);
                }
                else
                {
                    CachedXpObjects[memberInfo.MemberType] = objs;
                }
            }
        }

        /// <summary>
        /// Sets the value of the member of targetObj
        /// </summary>
        /// <param name="targetObj">Main object whose members are to be assigned a value</param>
        /// <param name="memberInfo">Information about the member used to determine how the value is converted to the member type</param>
        /// <param name="value">Value to assigned to the member</param>
        public void SetMemberValue(IXPObject targetObj, IMemberInfo memberInfo, string value)
        {
            object newValue;
            if (string.IsNullOrWhiteSpace(value))
            {
                newValue = null;
                return;
            }
            if (memberInfo.MemberType == typeof(DateTime))
            {
                newValue = Convert.ToDateTime(value);
            }
            else if (memberInfo.MemberType == typeof(bool))
            {
                newValue = ConvertToBool(value);
            }
            else if (memberInfo.MemberType == typeof(decimal))
            {
                newValue = Convert.ToDecimal(value);
            }
            else if (memberInfo.MemberType == typeof(int))
            {
                newValue = Convert.ToInt32(value);
            }
            else if (typeof(Enum).IsAssignableFrom(memberInfo.MemberType))
            {
                newValue = ConvertToEnum(value, memberInfo.MemberType);
            }
            else if (memberInfo.MemberType.IsNumericType())
            {
                newValue = Convert.ToDouble(value);
            }
            else if (typeof(IXPObject).IsAssignableFrom(memberInfo.MemberType))
            {
                if (_Options.CacheObjects)
                {
                    newValue = ConvertToXpObjectCached(value, memberInfo, targetObj.Session);
                }
                else
                {
                    newValue = ConvertToXpObject(value, memberInfo, targetObj.Session);
                }
            }
            else
            {
                newValue = value;
            }
            memberInfo.SetValue(targetObj, newValue);
        }

        private Enum ConvertToEnum(string value, Type memberType)
        {
            // use Display Name if attribute is found
            var fields = memberType.GetFields();
            foreach (var field in fields)
            {
                object[] attrs = field.GetCustomAttributes(typeof(DevExpress.ExpressApp.DC.XafDisplayNameAttribute), true);
                foreach (DevExpress.ExpressApp.DC.XafDisplayNameAttribute attr in attrs)
                {
                    if (attr.DisplayName == value)
                    {
                        value = field.Name;
                        break;
                    }
                }
            }
            return (Enum)Enum.Parse(memberType, value.Replace(" ", ""), true);
        }

        private IXPObject ConvertToXpObjectCached(string value, IMemberInfo memberInfo, Session session)
        {
            IList lookupObjs = CachedXpObjects[memberInfo.MemberType];
            IXPObject newValue = null;
            var memTypeId = ModelNodeIdHelper.GetTypeId(memberInfo.MemberType);
            var model = Application.Model.BOModel[memTypeId];
            var lookupMemberInfo = model.FindMember(model.DefaultProperty).MemberInfo;

            foreach (IXPObject lookupObj in lookupObjs)
            {
                // get lookup member model
                
                
                // get default property of lookup member
                object lookupValue = lookupMemberInfo.GetValue(lookupObj);

                if (Convert.ToString(lookupValue) == value)
                {
                    newValue = lookupObj;
                    break;
                }
            }
            if (newValue == null)
                newValue = OnMissingMember(value, memberInfo.MemberType, session, model.DefaultProperty);
            return newValue;
        }

        private IXPObject ConvertToXpObject(string value, IMemberInfo memberInfo, Session session)
        {
            object newValue;
            var memberType = memberInfo.MemberType;
            var memTypeId = ModelNodeIdHelper.GetTypeId(memberType);
            var model = Application.Model.BOModel[memTypeId];
            var cop = CriteriaOperator.Parse(string.Format("[{0}] = ?", model.DefaultProperty), value);
            newValue = session.FindObject(memberType, cop);
            if (newValue == null)
                newValue = OnMissingMember(value, memberType, session, model.DefaultProperty);
            return (IXPObject)newValue;
        }

        private IXPObject OnMissingMember(string value, Type memberType, Session session, string defaultProperty)
        {
            object newValue = null;
            if (_Options.CreateMembers)
            {
                var newObj = (IXPObject)Activator.CreateInstance(memberType, session);
                ReflectionHelper.SetMemberValue(newObj, defaultProperty, value);
                newObj.Session.Save(newObj);
                newValue = newObj;
                CachedXpObjects[memberType].Add(newValue);
            }
            List<string> memberValues;
            if (!XpObjectsNotFound.TryGetValue(memberType, out memberValues))
            {
                memberValues = new List<string>();
                XpObjectsNotFound.Add(memberType, memberValues);
            }
            if (!memberValues.Contains(value))
                memberValues.Add(value);
            return (IXPObject)newValue;
        }

        private bool ConvertToBool(string value)
        {
            switch (value.ToLower())
            {
                case "unchecked":
                    return false;
                case "checked":
                    return true;
                default:
                    return Convert.ToBoolean(value);
            }
        }
    }
    public class ImportOptions
    {
        public bool CreateMembers = false;
        public bool CacheObjects = false;
        public bool UseOrdinals = false;
        public bool HasHeaders = true;
    }
}
