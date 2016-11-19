using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.Logic.Cash;
using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class CashFlowFixParamViewController : ViewController
    {
        public CashFlowFixParamViewController()
        {
            TargetObjectType = typeof(CashFlowFixParam);

            var addDays = new SimpleAction(this, "CashFlowRephaseAddDaysAction", "SetupActions");
            addDays.Caption = "Add Days";
            addDays.Execute += (sender, e) => AddDaysAction_Execute(sender, e);

            var runAction = new SimpleAction(this, "CashFlowRunAction", "ExecuteActions");
            runAction.Caption = "Run";
            runAction.Execute += (sender, e) => RunAction_Execute(sender, e);

            var fixAction = new SimpleAction(this, "CashFlowFixAction", "ExecuteActions");
            fixAction.Caption = "Fix";
            fixAction.Execute += (sender, e) => FixAction_Execute(sender, e, true);

            var rephaseAction = new SimpleAction(this, "CashFlowRephaseAction", "ExecuteActions");
            rephaseAction.Caption = "Rephase";
            rephaseAction.Execute += (sender, e) => RephaseAction_Execute(sender, e, true);

            var unfixAction = new SimpleAction(this, "CashFlowUnfixAction", "ExecuteActions");
            unfixAction.Caption = "Unfix";
            unfixAction.Execute += (sender, e) => UnfixAction_Execute(sender, e, true);

            var mapAction = new SimpleAction(this, "CashFlowMapAction", "ExecuteActions");
            mapAction.Caption = "Map";
            mapAction.Execute += (sender, e) => MapAction_Execute(sender, e, true);

            var revalAction = new SimpleAction(this, "CashFlowRevalAction", "ExecuteActions");
            revalAction.Caption = "Reval";
            revalAction.Execute += (sender, e) => RevalAction_Execute(sender, e, true);
        }

        private void AddDaysAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var paramObj = View.CurrentObject as CashFlowFixParam;
            if (paramObj != null)
            {
                paramObj.ApayableLockdownDate = paramObj.ApayableLockdownDate.AddDays(7);
                paramObj.ApayableNextLockdownDate = paramObj.ApayableNextLockdownDate.AddDays(7);
                paramObj.PayrollLockdownDate = paramObj.PayrollLockdownDate.AddDays(7);
                paramObj.PayrollNextLockdownDate = paramObj.PayrollNextLockdownDate.AddDays(7);
            }
        }

        private void RunAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            string messageText = string.Empty;

            messageText += MapAction_Execute(sender, e, false);
            messageText += "\r\n" + RephaseAction_Execute(sender, e, false);
            messageText += "\r\n" + UnfixAction_Execute(sender, e, false);
            messageText += "\r\n" + FixAction_Execute(sender, e, false);
            messageText += "\r\n" + RevalAction_Execute(sender, e, false);

            sw.Stop();

            messageText += string.Format("\r\nTotal Elapsed Time = {0} seconds",
                Math.Round(sw.Elapsed.TotalSeconds,2));

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                messageText, "Cash Flow Fix SUCCESS");
        }

        private string RephaseAction_Execute(object sender, SimpleActionExecuteEventArgs e, bool isRootSender = false)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            ObjectSpace.CommitChanges();

            var os = (XPObjectSpace)Application.CreateObjectSpace();
            var paramObj = View.CurrentObject as CashFlowFixParam;
            if (paramObj != null)
            {
                var algo = new FixCashFlowsAlgorithm(os, paramObj);
                algo.Rephase();
            }
            sw.Stop();
            var messageText = string.Format("Cash Flows were successfully 'Rephased'. Elapsed Time = {0} seconds",
                Math.Round(sw.Elapsed.TotalSeconds, 2));

            if (isRootSender)
            {
                new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                messageText,
                "Cash Flow Rephase SUCCESS");
            }

            return string.Format(messageText,
                Math.Round(sw.Elapsed.TotalSeconds, 2));
        }

        private string RevalAction_Execute(object sender, SimpleActionExecuteEventArgs e, bool isRootSender = false)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var os = (XPObjectSpace)Application.CreateObjectSpace();
            var paramObj = View.CurrentObject as CashFlowFixParam;
            var revaluer = new RevalueAccounts(os, paramObj);
            revaluer.Process();

            sw.Stop();
            var messageText = string.Format("Foreign Currency Account balances were successfully 'Revalued'. Elapsed Time = {0} seconds",
                Math.Round(sw.Elapsed.TotalSeconds,2));

            if (isRootSender)
            {
                new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                    messageText,
                    "Cash Balance Revaluation SUCCESS");
            }

            return string.Format(messageText,
                Math.Round(sw.Elapsed.TotalSeconds,2));
        }

        private string MapAction_Execute(object sender, SimpleActionExecuteEventArgs e, bool isRootSender = false)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var os = (XPObjectSpace)Application.CreateObjectSpace();
            var paramObj = View.CurrentObject as CashFlowFixParam;
            if (paramObj != null)
            {
                var algo = new FixCashFlowsAlgorithm(os, paramObj);
                algo.Map();
            }

            sw.Stop();
            var messageText = string.Format("Cash Flows were successfully 'Mapped'. Elapsed Time = {0} seconds",
                Math.Round(sw.Elapsed.TotalSeconds,2));

            if (isRootSender)
            {
                new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                    messageText,
                    "Cash Flow Map SUCCESS");
                
            }

            return string.Format(messageText,
                Math.Round(sw.Elapsed.TotalSeconds,2));
        }

        private string UnfixAction_Execute(object sender, SimpleActionExecuteEventArgs e, bool isRootSender = false)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var os = (XPObjectSpace)Application.CreateObjectSpace();
            var paramObj = View.CurrentObject as CashFlowFixParam;
            if (paramObj != null)
            {
                var algo = new FixCashFlowsAlgorithm(os, paramObj);
                algo.Reset();
            }

            var messageText = string.Format("Cash Flows were successfully 'Unfixed'. Elapsed Time = {0} seconds",
                Math.Round(sw.Elapsed.TotalSeconds, 2));

            if (isRootSender)
            {
                new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                    messageText,
                    "Cash Flow Unfix SUCCESS");
            }
            return string.Format(messageText,
                 Math.Round(sw.Elapsed.TotalSeconds, 2));
        }

        private string FixAction_Execute(object sender, SimpleActionExecuteEventArgs e, bool isRootSender = false)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            ObjectSpace.CommitChanges();

            var os = (XPObjectSpace)Application.CreateObjectSpace();
            var paramObj = View.CurrentObject as CashFlowFixParam;
            if (paramObj != null)
            {
                var algo = new FixCashFlowsAlgorithm(os, paramObj);
                algo.ApplyFix();
            }

            sw.Stop();
            var messageText = string.Format("Cash Flows were successfully 'Fixed'. Elapsed Time = {0} seconds",
                Math.Round(sw.Elapsed.TotalSeconds,2));

            if (isRootSender)
            {
                new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                    messageText,
                    "Cash Flow Fix SUCCESS");
            }

            return string.Format(messageText,
               Math.Round(sw.Elapsed.TotalSeconds,2));
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            var detailView = View as DetailView;
            if (detailView != null)
                detailView.ViewEditMode = ViewEditMode.Edit;
        }
    }
}
