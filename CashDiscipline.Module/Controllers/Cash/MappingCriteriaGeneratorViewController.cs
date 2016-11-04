using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.Interfaces;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.Xpo;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class MappingCriteriaGeneratorViewController : ViewController
    {
        public MappingCriteriaGeneratorViewController()
        {
            TargetObjectType = typeof(IMappingCriteriaGenerator);
            var buildAction = new SimpleAction(this, "MappingCriteriaBuildAction", "ExecuteActions");
            buildAction.ImageName = "ModelEditor_GenerateContent";
            buildAction.Caption = "Build Criteria Expression";
            buildAction.Execute += (sender, e) => BuildCriteriaExpression_Execute(sender, e);
        }

        private void BuildCriteriaExpression_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var obj = (IMappingCriteriaGenerator)View.CurrentObject;
            var xpoCriteriaText = obj.Criteria;

            var criteria = CriteriaEditorHelper.GetCriteriaOperator(
                xpoCriteriaText, obj.CriteriaObjectType, ObjectSpace);
            obj.CriteriaExpression = CriteriaToWhereClauseHelper.GetOracleWhere(XpoCriteriaFixer.Fix(criteria));
        }
    }
}
