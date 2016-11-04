using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;

namespace CashDisicpline.ScratchPad
{
    [FixedLengthRecord()]
    public class ApPoReceiptBalanceFixedWidth
    {
        [FieldFixedLength(16)]
        [FieldTrim(TrimMode.Right)]
        public int CustId;

        [FieldFixedLength(16)]
        [FieldTrim(TrimMode.Right)]
        public string Name;

        [FieldFixedLength(16)]
        [FieldTrim(TrimMode.Right)]
        public decimal Balance;
        
    }

    public class FixedWidthImportTest
    {
        public void Execute()
        {
            var filePath = @"\\localhost\VHA Import\SSIS\AP_Accrual_PO_Data\VHA_AP_and_PO_Accrual_Reconcil_021116.txt";

        }
    }
}
