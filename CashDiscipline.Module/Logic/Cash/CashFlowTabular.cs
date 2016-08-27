using DevExpress.ExpressApp.Xpo;
using DG2NTT.AnalysisServicesHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Cash
{
    public class CashFlowTabular
    {
        public CashFlowTabular()
        {
            if (CashDiscipline.Common.Constants.MsasTabularCompatibility_13)
            {
                tabular = new CashFlowTabular16();
            }
            else
            {
                tabular = new CashFlowTabular14();
            }
        }

        public string LastReturnMessage
        {
            get
            {
                return tabular.LastReturnMessage;
            }
        }

        private ICashFlowTabular tabular;

        public void ProcessAll()
        {
            tabular.ProcessAll();
        }

        public void ProcessCurrent(XPObjectSpace objSpace)
        {
            tabular.ProcessCurrent(objSpace);
        }

        public void ProcessHist()
        {
            tabular.ProcessHist();
        }

        public void ProcessSshot()
        {
            tabular.ProcessSshot();
        }
    }
}
