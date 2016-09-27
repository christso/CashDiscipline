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