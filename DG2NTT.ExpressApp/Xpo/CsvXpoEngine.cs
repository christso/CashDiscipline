using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.ExpressApp;
using DG2NTT.Utilities;
using CTMS.Module.HelperClasses.IO;
using System.Threading;
using CTMS.Module.ParamObjects.Import;

namespace CTMS.Module.HelperClasses.Xpo
{
    
    public class CsvXpoEngine : XpoImportEngine
    {

        public CsvXpoEngine(XafApplication application) : base(application)
        {
        }

        public CancellationTokenSource CancellationTokenSource = null;
        public ImportErrorInfo ErrorInfo = null;

        public void ImportCsvFile(MemoryStream stream, XPObjectSpace objSpace, ITypeInfo objTypeInfo, 
            bool hasHeaders = true, ImportActionType actionType = ImportActionType.Insert, FieldMaps fieldMaps = null)
        {
            XpObjectsNotFound.Clear();
            ErrorInfo = null;

            using (var csv = new CsvReader(new StreamReader(stream), hasHeaders))
            {
                #region Member Name Mappings

                string[] headers = csv.GetFieldHeaders();

                // add default aliases which are the same as the csv field name
                if (fieldMaps == null)
                {
                    fieldMaps = new FieldMaps();
                    foreach (var header in headers)
                    {
                        if (!fieldMaps.Source.ContainsKey(header))
                            fieldMaps.Add(header, header);
                    }
                }

                List<IMemberInfo> members = new List<IMemberInfo>();

                string keyField = null;

                if (actionType == ImportActionType.Insert)
                {
                    foreach (var member in objTypeInfo.Members)
                    {
                        if (fieldMaps.Target.ContainsKey(member.Name) && !member.IsKey)
                            members.Add(member);
                    }
                }
                else if (actionType == ImportActionType.Update)
                {
                    keyField = fieldMaps.Source[headers[0]].TargetField;
                    foreach (var member in objTypeInfo.Members)
                    {
                        if (fieldMaps.Target.ContainsKey(member.Name) && !member.IsKey
                            && keyField != member.Name)
                            members.Add(member);
                    }
                }

                foreach (var targetKey in fieldMaps.Target.Keys)
                {
                    IMemberInfo member = members.FirstOrDefault(c => c.Name == targetKey);
                    if (member == null)
                        throw new InvalidDataException(string.Format(
                            "Member '{0}' is not a valid member name",
                            targetKey));
                }
                #endregion

                #region Cache Objects
                var targetFieldNames = new string[fieldMaps.Target.Count];
                int i = 0;
                foreach (string field in fieldMaps.Target.Keys)
                {
                    targetFieldNames[i] = field;
                    i++;
                }
                if (Options.CacheObjects)
                {
                    CacheXpObjects(objTypeInfo, targetFieldNames, objSpace);
                }
                if (members == null)
                    throw new InvalidDataException("No members were found");
                #endregion

                while (csv.ReadNextRecord())
                {
                    if (CancellationTokenSource != null && CancellationTokenSource.IsCancellationRequested)
                        CancellationTokenSource.Token.ThrowIfCancellationRequested();

                    #region Target Object
                    IXPObject targetObject = null;
                    if (actionType == ImportActionType.Insert)
                    {
                        targetObject = (IXPObject)Activator.CreateInstance(objTypeInfo.Type, objSpace.Session);
                    }
                    else if (actionType == ImportActionType.Update)
                    {
                        targetObject = (IXPObject)objSpace.FindObject(objTypeInfo.Type, CriteriaOperator.Parse(keyField + " = ?", csv[0]));
                        if (targetObject == null)
                        {
                            var ex = new InvalidDataException(string.Format("No object matches criteria {0} = {1}. Try removing the object from the import file.",
                                keyField, csv[0]));
                            ErrorInfo = new ImportErrorInfo()
                            {
                                LineNumber = csv.CurrentRecordIndex + 1 + Convert.ToInt32(hasHeaders),
                                ExceptionInfo = ex
                            };
                            throw ex;
                        }
                    }
                    #endregion
                    #region Member Values
                    foreach (var memberInfo in members)
                    {
                        try
                        {
                            SetMemberValue(targetObject, memberInfo, csv[fieldMaps.Target[memberInfo.Name].SourceField]);
                        }
                        catch (Exception ex)
                        {
                            ErrorInfo = new ImportErrorInfo()
                            {
                                ColumnName = memberInfo.Name,
                                ColumnType = memberInfo.MemberType,
                                LineNumber = csv.CurrentRecordIndex + 1 + Convert.ToInt32(hasHeaders),
                                ExceptionInfo = ex,
                                OrigValue = csv[fieldMaps.Target[memberInfo.Name].SourceField]
                            };
                            throw new ConvertException(ErrorInfo.OrigValue, ErrorInfo.ColumnType);
                        }
                    }
                    objSpace.Session.Save(targetObject);
                    #endregion
                }
                objSpace.Session.CommitTransaction();
            }
        }
        public void ImportCsvFileUpdates(MemoryStream stream, XPObjectSpace objSpace, ITypeInfo objTypeInfo, bool hasHeaders = true, FieldMaps fieldMaps = null)
        {
            ImportCsvFile(stream, objSpace, objTypeInfo, hasHeaders, ImportActionType.Update, fieldMaps);
        }

        public void ImportCsvFileInserts(MemoryStream stream, XPObjectSpace objSpace, ITypeInfo objTypeInfo, bool hasHeaders = true, FieldMaps fieldMaps = null)
        {
            ImportCsvFile(stream, objSpace, objTypeInfo, hasHeaders, ImportActionType.Insert, fieldMaps);
        }
    }
}
