IF OBJECT_ID('dbo.sp_CalculateDateColumns') IS NOT NULL
	BEGIN		
		DROP PROCEDURE dbo.sp_CalculateDateColumns;
	END;

GO

CREATE PROCEDURE sp_CalculateDateColumns
(
@FromDate datetime,
@ToDate datetime = '2999-12-31'
)
AS
UPDATE DimDate SET
[DayOfMonth] = DAY(FullDate),
[MonthNumberOfYear] = MONTH(FullDate),
[Year] = YEAR(FullDate),
[DayOfWeekNumber] = DATEPART(weekday, FullDate),
[MonthAbbrev] = FORMAT(FullDate, 'MMM'),
[DayAbbrev] = FORMAT(FullDate, 'ddd'),
[WeekEnding] = DATEADD(d, 5, FullDate) - DATEPART(weekday, FullDate);
