echo off
echo "Running MS-Test and OpenCover"

REM "Workspace of the parent job"
SET WORKSPACE=%1

REM "Build configuration which was used to compile the parent project"
SET BUILD_CONFIG=%2

REM "MS-Test configuration file"
SET MSTEST_CONFIG=%3

"C:\build\scripts\windows\CodeCoverage\CodeCoverage-All.bat" %WORKSPACE% "/testcontainer:%WORKSPACE%\PSoC.ManagementService.Data.IntegrationTests\bin\%BUILD_CONFIG%\PSoC.ManagementService.Data.IntegrationTests.dll /testcontainer:%WORKSPACE%\PSoC.ManagementService.Data.UnitTests\bin\%BUILD_CONFIG%\PSoC.ManagementService.Data.UnitTests.dll /testcontainer:%WORKSPACE%\PSoC.ManagementService.IntegrationTests\bin\%BUILD_CONFIG%\PSoC.ManagementService.IntegrationTests.dll /testcontainer:%WORKSPACE%\PSoC.ManagementService.Services.UnitTest\bin\%BUILD_CONFIG%\PSoC.ManagementService.Services.UnitTest.dll /testcontainer:%WORKSPACE%\PSoC.ManagementService.Security.UnitTests\bin\%BUILD_CONFIG%\PSoC.ManagementService.Security.UnitTests.dll /testcontainer:%WORKSPACE%\PSoC.ManagementService.UnitTest\bin\%BUILD_CONFIG%\PSoC.ManagementService.UnitTest.dll /testsettings:%MSTEST_CONFIG% /resultsfile:%WORKSPACE%\mstest_results.trx" "+[*]* -[PSoC.ManagementService.Data.IntegrationTests]* -[PSoC.ManagementService.Data.UnitTests]* -[PSoC.ManagementService.IntegrationTests]* -[PSoC.ManagementService.Services.UnitTest]* -[PSoC.ManagementService.Security.UnitTests]* -[PSoC.ManagementService.UnitTest]*" MSTEST WIN
