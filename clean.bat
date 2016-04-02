for /f %%i in ("%0") do set curpath=%%~dpi 
cd /d %curpath% 

rmdir /S /Q CTMS.Module\bin
rmdir /S /Q CTMS.Module.Win\bin
rmdir /S /Q CTMS.Win\bin
rmdir /S /Q CTMS.UnitTests\bin

rmdir /S /Q CTMS.Module\obj
rmdir /S /Q CTMS.Module.Win\obj
rmdir /S /Q CTMS.Win\obj
rmdir /S /Q CTMS.UnitTests\obj

rmdir /S /Q "C:\Users\ctso\AppData\Local\Microsoft\VisualStudio\14.0\ProjectAssemblies"
rmdir /S /Q "C:\Users\ctso.VHA\AppData\Local\Microsoft\VisualStudio\14.0\ProjectAssemblies"
pause