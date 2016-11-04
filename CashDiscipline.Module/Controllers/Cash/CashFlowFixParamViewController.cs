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

        private void RunAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            string messageText = string.Empty;

            messageText += MapAction_Execute(sender, e, false);
            messageText += "\r\n" + FixAction_Execute(sender, e, false);
            messageText += "\r\n" + RevalAction_Execute(sender, e, false);

            sw.Stop();

            messageText += string.Format("\r\nTotal Elapsed Time = {0} seconds",
                sw.Elapsed.Seconds);

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                messageText, "Cash Flow Fix SUCCESS");
        }

        private void RephaseAction_Execute(object sender, SimpleActionExecuteEventArgs e, bool isRootSender = false)
        {
            ObjectSpace.CommitChanges();

            var os = (XPObjectSpace)Application.CreateObjectSpace();
            var paramObj = View.CurrentObject as CashFlowFixParam;
            if (paramObj != null)
            {
                var algo = new FixCashFlowsAlgorithm(os, paramObj);
                algo.Rephase();
            }

            if (isRootSender)
            {
                new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                "Cash Flows were successfully 'Rephased'.",
                "Cash Flow Fix SUCCESS");
            }
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
                sw.Elapsed.Seconds);

            if (isRootSender)
            {
                new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                    messageText,
                    "Cash Balance Revaluation SUCCESS");
            }

            return string.Format(messageText,
                sw.Elapsed.Seconds);
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
                sw.Elapsed.Seconds);

            if (isRootSender)
            {
                new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                    messageText,
                    "Cash Flow Map SUCCESS");
                
            }

            return string.Format(messageText,
                sw.Elapsed.Seconds);
        }

        private void UnfixAction_Execute(object sender, SimpleActionExecuteEventArgs e, bool isRootSender = false)
        {
            var os = (XPObjectSpace)Application.CreateObjectSpace();
            var paramObj = View.CurrentObject as CashFlowFixParam;
            if (paramObj != null)
            {
                var algo = new FixCashFlowsAlgorithm(os, paramObj);
                algo.Reset();
            }

            if (isRootSender)
            {
                new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                    "Cash Flows were successfully 'Unfixed'.",
                    "Cash Flow Unfix SUCCESS");
            }
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
                algo.Reset();
                algo.ProcessCashFlows();
            }

            sw.Stop();
            var messageText = string.Format("Cash Flows were successfully 'Fixed'. Elapsed Time = {0} seconds",
                sw.Elapsed.Seconds);

            if (isRootSender)
            {
                new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                    messageText,
                    "Cash Flow Fix SUCCESS");
            }

            return string.Format(messageText,
               sw.Elapsed.Seconds);
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
