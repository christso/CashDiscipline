<%@ Control Language="C#" CodeBehind="LogonTemplateContent1.ascx.cs" ClassName="LogonTemplateContent1"
    CompilationMode="Auto" Inherits="CTMS.Web.LogonTemplateContent1" AutoEventWireup="True" %>
<%@ Register Assembly="DevExpress.Web.v14.1, Version=14.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxEditors" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.ExpressApp.Web.v14.1" Namespace="DevExpress.ExpressApp.Web.Templates.ActionContainers"
    TagPrefix="cc2" %>
<%@ Register Assembly="DevExpress.ExpressApp.Web.v14.1" Namespace="DevExpress.ExpressApp.Web.Templates.Controls"
    TagPrefix="tc" %>
<%@ Register Assembly="DevExpress.ExpressApp.Web.v14.1" Namespace="DevExpress.ExpressApp.Web.Controls"
    TagPrefix="cc4" %>
<%@ Register Assembly="DevExpress.ExpressApp.Web.v14.1" Namespace="DevExpress.ExpressApp.Web.Templates"
    TagPrefix="cc3" %>
<%@ Register Assembly="DevExpress.Web.v14.1, Version=14.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxCallbackPanel" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v14.1, Version=14.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxPanel" TagPrefix="dx" %>
<cc3:XafUpdatePanel ID="UPPopupWindowControl" runat="server">
    <cc4:XafPopupWindowControl runat="server" ID="PopupWindowControl" />
</cc3:XafUpdatePanel>
<cc3:XafUpdatePanel ID="UPHeader" runat="server">
    <div class="Header">
        <table cellpadding="0" cellspacing="0" border="0">
            <tr>
                <td class="ViewCaption">
                    <cc4:ThemedImageControl ID="TIC" runat="server" BorderWidth="0px" DefaultThemeImageLocation="~/Images/" ImageName="vodafone_ctms_banner_h.png" />
                </td>

            </tr>
        </table>
    </div>
</cc3:XafUpdatePanel>
<table class="DialogContent Content LogonContent" border="0" cellpadding="0" cellspacing="0"
    width="100%">
    <tr>
        <td class="LogonContentCell" align="center">
            <cc3:XafUpdatePanel ID="UPEI" runat="server">
                <tc:ErrorInfoControl ID="ErrorInfo" Style="margin: 10px 0px 10px 0px" runat="server" />
            </cc3:XafUpdatePanel>
            <asp:Table ID="Table1" CssClass="Logon" runat="server" BorderWidth="0px" CellPadding="0"
                CellSpacing="0">
                <asp:TableRow ID="TableRow2" runat="server">
                    <asp:TableCell runat="server" ID="ViewSite">
                        <cc3:XafUpdatePanel ID="UPVSC" runat="server">
                            <cc4:ViewSiteControl ID="viewSiteControl" runat="server" />
                        </cc3:XafUpdatePanel>
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow ID="TableRow3" runat="server">
                    <asp:TableCell runat="server" ID="TableCell4" HorizontalAlign="Right" Style="padding: 20px 0px 20px 0px">
                        <cc3:XafUpdatePanel ID="UPPopupActions" runat="server">
                            <cc2:ActionContainerHolder ID="PopupActions" runat="server" Categories="PopupActions"
                                Style="margin-left: 10px; display: inline" Orientation="Horizontal" ContainerStyle="Buttons">
                                <menu width="100%" itemautowidth="False" horizontalalign="Right" />
                            </cc2:ActionContainerHolder>
                        </cc3:XafUpdatePanel>
                    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>
        </td>
    </tr>
</table>
