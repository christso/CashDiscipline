using CashDiscipline.Module.BusinessObjects.FinAccounting;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

using CashDiscipline.Module.ParamObjects.FinAccounting;
using DevExpress.ExpressApp.Xpo;
using CashDiscipline.Module.Logic.FinAccounting;
using DevExpress.ExpressApp.SystemModule;
using CashDiscipline.Module.Logic;

namespace CashDiscipline.Module.Controllers.FinAccounting 
{
    public class AustPostSettleViewController : ViewController
    {
        public AustPostSettleViewController()
        {
            TargetObjectType = typeof(AustPostSettle);

            AustPostActions = new SingleChoiceAction(this, "AustPostActions", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            AustPostActions.Caption = "Actions";
            AustPostActions.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            AustPostActions.Execute += runProgramAction_Execute;
            AustPostActions.ShowItemsOnClick = true;

            var fetchChoice = new ChoiceActionItem();
            fetchChoice.Caption = "Fetch from Bank Stmt";
            AustPostActions.Items.Add(fetchChoice);
        }

        private void runProgramAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var algo = new SqlAlgorithm((XPObjectSpace)ObjectSpace);
            algo.CommandText =
@"DECLARE @AustPostSettleActivity uniqueidentifier = (SELECT TOP 1 AustPostSettleActivity FROM SetOfBOoks WHERE GCRecord IS NULL)

INSERT INTO AustPostSettle
(
Oid,
BankStmt,
BankStmtDate,
BankStmtAmount,
GrossAmount,
DishonourChequeFee,
NegativeCorrections,
DishonourChequeReversal,
GrossCommission,
CommissionGST,
DateTimeCreated
)
SELECT 
	NEWID(),
	bs.Oid,
    bs.TranDate,
	bs.TranAmount,
	bs.TranAmount,
	0.00,
	0.00,
	0.00,
	0.00,
	0.00,
	GETDATE()
FROM BankStmt bs
WHERE bs.GCRecord IS NULL
	AND bs.Activity = @AustPostSettleActivity
	AND NOT EXISTS (SELECT * FROM AustPostSettle aps WHERE aps.BankStmt = bs.Oid AND aps.GCRecord IS NULL)";
            algo.Process();
            ObjectSpace.CommitChanges();
        }

        private SingleChoiceAction AustPostActions;
    }
}
