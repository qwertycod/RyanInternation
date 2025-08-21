using Azure.Identity;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework.Repository
{
    public class Helper
    {
        public static string Conn()
        {
            string server = "homeworkappdb.database.windows.net";
            string database = "TestDb";
            string tenantId = "13b90aa9-fd62-473c-a929-03fb65181677"; // (N) from Azure Portal → Azure AD → Properties
            string clientId = "b95c443d-b028-4c5d-891b-86e4b0d717aa"; // (N) from Azure Portal → App registration → Properties
            var clientSecretAppReg = "";

            var credential = new ClientSecretCredential(
                tenantId: tenantId,
                clientId: clientId,
                clientSecret: clientSecretAppReg
            );

            var token = credential.GetToken(
                new Azure.Core.TokenRequestContext(
                    new[] { "https://database.windows.net/.default" }
                )
            );

            var connection = new SqlConnection(new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = database,
                //AccessToken = token.Token
            }.ConnectionString);

            return connection.ConnectionString;
        }
    }
}
