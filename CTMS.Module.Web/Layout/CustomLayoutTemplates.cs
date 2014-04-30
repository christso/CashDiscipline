using DevExpress.ExpressApp.Web;
using DevExpress.ExpressApp.Web.Layout;
using DevExpress.ExpressApp.Web.SystemModule;
using DevExpress.Web.ASPxRoundPanel;
using DevExpress.Web.ASPxTabControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DevExpress.Web.ASPxEditors;
using DevExpress.Web.ASPxPanel;
using System.Diagnostics;
using CTMS.Module.Model;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Validation;

namespace CTMS.Module.Web.Layout
{
    public class CustomerHeaderTemplate : ITemplate
    {
        private string _caption = "N/A";
        private ASPxPanel _panel;

        public CustomerHeaderTemplate(string caption, ASPxPanel panel)
        {
            _caption = caption;
            _panel = panel;
        }

        void ITemplate.InstantiateIn(Control container)
        {
            Table table = new Table();
            table.CellPadding = 0;
            table.CellSpacing = 0;
            table.Rows.Add(new TableRow());
            table.Rows[0].Cells.Add(new TableCell());
            table.Rows[0].Cells.Add(new TableCell());

            // TODO: move to JS file <ClientSideEvents Click="OnExpandCollapseButtonClick" />
            ASPxButton btnExpandCollapse = new ASPxButton();
            btnExpandCollapse.Text = "-";
            btnExpandCollapse.AllowFocus = false;
            btnExpandCollapse.AutoPostBack = false;
            btnExpandCollapse.Width = Unit.Pixel(20);
            btnExpandCollapse.FocusRectPaddings.Padding = Unit.Parse("0");

            btnExpandCollapse.ClientSideEvents.Click = "function (s,e) { " +
                "var isVisible = " + _panel.ClientInstanceName + ".GetVisible();\n" +
                "s.SetText(isVisible ? '+' : '-');\n" +
                _panel.ClientInstanceName + ".SetVisible(!isVisible);\n" +
                "}";

            table.Rows[0].Cells[0].Controls.Add(new LiteralControl(_caption));
            table.Rows[0].Cells[0].Style["white-space"] = "nowrap";
            
            table.Rows[0].Cells[1].Controls.Add(btnExpandCollapse);
            table.Rows[0].Cells[1].Style["width"] = "1%";
            table.Rows[0].Cells[1].Style["padding-left"] = "5px";

            container.Controls.Add(table);
        }
    }

    public class CustomLayoutItemTemplate : LayoutItemTemplate
    {
        protected override Control CreateCaptionControl(LayoutItemTemplateContainer templateContainer)
        {
            Control baseControl = base.CreateCaptionControl(templateContainer);

            string icon = GetIcon(templateContainer);
            if (!string.IsNullOrEmpty(icon))
            {
                // TODO: specify character for mandatory marker
                return CreateItemIconTable(baseControl, icon);   
            }
            else
            {
                return baseControl;
            }
        }

        private static string GetIcon(LayoutItemTemplateContainer templateContainer)
        {
            // LayoutItem ItemIcon

            var modelLayoutItemIcon = templateContainer.Model as IModelLayoutItemIcon;

            if (modelLayoutItemIcon != null && !string.IsNullOrEmpty(modelLayoutItemIcon.ItemIcon))
            {
                return modelLayoutItemIcon.ItemIcon;
            }

            // Member ItemIcon

            IModelMember modelMember = (templateContainer.Model.ViewItem as IModelPropertyEditor).ModelMember;
            IModelIcon modelIcon = modelMember as IModelIcon;
            if (modelIcon != null && !string.IsNullOrEmpty(modelIcon.ItemIcon))
            {
                return ((IModelIcon)modelMember).ItemIcon;
            }

            // RuleRequiredField

            var attr = modelMember.MemberInfo.FindAttribute<RuleRequiredFieldAttribute>();
            if (attr != null)
            {
                return "<span style=\"color:#FF0000\">*</span>";
            }

            return "";
        }

        private static Table CreateItemIconTable(Control baseControl, string itemIcon = "?")
        {
            Table table = new Table();
            table.Rows.Add(new TableRow());
            table.Rows[0].Cells.Add(new TableCell());
            table.Rows[0].Cells.Add(new TableCell());
            
            table.Rows[0].Cells[0].Controls.Add(baseControl);

            // uncomment below to use <a href /> hyperlink
            //HtmlAnchor anchor = new HtmlAnchor();
            //anchor.InnerHtml = itemIcon;
            //anchor.HRef = "#";
            //anchor.Title = string.Format("Description for the '{0}' item", baseControl);
            //table.Rows[0].Cells[1].Controls.Add(anchor);

            // icon
            // usage: [ItemIcon("<span style=\"color:#FF0000\">*</span>")]
            table.Rows[0].Cells[1].Controls.Add(new LiteralControl(itemIcon));
            return table;
        }
    }

    public class CustomLayoutGroupTemplate : LayoutGroupTemplate
    {
        protected override void LayoutContentControls(LayoutGroupTemplateContainer templateContainer, IList<Control> controlsToLayout)
        {
            LayoutGroupTemplateContainer layoutGroupTemplateContainer = (LayoutGroupTemplateContainer)templateContainer;
            
            if (layoutGroupTemplateContainer.ShowCaption)
            {
                // Outer Panel for setting the default style
                ASPxPanel outerPanel = new ASPxPanel();
                outerPanel.Style.Add(HtmlTextWriterStyle.Padding, "10px 5px 10px 5px");
                
                // Round Panel containing the Content Panel
                ASPxRoundPanel roundPanel = new ASPxRoundPanel();
                roundPanel.Width = Unit.Percentage(100);
                roundPanel.ShowHeader = layoutGroupTemplateContainer.ShowCaption;
                if (layoutGroupTemplateContainer.HasHeaderImage)
                {
                    ASPxImageHelper.SetImageProperties(roundPanel.HeaderImage, layoutGroupTemplateContainer.HeaderImageInfo);
                }

                // Content Panel
                ASPxPanel contentPanel = new ASPxPanel();
                contentPanel.ClientInstanceName = templateContainer.Model.Id + "_ContentPanel";
                
                // Set the RoundPanel Header Template
                roundPanel.HeaderTemplate = new CustomerHeaderTemplate(layoutGroupTemplateContainer.Caption, contentPanel);

                // Populate the controls
                roundPanel.Controls.Add(contentPanel);
                outerPanel.Controls.Add(roundPanel);
                templateContainer.Controls.Add(outerPanel);
                foreach (Control control in controlsToLayout)
                {
                    contentPanel.Controls.Add(control);
                }

            }
            else
            {
                foreach (Control control in controlsToLayout)
                {
                    templateContainer.Controls.Add(control);
                }
            }
        }
    }

    public class CustomLayoutTabbedGroupTemplate : TabbedGroupTemplate
    {
        protected override ASPxPageControl CreatePageControl(TabbedGroupTemplateContainer tabbedGroupTemplateContainer)
        {
            ASPxPageControl pageControl = new ASPxPageControl();
            pageControl.ID = WebIdHelper.GetCorrectedLayoutItemId(tabbedGroupTemplateContainer.Model, "", "_pg");
            pageControl.TabPosition = TabPosition.Left;
            pageControl.Width = Unit.Percentage(100);
            pageControl.ContentStyle.Paddings.Padding = Unit.Pixel(10);
            pageControl.ContentStyle.CssClass = "TabControlContent";
            return pageControl;
        }
    }
}
