/* Forex Rate ------------ */

IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('dbo.ForexRate') AND NAME = 'i_ForexRate_Date_Curr')
    DROP INDEX [i_ForexRate_Date_Curr] ON [dbo].[ForexRate]
GO

CREATE NONCLUSTERED INDEX [i_ForexRate_Date_Curr] ON [dbo].[ForexRate]
(
	[ConversionDate] ASC,
	[FromCurrency] ASC,
	[ToCurrency] ASC
)
GO

/* CashFlow ------------*/

IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('dbo.CashFlow') AND NAME = 'iCashFlow_Snapshot_TranDate_Status')
    DROP INDEX [iCashFlow_Snapshot_TranDate_Status] ON [dbo].[CashFlow]
GO

CREATE NONCLUSTERED INDEX [iCashFlow_Snapshot_TranDate_Status] ON [dbo].[CashFlow]
(
	[Snapshot] ASC,
	[TranDate] ASC,
	[Status] ASC
)
GO

IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('dbo.CashFlow') AND NAME = 'i_CashFlow_Snapshot_Reclass')
    DROP INDEX [i_CashFlow_Snapshot_Reclass] ON [dbo].[CashFlow]
GO

CREATE NONCLUSTERED INDEX [i_CashFlow_Snapshot_Reclass] ON [dbo].[CashFlow]
(
	[Snapshot] ASC,
	[IsReclass] ASC,
	[GCRecord] ASC
)
INCLUDE ([Oid])