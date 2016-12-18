using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;

namespace CashDiscipline.Module.BusinessObjects.FinAccounting
{
    [ImageName("BO_List")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [ModelDefault("IsFooterVisible", "True")]
    [DeferredDeletion(false)]
    public class TB_TrialBalance : BaseObject
    {
        public TB_TrialBalance(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        string fCompany;
        [Size(10)]
        public string Company
        {
            get { return fCompany; }
            set { SetPropertyValue<string>("Company", ref fCompany, value); }
        }
        string fAccount;
        [Size(10)]
        public string Account
        {
            get { return fAccount; }
            set { SetPropertyValue<string>("Account", ref fAccount, value); }
        }
        string fBSorPL;
        [Size(10)]
        [Persistent(@"BS or PL")]
        public string BSorPL
        {
            get { return fBSorPL; }
            set { SetPropertyValue<string>("BSorPL", ref fBSorPL, value); }
        }
        string fAccountClass;
        [Size(50)]
        [Persistent(@"Account Class")]
        public string AccountClass
        {
            get { return fAccountClass; }
            set { SetPropertyValue<string>("AccountClass", ref fAccountClass, value); }
        }
        string fAccountDesc;
        [Size(255)]
        [Persistent(@"Account Desc")]
        public string AccountDesc
        {
            get { return fAccountDesc; }
            set { SetPropertyValue<string>("AccountDesc", ref fAccountDesc, value); }
        }
        string fPeriodName;
        [Size(10)]
        [Persistent(@"Period Name")]
        public string PeriodName
        {
            get { return fPeriodName; }
            set { SetPropertyValue<string>("PeriodName", ref fPeriodName, value); }
        }
        decimal fAUDBegBal;
        [Persistent(@"AUD Beg Bal")]
        public decimal AUDBegBal
        {
            get { return fAUDBegBal; }
            set { SetPropertyValue<decimal>("AUDBegBal", ref fAUDBegBal, value); }
        }
        decimal fAUDPTDDr;
        [Persistent(@"AUD PTD Dr")]
        public decimal AUDPTDDr
        {
            get { return fAUDPTDDr; }
            set { SetPropertyValue<decimal>("AUDPTDDr", ref fAUDPTDDr, value); }
        }
        decimal fAUDPTDCr;
        [Persistent(@"AUD PTD Cr")]
        public decimal AUDPTDCr
        {
            get { return fAUDPTDCr; }
            set { SetPropertyValue<decimal>("AUDPTDCr", ref fAUDPTDCr, value); }
        }
        decimal fAUDPTDNet;
        [Persistent(@"AUD PTD Net")]
        public decimal AUDPTDNet
        {
            get { return fAUDPTDNet; }
            set { SetPropertyValue<decimal>("AUDPTDNet", ref fAUDPTDNet, value); }
        }
        decimal fAUDEndBal;
        [Persistent(@"AUD End Bal")]
        public decimal AUDEndBal
        {
            get { return fAUDEndBal; }
            set { SetPropertyValue<decimal>("AUDEndBal", ref fAUDEndBal, value); }
        }
        DateTime fDateKey;
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime DateKey
        {
            get { return fDateKey; }
            set { SetPropertyValue<DateTime>("DateKey", ref fDateKey, value); }
        }
    }

}
