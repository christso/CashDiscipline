using CashDiscipline.Module.Test;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Win.Editors;
using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.Paste.Win;

namespace CashDiscipline.Module.Win.Test
{
    public class WinTestViewController : TestViewController
    {
        protected override void PasteTest()
        {
            #region Copy
            // TODO: expose through ICopiedText
            var copiedText = new CopiedText();

            var copyParser = new CopyParser(copiedText);
            var parsedArray = copyParser.ToArray();

            #endregion

            #region Paste

            var listView = (ListView)View;
            var listEditor = listView.Editor as GridListEditor;

            //var columns1 = listView.Model.Columns;
            //var columns2 = listEditor.Model.Columns;

            var paster = new NewRowPasteProcessor(copyParser, listView);
            paster.Process();

            #endregion
        }

        private class CopiedText : ICopiedText
        {
            public string Data
            {
                get
                {
                    return @"88f769ab-ef5b-41aa-a2c9-0020d23d653a	Current	12-May-16	VHA ANZ GBP";
                }
            }
        }



    }
}
