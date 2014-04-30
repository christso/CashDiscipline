using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DG2NTT.Utilities;

namespace CTMS.Module.ParamObjects.Import
{
    public enum ImportLibrary
    {
        [XafDisplayName("Fast CSV Reader")]
        FastCsvReader,
        [XafDisplayName("FileHelpers")]
        FileHelpers
    }
}
