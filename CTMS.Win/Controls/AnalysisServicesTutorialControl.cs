using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CTMS.Module.Editors;
using CTMS.Module.Win.Editors;
using DevExpress.XtraPivotGrid;
using System.Diagnostics;

namespace CTMS.Win.Controls
{
    public partial class AnalysisServicesTutorialControl : DevExpress.XtraEditors.XtraUserControl, IXpoSessionAwareControl, IMasterUserControl
    {
        public AnalysisServicesTutorialControl()
        {
            InitializeComponent();
        }

        public void UpdateDataSource(DevExpress.Xpo.Session session)
        {
            return;
        }

        public List<object> UserControls
        {
            get
            {
                return new List<object>() { pivotGridControl1 };
            }
        }
    }
}
