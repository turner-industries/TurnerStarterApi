# Turner Starter API

## Project Initialization

1. Verify that TurnerStarterApi.Api is selected as the StartUp Project

2. In `TurnerStarterApi.Core -> appsettings.json`, verify that the server name in the DataContext connection string is for the desired SQL Server instance.

3. Verify for `TurnerStarterApi.Tests.Integration -> appsettings.json` as well.

4. In the Package Manager Console, select `src\TurnerStarterApi.Core` as the Default project and run `Update-Database`.

5. Press `F5` to run the API.

## NCrunch

- The current configuration requires that NCrunch be configured to run on a single thread. This can be configured by selecting NCrunch -> Run Configuration Wizard and setting the number of background process threads to 1.

- Note: This is only required because NCrunch does not follow NUnit's `[OneTimeSetUp]` attribute, which is utilized in `AssemblySetup.cs` to initialize the testing environment.
