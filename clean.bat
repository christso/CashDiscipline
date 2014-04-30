for /f %%i in ("%0") do set curpath=%%~dpi 
cd /d %curpath% 

rmdir /S /Q CTMS.Common\bin
rmdir /S /Q CTMS.Forms\bin
rmdir /S /Q CTMS.ImportData\bin
rmdir /S /Q CTMS.Module\bin
rmdir /S /Q CTMS.Module.Web\bin
rmdir /S /Q CTMS.Module.Win\bin
rmdir /S /Q CTMS.Web\bin
rmdir /S /Q CTMS.Win\bin
rmdir /S /Q GenerateUserFriendlyId.Module\bin
rmdir /S /Q DG2NTT.ExpressApp\bin
rmdir /S /Q DG2NTT.ExpressApp.PivotGrid\bin
rmdir /S /Q DG2NTT.ExpressApp.PivotGrid.Web\bin
rmdir /S /Q DG2NTT.ExpressApp.PivotGrid.Win\bin
rmdir /S /Q DG2NTT.PivotGrid\bin
rmdir /S /Q DG2NTT.Utilities\bin

rmdir /S /Q CTMS.Common\obj
rmdir /S /Q CTMS.Forms\obj
rmdir /S /Q CTMS.ImportData\obj
rmdir /S /Q CTMS.Module\obj
rmdir /S /Q CTMS.Module.Web\obj
rmdir /S /Q CTMS.Module.Win\obj
rmdir /S /Q CTMS.Web\obj
rmdir /S /Q CTMS.Win\obj
rmdir /S /Q GenerateUserFriendlyId.Module\obj
rmdir /S /Q DG2NTT.ExpressApp\obj
rmdir /S /Q DG2NTT.ExpressApp.PivotGrid\obj
rmdir /S /Q DG2NTT.ExpressApp.PivotGrid.Web\obj
rmdir /S /Q DG2NTT.ExpressApp.PivotGrid.Win\obj
rmdir /S /Q DG2NTT.PivotGrid\obj
rmdir /S /Q DG2NTT.Utilities\obj

rmdir /S /Q "C:\Users\ctso\AppData\Local\Microsoft\VisualStudio\11.0\ProjectAssemblies"
pause