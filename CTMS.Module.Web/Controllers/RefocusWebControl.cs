using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Web;
using System.Collections.Generic;
using DevExpress.Web.ASPxClasses;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Web.Editors;
using DevExpress.ExpressApp.Web.Editors.ASPx;

namespace CTMS.Module.Web.Controllers
{
    /// <summary>
    /// Restore focus after a callback operation caused by ImmediatePostDataAttribute
    /// </summary>
    public class RefocusWebControl : ViewController<DetailView>
    {
        protected override void OnActivated()
        {
            base.OnActivated();
            if (View.ViewEditMode == ViewEditMode.Edit)
            {
                foreach (WebPropertyEditor item in View.GetItems<WebPropertyEditor>())
                {
                    if (item.Editor != null)
                    {
                        AddClientSideFunctionality(item);
                    }
                    else
                    {
                        item.ControlCreated += (s, e) =>
                        {
                            AddClientSideFunctionality(s);
                        };
                    }
                }
            }
        }
        protected void AddClientSideFunctionality(object element)
        {
            if (element is ASPxLookupPropertyEditor)
            {
                if (((ASPxLookupPropertyEditor)element).DropDownEdit != null
                && ((ASPxLookupPropertyEditor)element).FindEdit != null)
                {
                    AddClientSideFunctionalityCore(((ASPxLookupPropertyEditor)element).DropDownEdit.DropDown);
                    AddClientSideFunctionalityCore(((ASPxLookupPropertyEditor)element).FindEdit.TextBox);
                }
            }
            else if (element is WebPropertyEditor)
            {
                AddClientSideFunctionalityCore(((WebPropertyEditor)element).Editor as ASPxWebControl);
            }
        }
        private void AddClientSideFunctionalityCore(ASPxWebControl dxControl)
        {
            if (dxControl != null)
            {
                EventHandler loadEventHandler = (s, e) =>
                {
                    ASPxWebControl control = (ASPxWebControl)s;
                    AddClientSideEventHandlerSafe(control, "Init", "if (s.focused)window.lastFocusedEditor = s;");
                    AddClientSideEventHandlerSafe(control, "GotFocus", "window.lastFocusedEditor = s;");
                };
                EventHandler disposedEventHandler = null;
                disposedEventHandler = (s, e) =>
                {
                    ASPxWebControl control = (ASPxWebControl)s;
                    control.Disposed -= disposedEventHandler;
                    control.Load -= loadEventHandler;
                };
                dxControl.Disposed += disposedEventHandler;
                dxControl.Load += loadEventHandler;
            }
        }
        private static void AddClientSideEventHandlerSafe(ASPxWebControl control, string eventName, string handler)
        {
            string format = @"function(s,e){{{0}}}";
            string existingHandler = control.GetClientSideEventHandler(eventName);
            if (string.IsNullOrEmpty(existingHandler))
            {
                control.SetClientSideEventHandler(eventName, string.Format(format, handler));
            }
            else
            {
                existingHandler = String.Format("{0}{1}\r\n}}", existingHandler.Substring(0, existingHandler.LastIndexOf('}')), handler);
                control.SetClientSideEventHandler(eventName, existingHandler);
            }

        }
    }
}
