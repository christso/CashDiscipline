using DevExpress.ExpressApp.Utils;
using DevExpress.Xpo;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.HelperClasses.ExpressApp
{
    public class ExcelReportHelper
    {
        public static void CopyObjectsToWorksheet(Session session, System.Collections.ICollection objs, ExcelWorksheet ws)
        {
            // clear existing values
            ws.Cells[ws.Dimension.Start.Row, ws.Dimension.Start.Column, ws.Dimension.End.Row, ws.Dimension.End.Column].Clear();      

            // headings
            Type objType = null;
            foreach (var obj in objs)
            {
                objType = obj.GetType();
            }
            if (objType == null) return;

            var classInfo = session.GetClassInfo(objType);

            int h = 1;
            foreach (var memberInfo in classInfo.Members
                .Where(x => x.HasAttribute(typeof(ExcelReportFieldAttribute))))
            {
                ws.Cells[1, h++].Value = string.IsNullOrEmpty(memberInfo.DisplayName) ?
                    CaptionHelper.ConvertCompoundName(memberInfo.Name)
                    : memberInfo.DisplayName;
            }

            // data
            int r = 2;
            foreach (IXPObject obj in objs)
            {
                int c = 1;
                foreach (var memberInfo in obj.ClassInfo.Members
                    .Where(x => x.HasAttribute(typeof(ExcelReportFieldAttribute))))
                {
                    ws.Cells[r, c++].Value = memberInfo.GetValue(obj);
                }
                r++;
            }
        }
    }
}
