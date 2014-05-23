﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.HelperClasses.IO
{
    public class ImportErrorInfo
    {
        public Exception ExceptionInfo;
        public Type ColumnType;
        public string ColumnName;
        public long LineNumber;
        public string OrigValue;
    }
}