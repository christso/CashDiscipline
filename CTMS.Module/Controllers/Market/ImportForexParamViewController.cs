using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTMS.Module.ParamObjects.Import;
using DevExpress.ExpressApp.SystemModule;
using System.Diagnostics;
using System.Threading;
using DevExpress.ExpressApp.Editors;
using CTMS.Module.BusinessObjects.Market;
using CTMS.Module.BusinessObjects;
using DevExpress.Data.Filtering;
using System.IO;
using DevExpress.ExpressApp.Security.Strategy;

using DevExpress.ExpressApp.Xpo;
using SDP.ParserUtils;
using D2NXAF.Utils;

namespace CTMS.Module.Controllers.Market
{
    public class ImportForexParamViewController : ViewControllerEx
    {
        public ImportForexParamViewController()
        {
            TargetObjectType = typeof(ImportForexRatesParam);
            TargetViewType = ViewType.DetailView;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ((DetailView)View).ViewEditMode = ViewEditMode.Edit;
            var dc = Frame.GetController<DialogController>();
            if (dc != null)
                dc.AcceptAction.Execute += AcceptAction_Execute;
        }

        void AcceptAction_Execute(object sender, DevExpress.ExpressApp.Actions.SimpleActionExecuteEventArgs e)
        {
            var paramObj = View.CurrentObject as ImportForexRatesParam;
            var byteArray = paramObj.File.Content;
            var stream = new MemoryStream(byteArray);
            var objSpace = Application.CreateObjectSpace();

            // reader must be instantiated on main thread or you get null exception
            var reader = new FlatFileReader(stream, Encoding.GetEncoding("iso-8859-1"));
            var parser = new WbcFxRateParser();

            Action job = new Action(() =>
            {
                string dateText;
                DateTime convDate = new DateTime();
                while (reader.ParseLine())
                {
                    //get dates
                    if (reader.CurrentLineNumber == 3)
                    {
                        //ensure that length argument does not exceed the line length
                        dateText = reader.CurrentLine.Substring(60, 20 + reader.CurrentLine.Length - 80);
                        if (!DateTime.TryParse(dateText, out convDate))
                            throw new InvalidDataException(string.Format("Invalid date format '{0}'", dateText));
                    }
              
                    if (reader.CurrentLineNumber < 11) continue;
                    if (reader.CurrentLineNumber > 55) break;
                    parser.Parse(reader.CurrentLine);
                    if (!parser.IsValid()) continue;

                    var forexRate = objSpace.CreateObject<ForexRate>();
                    forexRate.ToCurrency = objSpace.FindObject<Currency>(CriteriaOperator.Parse(Currency.FieldNames.Name + " = ?", parser.CcyCode));
                    forexRate.ConversionDate = convDate;
                    forexRate.ConversionRate = parser.TtMidAmt;
                }
                objSpace.CommitChanges();
            });
            SubmitRequest("Import Forex Rates", job);
        }

        private class WbcFxRateParser : StringLayoutUtility
        {
            [StringLayout(35, 37)]
            public string CcyCode { get; set; }

            [StringLayout(41, 46)]
            public string TtBuy { get; set; }

            [StringLayout(50, 55)]
            public string ChqBuy { get; set; }

            [StringLayout(59, 64)]
            public string NoteBuy { get; set; }

            [StringLayout(68, 73)]
            public string TtSell { get; set; }

            public decimal TtMidAmt
            {
                get { return (Convert.ToDecimal(TtBuy) + Convert.ToDecimal(TtSell)) / 2; }
            }

            public bool IsValid()
            {
                return TtBuy.IsNumeric() && TtSell.IsNumeric() && CcyCode != null;
            }
        }
    }
}
