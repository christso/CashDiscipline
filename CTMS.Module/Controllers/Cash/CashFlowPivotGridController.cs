using CTMS.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.XtraPivotGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTMS.Module.BusinessObjects.User;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using CTMS.Module.ParamObjects.User;
using DevExpress.ExpressApp.SystemModule;
using CTMS.Module.HelperClasses.UI;
using CTMS.Module.HelperClasses;
using System.IO;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Xpo;
using System.Diagnostics;
using DG2NTT.ExpressApp.PivotGrid.Controllers;
using CTMS.Module.ParamObjects.Cash;
using System.Collections;
using CTMS.Module.BusinessObjects;

namespace CTMS.Module.Controllers.Cash
{
    public class CashFlowPivotGridController : CashReportViewController
    {
        public CashFlowPivotGridController()
        {
            TargetObjectType = typeof(CashFlow);
            TargetViewType = ViewType.ListView;
            TargetViewId = "CashFlow_PivotGridView";

            myLayoutAction = new SingleChoiceAction(this, "UserLayoutAction", DevExpress.Persistent.Base.PredefinedCategory.View);
            myLayoutAction.Caption = "Layout";
            myLayoutAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            myLayoutAction.DefaultItemMode = DefaultItemMode.FirstActiveItem;
            myLayoutAction.ShowItemsOnClick = true;
            myLayoutAction.Execute += myLayoutAction_Execute;

            var saveLayoutChoice = new ChoiceActionItem();
            saveLayoutChoice.Caption = "Save";
            myLayoutAction.Items.Add(saveLayoutChoice);

            var loadLayoutChoice = new ChoiceActionItem();
            loadLayoutChoice.Caption = "Load";
            myLayoutAction.Items.Add(loadLayoutChoice);

            var resetChoice = new ChoiceActionItem();
            resetChoice.Caption = "Reset";
            myLayoutAction.Items.Add(resetChoice);
        }

        protected override void SetupView()
        {
            base.SetupView();
            var listView = (ListView)View;

            var dateCriteria = CriteriaOperator.Parse("TranDate Between (?,?)",
            ReportParam.FromDate, ReportParam.ToDate);

            CriteriaOperator snapshotCriteria = null;
            if (ReportParam.Snapshot1 != null)
                snapshotCriteria = snapshotCriteria | CriteriaOperator.Parse("Snapshot.Oid = ?", ReportParam.Snapshot1.Oid);
            if (ReportParam.Snapshot2 != null)
                snapshotCriteria = snapshotCriteria | CriteriaOperator.Parse("Snapshot.Oid = ?", ReportParam.Snapshot2.Oid);
            listView.CollectionSource.Criteria["Filter1"] = dateCriteria & snapshotCriteria;
        }

        void myLayoutAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var objSpace = Application.CreateObjectSpace();

            switch (e.SelectedChoiceActionItem.Caption)
            {
                case "Save":
                    var saveDialog = new PopupDialogListViewManager(Application, typeof(UserViewLayout), objSpace);
                    saveDialog.Accepting += saveDialog_Accepting;
                    saveDialog.ShowView();
                    break;
                case "Load":
                    var loadDialog = new PopupDialogListViewManager(Application, typeof(UserViewLayout), objSpace);
                    loadDialog.Accepting += loadDialog_Accepting;
                    loadDialog.ShowView();
                    break;
                case "Reset":
                    myLayoutAction.Caption = "Layout";
                    ResetPivotGridLayout();
                    break;
            }
        }

        private ParametrizedAction parametrizedAction1;

        public const string fieldNoteCaption = "Note";

        protected virtual void SavePivotGridLayoutToStream(MemoryStream stream)
        {
        }

        void loadDialog_Accepting(object sender, DialogControllerAcceptingEventArgs e)
        {
            var controller = (DialogController)sender;
            var objSpace = controller.Frame.View.ObjectSpace;

            var layoutObj = (UserViewLayout)e.AcceptActionArgs.CurrentObject;
            if (layoutObj != null)
            {
                // save selected layout as default for user
                var settings = GetUserDefaultSettings();
                settings.UserViewLayout = ObjectSpace.GetObjectByKey<UserViewLayout>(layoutObj.Session.GetKeyValue(layoutObj));
                settings.Session.CommitTransaction();
                LoadPivotGridLayout(layoutObj);
            }
            objSpace.CommitChanges();
        }

        private void saveDialog_Accepting(object sender, DialogControllerAcceptingEventArgs e)
        {
            var layoutObj = (UserViewLayout)e.AcceptActionArgs.CurrentObject;

            // save selected layout as default for user
            var settings = GetUserDefaultSettings();
            settings.UserViewLayout = ObjectSpace.GetObjectByKey<UserViewLayout>(layoutObj.Session.GetKeyValue(layoutObj));
            settings.Save();
            ObjectSpace.CommitChanges();
            SavePivotGridLayout(layoutObj);
        }

        protected virtual void LoadPivotGridLayout(UserViewLayout layoutObj)
        {
            myLayoutAction.Caption = string.Format("Layout : {0}", layoutObj.LayoutName);
        }

        /// <summary>
        /// Saves Pivot Layout into the layout object.
        /// </summary>
        /// <remarks>It will not save the Layout Object. You need to save the layout object youself.</remarks>
        /// <param name="layoutObj">The object to save the Pivot Layout to.</param>
        protected virtual void SavePivotGridLayout(UserViewLayout layoutObj)
        {
        }

        protected void SavePivotGridLayout(UserViewLayout layoutObj, MemoryStream stream)
        {
            myLayoutAction.Caption = string.Format("Layout : {0}", layoutObj.LayoutName);
            layoutObj.LayoutFile = new FileData(layoutObj.Session);
            layoutObj.LayoutFile.LoadFromStream("PivotGridLayout.xml", stream);
            layoutObj.Save();
            layoutObj.Session.CommitTransaction();
        }

        protected virtual void ResetPivotGridLayout()
        {
            RestoreAppLayouts();
            ObjectSpace.CommitChanges();
        }

        /// <summary>
        /// Mirror platform-agnostic PivotGridControl layout to the WinForms and ASP.NET PivotGridControl
        /// </summary>
        protected virtual void SyncPivotGridFieldsToControl()
        {
        }

        protected virtual PivotGridFieldCollectionBase PivotGridControlFields
        { get { return null; } }

        protected XPObjectSpace SearchObjectSpace {get; set;}

        private SingleChoiceAction myLayoutAction;

        private void AddCriteriaText(ref string outputText, string text, string op = "And")
        {
            if (!string.IsNullOrWhiteSpace(outputText))
                outputText += " " + op + " ";
            outputText += text;
        }

        protected List<PivotGridFieldBase> PivotGridFields;

        protected PivotGridFieldBase fieldSnapshot;
        protected PivotGridFieldBase fieldTranYear;
        protected PivotGridFieldBase fieldTranQuarter;
        protected PivotGridFieldBase fieldTranMonth;
        protected PivotGridFieldBase fieldTranDate;
        protected PivotGridFieldBase fieldAccountCcyAmt;
        protected PivotGridFieldBase fieldFunctionalCcyAmt;
        protected PivotGridFieldBase fieldAccountCcyAmtVar;
        protected PivotGridFieldBase fieldFunctionalCcyAmtVar;
        protected PivotGridFieldBase fieldCounterCcyAmtVar;
        protected PivotGridFieldBase fieldSource;
        protected PivotGridFieldBase fieldActivity;
        protected PivotGridFieldBase fieldDim_1_1;
        protected PivotGridFieldBase fieldDim_1_2;
        protected PivotGridFieldBase fieldDim_1_3;
        protected PivotGridFieldBase fieldFix;
        protected PivotGridFieldBase fieldFixType;
        protected PivotGridFieldBase fieldFixRank;
        protected PivotGridFieldBase fieldFixerYear;
        protected PivotGridFieldBase fieldFixerQuarter;
        protected PivotGridFieldBase fieldFixerMonth;
        protected PivotGridFieldBase fieldFixerDate;
        protected PivotGridFieldBase fieldFixActivity;
        protected PivotGridFieldBase fieldParentYear;
        protected PivotGridFieldBase fieldParentQuarter;
        protected PivotGridFieldBase fieldParentMonth;
        protected PivotGridFieldBase fieldParentDate;
        protected PivotGridFieldBase fieldParentSource;
        protected PivotGridFieldBase fieldParentActivity;
        protected PivotGridFieldBase fieldParentDim_1_1;
        protected PivotGridFieldBase fieldParentDim_1_2;
        protected PivotGridFieldBase fieldParentDim_1_3;
        protected PivotGridFieldBase fieldCounterCcy;
        protected PivotGridFieldBase fieldNote;

        protected UserDefaultSetting GetUserDefaultSettings()
        {
            return UserDefaultSetting.GetUserDefaultSettings(ObjectSpace);
        }

        protected virtual void CreatePivotGridFields()
        {
            #region Cash Flow

            fieldSnapshot = new PivotGridFieldBase("Snapshot.Name", PivotArea.FilterArea);
            fieldSnapshot.Name = "fieldSnapshot";
            fieldSnapshot.Caption = "Snapshot";
            fieldSnapshot.SortMode = PivotSortMode.Custom;

            fieldTranYear = new PivotGridFieldBase("TranDate", PivotArea.FilterArea);
            fieldTranYear.Name = "fieldTranYear";
            fieldTranYear.Caption = "Tran Year";
            fieldTranYear.GroupInterval = PivotGroupInterval.DateYear;

            fieldTranQuarter = new PivotGridFieldBase("TranDate", PivotArea.FilterArea);
            fieldTranQuarter.GroupInterval = PivotGroupInterval.DateQuarter;
            fieldTranQuarter.Name = "fieldTranQuarter";
            fieldTranQuarter.Caption = "Tran Quarter";
            fieldTranQuarter.ValueFormat.FormatString = "Qtr {0}";
            fieldTranQuarter.ValueFormat.FormatType = DevExpress.Utils.FormatType.Custom;

            fieldTranMonth = new PivotGridFieldBase("TranDate", PivotArea.FilterArea);
            fieldTranMonth.Name = "fieldTranMonth";
            fieldTranMonth.Caption = "Tran Month";
            fieldTranMonth.GroupInterval = PivotGroupInterval.DateMonth;
            fieldTranMonth.ValueFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            fieldTranMonth.ValueFormat.FormatString = "{0:MMM}";

            fieldTranDate = new PivotGridFieldBase("TranDate", PivotArea.FilterArea);
            fieldTranDate.Name = "fieldTranDate";
            fieldTranDate.Caption = "Tran Date";
            fieldTranDate.ValueFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            fieldTranDate.ValueFormat.FormatString = "{0:dd-MMM-yy}";

            fieldAccountCcyAmt = new PivotGridFieldBase("AccountCcyAmt", PivotArea.FilterArea);
            fieldAccountCcyAmt.Name = "fieldAccountCcyAmt";
            fieldAccountCcyAmt.Caption = "Account Ccy Amt";
            fieldAccountCcyAmt.CellFormat.FormatString = "n2";
            fieldAccountCcyAmt.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;

            fieldFunctionalCcyAmt = new PivotGridFieldBase("FunctionalCcyAmt", PivotArea.FilterArea);
            fieldFunctionalCcyAmt.Name = "fieldFunctionalCcyAmt";
            fieldFunctionalCcyAmt.Caption = "Functional Ccy Amt";
            fieldFunctionalCcyAmt.CellFormat.FormatString = "n2";
            fieldFunctionalCcyAmt.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            fieldFunctionalCcyAmt.ColumnValueLineCount = 2;
            
            fieldAccountCcyAmtVar = new PivotGridFieldBase("AccountCcyAmt", PivotArea.FilterArea);
            fieldAccountCcyAmtVar.Name = "fieldAccountCcyAmtVarVar";
            fieldAccountCcyAmtVar.Caption = "Account Ccy Amt Var";
            fieldAccountCcyAmtVar.CellFormat.FormatString = "n2";
            fieldAccountCcyAmtVar.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            fieldAccountCcyAmtVar.SummaryType = DevExpress.Data.PivotGrid.PivotSummaryType.Custom;
            fieldAccountCcyAmtVar.ColumnValueLineCount = 2;


            fieldFunctionalCcyAmtVar = new PivotGridFieldBase("FunctionalCcyAmt", PivotArea.FilterArea);
            fieldFunctionalCcyAmtVar.Name = "fieldFunctionalCcyAmtVar";
            fieldFunctionalCcyAmtVar.Caption = "Functional Ccy Amt Var";
            fieldFunctionalCcyAmtVar.CellFormat.FormatString = "n2";
            fieldFunctionalCcyAmtVar.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            fieldFunctionalCcyAmtVar.SummaryType = DevExpress.Data.PivotGrid.PivotSummaryType.Custom;
            fieldFunctionalCcyAmtVar.ColumnValueLineCount = 2;

            fieldCounterCcyAmtVar = new PivotGridFieldBase("CounterCcyAmt", PivotArea.FilterArea);
            fieldCounterCcyAmtVar.Name = "fieldCounterCcyAmtVar";
            fieldCounterCcyAmtVar.Caption = "Counter Ccy Amt Var";
            fieldCounterCcyAmtVar.CellFormat.FormatString = "n2";
            fieldCounterCcyAmtVar.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            fieldCounterCcyAmtVar.SummaryType = DevExpress.Data.PivotGrid.PivotSummaryType.Custom;
            fieldCounterCcyAmtVar.ColumnValueLineCount = 2;

            fieldActivity = new PivotGridFieldBase("Activity.Name", PivotArea.FilterArea);
            fieldActivity.Caption = "Activity";

            fieldDim_1_1 = new PivotGridFieldBase("Activity.Dim_1_1", PivotArea.FilterArea);
            fieldDim_1_1.Name = "fieldDim_1_1";
            fieldDim_1_1.Caption = "Dim_1_1";

            fieldDim_1_2 = new PivotGridFieldBase("Activity.Dim_1_2", PivotArea.FilterArea);
            fieldDim_1_2.Name = "fieldDim_1_2";
            fieldDim_1_2.Caption = "Dim_1_2";

            fieldDim_1_3 = new PivotGridFieldBase("Activity.Dim_1_3", PivotArea.FilterArea);
            fieldDim_1_3.Name = "fieldDim_1_3";
            fieldDim_1_3.Caption = "Dim_1_3";

            fieldSource = new PivotGridFieldBase("Source.Name", PivotArea.FilterArea);
            fieldSource.Name = "fieldSource";
            fieldSource.Caption = "Source";

            fieldCounterCcy = new PivotGridFieldBase("CounterCcy.Name", PivotArea.FilterArea);
            fieldCounterCcy.Name = "fieldCounterCcy";
            fieldCounterCcy.Caption = "Counter Ccy";

            fieldFixActivity = new PivotGridFieldBase("FixActivity.Name", PivotArea.FilterArea);
            fieldFixActivity.Name = "fieldFixActivity";
            fieldFixActivity.Caption = "Fix Activity";

            fieldFix = new PivotGridFieldBase("Fix.Name", PivotArea.FilterArea);
            fieldFix.Name = "fieldFix";
            fieldFix.Caption = "Fix";

            fieldFixType = new PivotGridFieldBase("Fix.FixTagType", PivotArea.FilterArea);
            fieldFixType.Name = "fieldFixType";
            fieldFixType.Caption = "FixType";

            fieldNote = new PivotGridFieldBase("Note", PivotArea.FilterArea);
            fieldNote.Name = "fieldNote";
            fieldNote.UnboundType = DevExpress.Data.UnboundColumnType.String;
            fieldNote.SummaryType = DevExpress.Data.PivotGrid.PivotSummaryType.Custom;
            fieldNote.Caption = fieldNoteCaption;

            #endregion

            #region Parent
            fieldParentYear = new PivotGridFieldBase("ParentTranDate", PivotArea.FilterArea);
            fieldParentYear.Name = "fieldParentYear";
            fieldParentYear.Caption = "Parent Year";
            fieldParentYear.GroupInterval = PivotGroupInterval.DateYear;

            fieldParentQuarter = new PivotGridFieldBase("ParentTranDate", PivotArea.FilterArea);
            fieldParentQuarter.GroupInterval = PivotGroupInterval.DateQuarter;
            fieldParentQuarter.Name = "fieldParentQuarter";
            fieldParentQuarter.Caption = "Parent Quarter";
            fieldParentQuarter.ValueFormat.FormatString = "Qtr {0}";
            fieldParentQuarter.ValueFormat.FormatType = DevExpress.Utils.FormatType.Custom;

            fieldParentMonth = new PivotGridFieldBase("ParentTranDate", PivotArea.FilterArea);
            fieldParentMonth.Name = "fieldParentMonth";
            fieldParentMonth.Caption = "Parent Month";
            fieldParentMonth.GroupInterval = PivotGroupInterval.DateMonth;
            fieldParentMonth.ValueFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            fieldParentMonth.ValueFormat.FormatString = "{0:MMM}";

            fieldParentDate = new PivotGridFieldBase("ParentTranDate", PivotArea.FilterArea);
            fieldParentDate.Caption = "Parent Date";
            fieldParentDate.ValueFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            fieldParentDate.ValueFormat.FormatString = "{0:dd-MMM-yy}";

            fieldParentSource = new PivotGridFieldBase("ParentSource.Name", PivotArea.FilterArea);
            fieldParentSource.Caption = "ParentSource";

            fieldParentActivity = new PivotGridFieldBase("ParentActivity.Name", PivotArea.FilterArea);
            fieldParentActivity.Caption = "ParentActivity";

            fieldParentDim_1_1 = new PivotGridFieldBase("ParentActivity.Dim_1_1", PivotArea.FilterArea);
            fieldParentDim_1_1.Caption = "ParentDim_1_1";

            fieldParentDim_1_2 = new PivotGridFieldBase("ParentActivity.Dim_1_2", PivotArea.FilterArea);
            fieldParentDim_1_2.Caption = "ParentDim_1_2";

            fieldParentDim_1_3 = new PivotGridFieldBase("ParentActivity.Dim_1_3", PivotArea.FilterArea);
            fieldParentDim_1_3.Caption = "ParentDim_1_3";

            #endregion

            PivotGridFields = new List<PivotGridFieldBase>()
            {
                fieldSnapshot,
                fieldTranYear,
                fieldTranQuarter,
                fieldTranMonth,
                fieldTranDate,
                fieldAccountCcyAmt,
                fieldCounterCcy,
                fieldFunctionalCcyAmt,
                fieldAccountCcyAmtVar,
                fieldFunctionalCcyAmtVar,
                fieldCounterCcyAmtVar,
                fieldSource,
                fieldActivity,
                fieldDim_1_1,
                fieldDim_1_2,
                fieldDim_1_3,
                fieldFix,
                fieldFixType,
                fieldFixRank,
                fieldFixerYear,
                fieldFixerQuarter,
                fieldFixerMonth,
                fieldFixerDate,
                fieldParentYear,
                fieldParentQuarter,
                fieldParentMonth,
                fieldParentDate,
                fieldParentSource,
                fieldParentActivity,
                fieldParentDim_1_1,
                fieldParentDim_1_2,
                fieldParentDim_1_3,
                fieldNote
            };
        }

        /// <summary>
        /// Change the PivotArea of fields depending on the LayoutName specified
        /// </summary>
        protected void UseAppLayout(string layoutName)
        {
            if (!Constants.CashFlowPivotLayouts.Contains(layoutName))
                throw new ArgumentOutOfRangeException("Layout '" + layoutName + "' is not a valid application-defined layout.");

            // reset layout
            foreach (var field in PivotGridFields)
            {
                if (field == null) continue;
                field.Area = PivotArea.FilterArea;
            }

            switch (layoutName)
            {
                case Constants.CashFlowPivotLayoutFixForecast:
                    fieldParentMonth.Area = PivotArea.RowArea;
                    fieldParentSource.Area = PivotArea.RowArea;
                    fieldParentDate.Area = PivotArea.RowArea;
                    fieldFix.Area = PivotArea.ColumnArea;
                    fieldFunctionalCcyAmt.Area = PivotArea.DataArea;
                    break;
                case Constants.CashFlowPivotLayoutMonthly:
                    fieldDim_1_1.Area = PivotArea.RowArea;
                    fieldDim_1_2.Area = PivotArea.RowArea;
                    fieldDim_1_3.Area = PivotArea.RowArea;
                    fieldTranYear.Area = PivotArea.ColumnArea;
                    fieldTranMonth.Area = PivotArea.ColumnArea;
                    fieldFunctionalCcyAmt.Area = PivotArea.DataArea;
                    break;
                case Constants.CashFlowPivotLayoutMonthlyVariance:
                    fieldActivity.Area = PivotArea.RowArea;
                    fieldSnapshot.Area = PivotArea.ColumnArea;
                    fieldFunctionalCcyAmtVar.Area = PivotArea.DataArea;
                    break;
            }
        }

        protected UserViewLayout GetUserDefaultLayout()
        {
            var setting = GetUserDefaultSettings();
            return setting.UserViewLayout;
        }

        protected void RestoreAppLayouts()
        {
            CreatePivotGridFields();
            foreach (var layoutName in Constants.CashFlowPivotLayouts)
            {
                RestoreAppLayout(layoutName);
            }
        }

        private UserViewLayout RestoreAppLayout(string appLayoutName)
        {
            var objSpace = ObjectSpace;

            // set user's default layout if it's not set
            UseAppLayout(appLayoutName);
            SyncPivotGridFieldsToControl();

            // get app layout
            var layoutObj = ObjectSpace.FindObject<UserViewLayout>(CriteriaOperator.Parse(
                "LayoutName = ? And LayoutType = ?", appLayoutName, UserViewLayoutType.CashFlowReportPivotGrid));
            if (layoutObj == null)
            {
                // create app layout if it does not exist (because the layout name was not found)
                layoutObj = objSpace.CreateObject<UserViewLayout>();
                layoutObj.LayoutName = appLayoutName;
                layoutObj.LayoutType = UserViewLayoutType.CashFlowReportPivotGrid;
            }
            // persist app-layout to the datastore
            SavePivotGridLayout(layoutObj);

            return layoutObj;
        }

        /// <summary>
        /// Get the default application-defined layout object as defined by SetOfBooks.
        /// </summary>
        protected void RestoreAppDefaultLayout()
        {
            // set user's default layout if it's not set
            var appLayoutName = SetOfBooks.CachedInstance.CashFlowPivotLayoutName;
            CreatePivotGridFields();
            RestoreAppLayout(appLayoutName);
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            
            // get default layout for the user
            var userLayout = GetUserDefaultSettings().UserViewLayout;
            if (userLayout != null)
            {
                CreatePivotGridFields();
                LoadPivotGridLayout(userLayout);
                return;
            }
            RestoreAppDefaultLayout();
            ObjectSpace.CommitChanges();
        }

        protected PivotCellEventArgs DoubleClickPivotCellEventArgs;
        protected virtual void RefreshNoteField() { }

        protected bool IsVarianceField(PivotGridFieldBase field)
        {
            return field.Name == fieldAccountCcyAmtVar.Name
                || field.Name == fieldCounterCcyAmtVar.Name
                || field.Name == fieldFunctionalCcyAmtVar.Name;
        }

        protected CashFlowNote GetNoteObject(PivotCellBaseEventArgs e, IObjectSpace objSpace, 
            bool autoCreate)
        {
            if (e.DataField.Caption != fieldNoteCaption)
                throw new InvalidOperationException("DataField must be of type CashFlowNote");

            var rowFields = e.GetRowFields();
            var colFields = e.GetColumnFields();
            string criteriaText = "";

            #region Add Time Criteria
            int year = 0;
            int month = 0;
            foreach (var field in colFields)
            {
                if (field.Caption == "Tran Month")
                {
                    month = (int)e.GetFieldValue(field);
                }
                else if (field.Caption == "Tran Year")
                {
                    year = (int)e.GetFieldValue(field);
                }
            }

            if (year != 0)
                AddCriteriaText(ref criteriaText, "TranYear = " + year, "And");
            else
                AddCriteriaText(ref criteriaText, "TranYear Is Null", "And");
            if (month != 0)
                AddCriteriaText(ref criteriaText, "TranMonth = " + month, "And");
            else
                AddCriteriaText(ref criteriaText, "TranMonth Is Null", "And");
            #endregion

            #region Add Activity Criteria
            ActivityTag dim_1_1 = null;
            ActivityTag dim_1_2 = null;
            ActivityTag dim_1_3 = null;
            foreach (var field in rowFields)
            {
                if (field.Caption == "Dim_1_1")
                {
                    dim_1_1 = (ActivityTag)e.GetFieldValue(field);
                }
                else if (field.Caption == "Dim_1_2")
                {
                    dim_1_2 = (ActivityTag)e.GetFieldValue(field);
                }
                else if (field.Caption == "Dim_1_3")
                {
                    dim_1_3 = (ActivityTag)e.GetFieldValue(field);
                }
            }

            if (dim_1_1 != null)
                AddCriteriaText(ref criteriaText, string.Format("Dim_1_1.Text = '{0}'", dim_1_1));
            else
                AddCriteriaText(ref criteriaText, "Dim_1_1 Is Null");
            if (dim_1_2 != null)
                AddCriteriaText(ref criteriaText, string.Format("Dim_1_2.Text = '{0}'", dim_1_2));
            else
                AddCriteriaText(ref criteriaText, "Dim_1_2 Is Null");
            if (dim_1_3 != null)
                AddCriteriaText(ref criteriaText, string.Format("Dim_1_3.Text = '{0}'", dim_1_3));
            else
                AddCriteriaText(ref criteriaText, "Dim_1_3 Is Null");
            #endregion

            var criteria = CriteriaOperator.Parse(criteriaText);
            var noteObj = objSpace.FindObject<CashFlowNote>(criteria);
            if (noteObj == null && autoCreate)
            {
                noteObj = objSpace.CreateObject<CashFlowNote>();
                noteObj.TranYear = year;
                noteObj.TranMonth = month;
                noteObj.Dim_1_1 = dim_1_1;
                noteObj.Dim_1_2 = dim_1_2;
                noteObj.Dim_1_3 = dim_1_3;
                noteObj.Save();
                objSpace.CommitChanges();
            }
            return noteObj;
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.parametrizedAction1 = new DevExpress.ExpressApp.Actions.ParametrizedAction(this.components);
            // 
            // parametrizedAction1
            // 
            this.parametrizedAction1.Caption = null;
            this.parametrizedAction1.ConfirmationMessage = null;
            this.parametrizedAction1.Id = "697b962f-a7ec-4141-8ad6-bb16880dce3a";
            this.parametrizedAction1.NullValuePrompt = null;
            this.parametrizedAction1.ShortCaption = null;
            this.parametrizedAction1.ToolTip = null;

        }

        private System.ComponentModel.IContainer components;

    }
}
