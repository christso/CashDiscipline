using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.ServiceLib
{
    public class ImportBankStmtServiceParam
    {

        private bool _AnzEnabled;
        public bool AnzEnabled
        {
            get
            {
                return _AnzEnabled;
            }
            set
            {
                _AnzEnabled = value;
            }
        }

        private string _AnzFilePath;
        public string AnzFilePath
        {
            get
            {
                return _AnzFilePath;
            }
            set
            {
                _AnzFilePath = value;
            }
        }

        private bool _WbcEnabled;
        public bool WbcEnabled
        {
            get
            {
                return _WbcEnabled;
            }
            set
            {
                _WbcEnabled = value;
            }
        }

        private string _WbcFilePath;
        public string WbcFilePath
        {
            get
            {
                return _WbcFilePath;
            }
            set
            {
                _WbcFilePath = value;
            }
        }


        private bool _CbaBosEnabled;
        public bool CbaBosEnabled
        {
            get
            {
                return _CbaBosEnabled;
            }
            set
            {
                _CbaBosEnabled = value;
            }
        }

        private string _CbaBosFilePath;
        public string CbaBosFilePath
        {
            get
            {
                return _CbaBosFilePath;
            }
            set
            {
                _CbaBosFilePath = value;
            }
        }

        private bool _CbaOpEnabled;
        public bool CbaOpEnabled
        {
            get
            {
                return _CbaOpEnabled;
            }
            set
            {
                _CbaOpEnabled = value;
            }
        }

        private string _CbaOpFilePath;
        public string CbaOpFilePath
        {
            get
            {
                return _CbaOpFilePath;
            }
            set
            {
                _CbaOpFilePath = value;
            }
        }
    }
}
