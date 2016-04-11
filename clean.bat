for /f %%i in ("%0") do set curpath=%%~dpi 
cd /d %curpath% 

rmdir /S /Q CashDiscipline.Module\bin
rmdir /S /Q CashDiscipline.Module.Win\bin
rmdir /S /Q CashDiscipline.Win\bin
rmdir /S /Q CashDiscipline.UnitTests\bin

rmdir /S /Q CashDiscipline.Module\obj
rmdir /S /Q CashDiscipline.Module.Win\obj
rmdir /S /Q CashDiscipline.Win\obj
rmdir /S /Q CashDiscipline.UnitTests\obj

rmdir /S /Q "C:\Users\ctso\AppData\Local\Microsoft\VisualStudio\14.0\ProjectAssemblies"
rmdir /S /Q "C:\Users\ctso.VHA\AppData\Local\Microsoft\VisualStudio\14.0\ProjectAssemblies"
pause