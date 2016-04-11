using DevExpress.Persistent.BaseImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpo;

namespace CashDiscipline.UnitTests.TestObjects
{
    public class MockLookupObject1 : BaseObject
    {
        private string name;
        public MockLookupObject1(DevExpress.Xpo.Session session)
            : base(session)
        {

        }
        public MockLookupObject1()
        {

        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                SetPropertyValue("Name", ref name, value);
            }
        }

    }
}
