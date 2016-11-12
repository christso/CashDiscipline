using Excel;
using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Clients
{
    public static class DataObjectFactory
    {
        public static DataTable CreateTableFromExcelXml(string filePath, string sheetName)
        {
            using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                excelReader.IsFirstRowAsColumnNames = true;
                DataSet xlsDataSet = excelReader.AsDataSet();
                excelReader.Close();
                return xlsDataSet.Tables[sheetName];
            }
        }

        public static IDataReader CreateReaderFromCsv(string filePath)
        {
            FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return new CachedCsvReader(new StreamReader(stream), true);
        }
    }
}
