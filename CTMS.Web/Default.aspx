<%@ Page Language="C#" AutoEventWireup="true" Inherits="Default" EnableViewState="false"
    ValidateRequest="false" CodeBehind="Default.aspx.cs" %>

<%@ Register Assembly="DevExpress.Web.v15.2, Version=15.2.7.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.ExpressApp.Web.v15.2" Namespace="DevExpress.ExpressApp.Web.Templates"
    TagPrefix="cc3" %>
<%@ Register Assembly="DevExpress.ExpressApp.Web.v15.2" Namespace="DevExpress.ExpressApp.Web.Controls"
    TagPrefix="cc4" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Main Page</title>
    <meta http-equiv="Expires" content="0" />
</head>
<body class="VerticalTemplate">
    <form id="form2" runat="server">
    <cc3:XafUpdatePanel ID="UPPopupWindowControl" runat="server">
        <cc4:XafPopupWindowControl runat="server" ID="PopupWindowControl" />
    </cc3:XafUpdatePanel>
    <cc4:ASPxProgressControl ID="ProgressControl" runat="server" />
    <div runat="server" id="Content" />
    <!--Restore focus after a callback operation caused by ImmediatePostDataAttribute-->
    <dx:ASPxGlobalEvents ID="ASPxGlobalEvents1" runat="server">
        <ClientSideEvents EndCallback="function(s, e) {
                    if (window.lastFocusedEditor !== undefined &amp;&amp; window.lastFocusedEditor !== null) {
                        window.lastFocusedEditor.Focus();
                    }
}" />
    </dx:ASPxGlobalEvents>
    </form>
</body>
</html>
