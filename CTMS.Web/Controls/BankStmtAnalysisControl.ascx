<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BankStmtAnalysisControl.ascx.cs" Inherits="CTMS.Web.Controls.BankStmtAnalysisControl" %>
<%@ Register assembly="DevExpress.Web.ASPxPivotGrid.v13.2, Version=13.2.9.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.Web.ASPxPivotGrid" tagprefix="dx" %>

<dx:ASPxPivotGrid ID="ASPxPivotGrid1" runat="server" ClientIDMode="AutoID" OLAPConnectionString="provider=msolap;data source=169.254.148.173;initial catalog=TreasuryAnalysis;user id=ctso;password=hutcat105!;cube name=Treasury">
    <Fields>
        <dx:PivotGridField ID="fieldAccountName1" AreaIndex="0" Caption="Account Name" FieldName="[Account].[Account Name].[Account Name]">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldActivityName1" AreaIndex="1" Caption="Activity Name" FieldName="[Activity].[Activity Name].[Activity Name]">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldDim111" AreaIndex="2" Caption="Dim 1 1" FieldName="[Activity].[Dim 1 1].[Dim 1 1]">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldDim121" AreaIndex="3" Caption="Dim 1 2" FieldName="[Activity].[Dim 1 2].[Dim 1 2]">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldDim131" AreaIndex="4" Caption="Dim 1 3" FieldName="[Activity].[Dim 1 3].[Dim 1 3]">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldCurrencyName1" AreaIndex="5" Caption="Currency Name" FieldName="[Currency].[Currency Name].[Currency Name]">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldYears1" AreaIndex="6" Caption="Years" FieldName="[Date].[Calendar Date].[Years]" GroupIndex="0" InnerGroupIndex="0">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldHalfYears1" AreaIndex="7" Caption="HalfYears" FieldName="[Date].[Calendar Date].[HalfYears]" GroupIndex="0" InnerGroupIndex="1">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldQuarters1" AreaIndex="8" Caption="Quarters" FieldName="[Date].[Calendar Date].[Quarters]" GroupIndex="0" InnerGroupIndex="2">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldMonths1" AreaIndex="9" Caption="Months" FieldName="[Date].[Calendar Date].[Months]" GroupIndex="0" InnerGroupIndex="3">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldDate1" AreaIndex="10" Caption="Date" FieldName="[Date].[Calendar Date].[Date]" GroupIndex="0" InnerGroupIndex="4">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldDate2" AreaIndex="11" Caption="Date" FieldName="[Date].[Date].[Date]">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldHalfYears2" AreaIndex="12" Caption="HalfYears" FieldName="[Date].[HalfYears].[HalfYears]">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldMonths2" AreaIndex="13" Caption="Months" FieldName="[Date].[Months].[Months]">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldQuarters2" AreaIndex="14" Caption="Quarters" FieldName="[Date].[Quarters].[Quarters]">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldYears2" AreaIndex="15" Caption="Years" FieldName="[Date].[Years].[Years]">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldTranAmount1" Area="DataArea" AreaIndex="0" Caption="Tran Amount" DisplayFolder="Bank Stmt" FieldName="[Measures].[Tran Amount]">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldFunctionalCcyAmt1" Area="DataArea" AreaIndex="1" Caption="Local Ccy Amt" DisplayFolder="Bank Stmt" FieldName="[Measures].[Local Ccy Amt]">
        </dx:PivotGridField>
        <dx:PivotGridField ID="fieldCounterCcyAmt1" Area="DataArea" AreaIndex="2" Caption="Counter Ccy Amt" DisplayFolder="Bank Stmt" FieldName="[Measures].[Counter Ccy Amt]">
        </dx:PivotGridField>
    </Fields>
    <Groups>
        <dx:PivotGridWebGroup Caption="Calendar Date" Hierarchy="[Date].[Calendar Date]" ShowNewValues="True" />
    </Groups>
</dx:ASPxPivotGrid>

