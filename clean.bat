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
rmdir /S /Q Xafology.ExpressApp\bin
rmdir /S /Q Xafology.ExpressApp.PivotGrid\bin
rmdir /S /Q Xafology.ExpressApp.PivotGrid.Web\bin
rmdir /S /Q Xafology.ExpressApp.PivotGrid.Win\bin
rmdir /S /Q Xafology.PivotGrid\bin
rmdir /S /Q Xafology.Utils\bin

rmdir /S /Q CTMS.Common\obj
rmdir /S /Q CTMS.Forms\obj
rmdir /S /Q CTMS.ImportData\obj
rmdir /S /Q CTMS.Module\obj
rmdir /S /Q CTMS.Module.Web\obj
rmdir /S /Q CTMS.Module.Win\obj
rmdir /S /Q CTMS.Web\obj
rmdir /S /Q CTMS.Win\obj
rmdir /S /Q GenerateUserFriendlyId.Module\obj
rmdir /S /Q Xafology.ExpressApp\obj
rmdir /S /Q Xafology.ExpressApp.PivotGrid\obj
rmdir /S /Q Xafology.ExpressApp.PivotGrid.Web\obj
rmdir /S /Q Xafology.ExpressApp.PivotGrid.Win\obj
rmdir /S /Q Xafology.PivotGrid\obj
rmdir /S /Q Xafology.Utils\obj

rmdir /S /Q "C:\Users\ctso\AppData\Local\Microsoft\VisualStudio\12.0\ProjectAssemblies"
pause