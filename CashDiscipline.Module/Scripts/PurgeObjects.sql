/* Test purposes only */

delete from genledger
where exists (select * from cashflow where cashflow.GCRecord is not null and cashflow.oid = genledger.SrcCashFlow)

delete from ForexSettleLink
where exists (select * from cashflow where cashflow.GCRecord is not null and cashflow.oid = forexsettlelink.CashFlowIn)

delete from ForexSettleLink
where exists (select * from cashflow where cashflow.GCRecord is not null and cashflow.oid = forexsettlelink.CashFlowOut)

update cf0
set ParentCashFlow = null
from cashflow cf0
where exists (select * from cashflow cf1 where cf1.GCRecord is not null and cf1.oid = cf0.ParentCashFlow)

delete From cashflow
where GCRecord is not null