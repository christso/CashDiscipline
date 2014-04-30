using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using DevExpress.Data;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.DC;

namespace CTMS.Module.Model
{
    public interface IModelIcon : IModelNode
    {
        [Category("Appearance")]
        string ItemIcon { get; set; }
    }
    public interface IModelLayoutItemIcon : IModelNode
    {
        [Category("Appearance")]
        string ItemIcon { get; set; }
    }

    [DomainLogic(typeof(IModelIcon))]
    public static class ModelMemberItemIconLogic
    {
        public static string Get_ItemIcon(IModelMember modelMember)
        {
            if (modelMember != null && modelMember.MemberInfo != null)
            {
                ItemIconAttribute attribute = modelMember.MemberInfo.FindAttribute<ItemIconAttribute>();
                if (attribute != null)
                    return attribute.ItemIcon;
            }
            return null;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ItemIconAttribute : Attribute
    {
        public static ItemIconAttribute Default = new ItemIconAttribute(null);

        public ItemIconAttribute(string value) { ItemIcon = value; }
        public string ItemIcon { get; set; }
    }
}
