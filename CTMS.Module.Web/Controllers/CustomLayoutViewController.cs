using System;
using System.Collections.Generic;
using System.Text;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Web.Layout;
using System.Web.UI;
using DevExpress.ExpressApp.Web;
using System.Web;
using DevExpress.ExpressApp.SystemModule;
using CTMS.Module.Web.Layout;

namespace CTMS.Module.Web.Controllers
{
    // For more information on Controllers and their life cycle, check out the http://documentation.devexpress.com/#Xaf/CustomDocument2621 and http://documentation.devexpress.com/#Xaf/CustomDocument3118 help articles.
    // For more information on Controllers and their life cycle, check out the http://documentation.devexpress.com/#Xaf/CustomDocument2621 and http://documentation.devexpress.com/#Xaf/CustomDocument3118 help articles.
    public partial class CustomLayoutViewController : ViewController
    {
        // Use this to do something when a Controller is instantiated (do not execute heavy operations here!).
        public CustomLayoutViewController()
        {
            InitializeComponent();
            RegisterActions(components);
        }

        private void View_ControlsCreating(object sender, EventArgs e)
        {
            UpdateLayoutManagerTemplates();
        }
        private void UpdateLayoutManagerTemplates()
        {
            LayoutBaseTemplate itemTemplate = new CustomLayoutItemTemplate();
            //LayoutBaseTemplate itemTemplate = new LayoutItemTemplate();
            WebLayoutManager layoutManager = (WebLayoutManager)((DetailView)View).LayoutManager;
            layoutManager.LayoutItemTemplate = itemTemplate;
            LayoutBaseTemplate groupTemplate = new CustomLayoutGroupTemplate();
            layoutManager.LayoutGroupTemplate = groupTemplate;
            //LayoutBaseTemplate tabbedGroupTemplate1 = new CustomLayoutTabbedGroupTemplate();
            LayoutBaseTemplate tabbedGroupTemplate1 = new TabbedGroupTemplate();
            layoutManager.TabbedGroupTemplate = tabbedGroupTemplate1;
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            View.ControlsCreating += new EventHandler(View_ControlsCreating);
        }
        protected override void OnDeactivated()
        {
            View.ControlsCreating -= new EventHandler(View_ControlsCreating);
            base.OnDeactivated();
        }
    }
}
