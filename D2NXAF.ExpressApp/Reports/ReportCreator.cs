using DevExpress.ExpressApp;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.HelperClasses.ExpressApp
{
    public abstract class ExcelReportCreator
    {
        public ExcelReportCreator()
        {
        }

        public ExcelReportCreator(XafApplication app, ExcelPackage package)
        {
            SetEnv(app, package);
        }

        public void SetEnv(XafApplication app, ExcelPackage package)
        {
            this.Application = app;
            this.Package = package;
        }

        public abstract void Execute();

        protected string TargetName;
        protected XafApplication Application;
        protected ExcelPackage Package;
    }
}
