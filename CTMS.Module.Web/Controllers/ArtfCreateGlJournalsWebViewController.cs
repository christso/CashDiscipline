using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.ExpressApp.Web.SystemModule;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Actions;
using CTMS.Module.BusinessObjects.Artf;
using System.Web;
using System.IO;
using System.Xml;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using CTMS.Module.ParamObjects.Artf;

namespace CTMS.Module.Web.Controllers
{
    public class ArtfCreateGlJournalsWebViewController : ViewController
    {
        private ArtfCreateGlJournalsParam currentObject;
        private SimpleAction createAction;
        public ArtfCreateGlJournalsWebViewController()
        {
            InitializeComponent();
            // TODO: add DetailViewActions to actioncontainermapping
            createAction = new SimpleAction(this, "SimpleAction", "DetailViewActions");
            createAction.Caption = "Create";
            createAction.Execute += simpleAction_Execute;
        }

        private void InitializeComponent()
        {
            // 
            // ArtfCreateGlJournalsViewController
            // 
            this.TargetObjectType = typeof(CTMS.Module.ParamObjects.Artf.ArtfCreateGlJournalsParam);
            this.TargetViewType = DevExpress.ExpressApp.ViewType.DetailView;
            this.TypeOfView = typeof(DevExpress.ExpressApp.DetailView);

        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            currentObject = (ArtfCreateGlJournalsParam)View.CurrentObject;
        }

        void simpleAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            // TODO: open new window

            LogTrace("The 'SimpleAction' is executed");

            CreateFile();
            TransmitFile();


        }

        // web only
        private void CreateFile()
        {
            string outputFolder = @"~/SavedFiles/";
            string outputPath = HttpContext.Current.Server.MapPath(outputFolder + @"sample1.xlsx");
            FileInfo newFile = new FileInfo(outputPath);
            if (newFile.Exists)
            {
                newFile.Delete();  // ensures we create a new workbook
                newFile = new FileInfo(outputPath);
            }
            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Output");

                #region Sample Data
                //Add the headers
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Product";
                worksheet.Cells[1, 3].Value = "Quantity";
                worksheet.Cells[1, 4].Value = "Price";
                worksheet.Cells[1, 5].Value = "Value";

                //Add some items...
                worksheet.Cells["A2"].Value = 12001;
                worksheet.Cells["B2"].Value = "Nails";
                worksheet.Cells["C2"].Value = 37;
                worksheet.Cells["D2"].Value = 3.99;

                worksheet.Cells["A3"].Value = 12002;
                worksheet.Cells["B3"].Value = "Hammer";
                worksheet.Cells["C3"].Value = 5;
                worksheet.Cells["D3"].Value = 12.10;

                worksheet.Cells["A4"].Value = 12003;
                worksheet.Cells["B4"].Value = "Saw";
                worksheet.Cells["C4"].Value = 12;
                worksheet.Cells["D4"].Value = 15.37;
                #endregion

                package.Save();
            }
        }
        // web only
        private void TransmitFile()
        {
            string filename = @"sample1.xlsx";
            string folder = @"~/SavedFiles/";
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ContentType = "application/octet-stream";
            HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + filename);
            HttpContext.Current.Response.TransmitFile(HttpContext.Current.Server.MapPath(folder + filename));
            HttpContext.Current.Response.End();
        }
        private void LogTrace(string message)
        {
            currentObject.Log = message + "\r\n" + currentObject.Log;
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Hide Save, New and Delete actions
            Frame.GetController<ModificationsController>().Active["HideModificationsController"] = false;

            // the built-in Export Action is executed on postback, because postback is required to transmit a file to client. 
            // It is impossible to call Response.End() and Response.Complete() on callback
            createAction.Model.SetValue<bool>("IsPostBackRequired", true);
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            Frame.GetController<ModificationsController>().Active["HideModificationsController"] = true;
        }
    }
}
