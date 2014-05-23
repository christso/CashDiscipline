using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.HelperClasses.IO
{
    public struct ImportErrorInfoKey : IEquatable<ImportErrorInfoKey>
    {
        private readonly int lineNumber;
        private readonly string columnName;

        public int LineNumber { get { return lineNumber; } }
        public string ColumnName { get { return columnName; } }

        public ImportErrorInfoKey(int lineNumber, string columnName)
        {
            this.lineNumber = lineNumber;
            this.columnName = columnName;
        }

        public override int GetHashCode()
        {
            int somePrimeNumber = 37;
            return lineNumber.GetHashCode() * somePrimeNumber + columnName.GetHashCode();
        }

        public override bool Equals(object other)
        {
            return other is ImportErrorInfoKey ? Equals((ImportErrorInfoKey)other) : false;
        }

        public bool Equals(ImportErrorInfoKey other)
        {
            return lineNumber == other.lineNumber &&
                   columnName == other.columnName;
        }
    }
}
