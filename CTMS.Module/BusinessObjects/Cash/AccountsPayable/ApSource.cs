using DevExpress.Data.Filtering;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace CTMS.Module.BusinessObjects.Cash.AccountsPayable
{
    public class ApSource : BaseObject
    {
        public ApSource(Session session)
            : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }

        // Fields...
        private string _Name;

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                SetPropertyValue("Name", ref _Name, value);
            }
        }

        public new class Fields
        {
            public static OperandProperty Name
            {
                get
                {
                    return new OperandProperty("Name");
                }
            }
        }
    }
}
