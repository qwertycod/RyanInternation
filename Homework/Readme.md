Student management system
DI for IOC
Using JWT middleware to authenticate.


----
All config are coming from keyvault(n)
like - _configuration.GetConnectionString("ServicBusConnectionString");
only secret of KV and AppInsightComing from App service.

If the user is having valid token in request, context User is set, so the [Authorize] key works.
Other way to authenticate, (not being used currently) is login by loginByCookie method. It uses HomeworkAppCookie.
Setting CSRF in the login using SameSite = SameSiteMode.Strict can prevent CSRF attacks.


For sql azure auth to work, we added the user in the az directory and this in the sql

CREATE USER [v-user@microsoft.com] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [v-user@microsoft.com];
ALTER ROLE db_datawriter ADD MEMBER [v-user@microsoft.com];


scaffold - command
Scaffold-DbContext "Server=tcp:homeworkappdb.database.windows.net,1433;Initial Catalog=TestDb;Persist Security Info=False;User ID=HomeworkIssuer;Password=;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context TestDbContext -DataAnnotations -Force

---------------

Az function app running to update/revert inventroy details.

---------------

To run on docker
--> run these from root folder where we all the projects
docker build -t homeworkapi:1.0 .
docker run -p 8080:8080 homeworkapi:1.0

--> app runs on localhost:8080

--> to push via ACR - create container registry and use commands

-----------------