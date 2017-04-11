/* Creates SQL Server Job which is scheduled to run at 7am daily */

USE [msdb]
GO

IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs_view WHERE name = N'cashdisc_purge_objects')
EXEC msdb.dbo.sp_delete_job @job_name=N'cashdisc_purge_objects', @delete_unused_schedule=1
GO

/****** Object:  Job [cashdisc_purge_objects]    Script Date: 10/04/2017 6:19:58 PM ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [Database Maintenance]    Script Date: 10/04/2017 6:19:58 PM ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'Database Maintenance' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'Database Maintenance'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'cashdisc_purge_objects', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'No description available.', 
		@category_name=N'Database Maintenance', 
		@owner_login_name=N'sa', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [purge]    Script Date: 10/04/2017 6:19:58 PM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'purge', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'
delete gl from genledger gl
where exists (select * from cashflow cf where cf.GCRecord is not null and cf.oid = gl.SrcCashFlow);

delete fsl from ForexSettleLink fsl
where exists (select * from cashflow cf where cf.GCRecord is not null and cf.oid = fsl.CashFlowIn);

delete fsl from ForexSettleLink fsl
where exists (select * from cashflow cf where cf.GCRecord is not null and cf.oid = fsl.CashFlowOut);

delete bcf from BankStmtCashFlowForecast bcf;

update ft
set ft.CounterCashFlow = null
from forextrade ft
where exists (select * from cashflow cf where cf.oid = ft.CounterCashFlow and cf.gcrecord is not null);

update ft
set ft.PrimaryCashFlow = null
from forextrade ft
where exists (select * from cashflow cf where cf.oid = ft.PrimaryCashFlow and cf.gcrecord is not null);

update cf0
set OrigCashFlow = null
from cashflow cf0
where exists (select * from cashflow cf1 where cf1.GCRecord is not null and cf1.oid = cf0.OrigCashFlow);

update cf0
set ParentCashFlow = null
from cashflow cf0
where exists (select * from cashflow cf1 where cf1.GCRecord is not null and cf1.oid = cf0.ParentCashFlow);

update cf0
set Fixer = null
from cashflow cf0
where exists (select * from cashflow cf1 where cf1.GCRecord is not null and cf1.oid = cf0.Fixer);

delete cf From cashflow cf
where cf.GCRecord is not null;', 
		@database_name=N'CashDiscipline', 
		@flags=12
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'Daily', 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=1, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20170410, 
		@active_end_date=99991231, 
		@active_start_time=70000, 
		@active_end_time=235959, 
		@schedule_uid=N'dc33488e-0f03-4367-b96f-8261a157e13b'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:

GO