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
rmdir /S /Q D2NXAF.ExpressApp\bin
rmdir /S /Q D2NXAF.ExpressApp.PivotGrid\bin
rmdir /S /Q D2NXAF.ExpressApp.PivotGrid.Web\bin
rmdir /S /Q D2NXAF.ExpressApp.PivotGrid.Win\bin
rmdir /S /Q D2NXAF.PivotGrid\bin
rmdir /S /Q D2NXAF.Utilities\bin

rmdir /S /Q CTMS.Common\obj
rmdir /S /Q CTMS.Forms\obj
rmdir /S /Q CTMS.ImportData\obj
rmdir /S /Q CTMS.Module\obj
rmdir /S /Q CTMS.Module.Web\obj
rmdir /S /Q CTMS.Module.Win\obj
rmdir /S /Q CTMS.Web\obj
rmdir /S /Q CTMS.Win\obj
rmdir /S /Q GenerateUserFriendlyId.Module\obj
rmdir /S /Q D2NXAF.ExpressApp\obj
rmdir /S /Q D2NXAF.ExpressApp.PivotGrid\obj
rmdir /S /Q D2NXAF.ExpressApp.PivotGrid.Web\obj
rmdir /S /Q D2NXAF.ExpressApp.PivotGrid.Win\obj
rmdir /S /Q D2NXAF.PivotGrid\obj
rmdir /S /Q D2NXAF.Utilities\obj

rmdir /S /Q "C:\Users\ctso\AppData\Local\Microsoft\VisualStudio\12.0\ProjectAssemblies"
pause