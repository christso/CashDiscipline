mkdir "%userprofile%\Cash Discipline\"
robocopy "\\finserv01\Apps\Cash Discipline\Application Files\Current" "%userprofile%\Cash Discipline" /MIR
start "" "%userprofile%\Cash Discipline\CashDiscipline.Win.exe"
EXIT