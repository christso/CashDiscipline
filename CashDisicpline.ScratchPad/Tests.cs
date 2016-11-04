using DevExpress.Data.Filtering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.Xpo;

namespace CashDisicpline.ScratchPad
{
    public class Tests
    {
        public void CriteriaToSqlTest()
        {
            string xpoCriteriaText = "Activity IS NOT NULL OR [Action Owner.Name] Like 'UNDEFINED'";
            var criteria = CriteriaOperator.Parse(xpoCriteriaText);
            var sqlCriteriaText = CriteriaToWhereClauseHelper.GetMsSqlWhere(XpoCriteriaFixer.Fix(criteria));
            Console.WriteLine(sqlCriteriaText);
        }
    }
}
