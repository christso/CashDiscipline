/* Forex Rate ------------ */

IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('dbo.ForexRate') AND NAME = 'iConversionDateToCurrencyFromCurrency_ForexRate')
    DROP INDEX [iConversionDateToCurrencyFromCurrency_ForexRate] ON [dbo].[ForexRate]
GO

CREATE NONCLUSTERED INDEX [iConversionDateToCurrencyFromCurrency_ForexRate] ON [dbo].[ForexRate]
(
	[ConversionDate] ASC,
	[FromCurrency] ASC,
	[ToCurrency] ASC
)
GO


/* CashFlow ------------*/

IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('dbo.CashFlow') AND NAME = 'i_CashFlow_Snapshot_TranDate_Status')
    DROP INDEX [i_CashFlow_Snapshot_TranDate_Status] ON [dbo].[CashFlow]
GO

CREATE NONCLUSTERED INDEX [i_CashFlow_Snapshot_TranDate_Status] ON [dbo].[CashFlow]
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

IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('dbo.CashFlow') AND NAME = 'i_CashFlow_Oid')
    DROP INDEX i_CashFlow_Oid ON [dbo].[CashFlow]
GO

CREATE NONCLUSTERED INDEX [i_CashFlow_Oid]
ON [dbo].[CashFlow] ([Oid])
INCLUDE (Snapshot, TranDate, Account, Activity, Counterparty, AccountCcyAmt, FunctionalCcyAmt, CounterCcyAmt, CounterCcy, Description, Source, Status, FixRank, Fix)

IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('dbo.CashFlow') AND NAME = 'i_CashFlow_GCRecord')
    DROP INDEX i_CashFlow_GCRecord ON [dbo].[CashFlow]
GO

CREATE NONCLUSTERED INDEX i_CashFlow_GCRecord
ON [dbo].[CashFlow] ([GCRecord])
INCLUDE ([Oid],[Snapshot],[TranDate],[Account],[Activity],[Counterparty],[AccountCcyAmt],[FunctionalCcyAmt],[CounterCcyAmt],[CounterCcy],[Description],[Source],[Status],[FixRank],[Fix],[IsReclass])