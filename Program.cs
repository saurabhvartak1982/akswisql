// <directives>
using System;
using System.Threading;
using Azure.Security.KeyVault.Secrets;
//**Namespace needed to connect to Azure SQL Database**
using Microsoft.Data.SqlClient;
// <directives>

namespace akvdotnet
{
    public class Program
    {
        static void Main(string[] args)
        {
            Program P = new Program();
            string keyvaultURL = Environment.GetEnvironmentVariable("KEYVAULT_URL");
            if (string.IsNullOrEmpty(keyvaultURL)) {
                Console.WriteLine("KEYVAULT_URL environment variable not set");
                return;
            }

            string secretName = Environment.GetEnvironmentVariable("SECRET_NAME");
            if (string.IsNullOrEmpty(secretName)) {
                Console.WriteLine("SECRET_NAME environment variable not set");
                return;
            }

            SecretClient client = new SecretClient(
                new Uri(keyvaultURL),
                new MyClientAssertionCredential());

            while (true)
            {
                Console.WriteLine($"{Environment.NewLine}START {DateTime.UtcNow} ({Environment.MachineName})");

                // <getsecret>
                var keyvaultSecret = client.GetSecret(secretName).Value;
                Console.WriteLine("Your secret is " + keyvaultSecret.Value);

                //**Code to connect to Azure SQL Database starts here**

                //Connect to Azure SQL Database
                Console.WriteLine("Attempting to connect to SQL Server");

                //Build a SqlConnection connection string. Replace the strings <your-sql-server-name> and <db-name> with the Server and Database name of your Azure SQL Database 
                SqlConnection connection = new SqlConnection("Server=tcp:<your-sql-server-name>.database.windows.net;Database=<db-name>;Authentication=Active Directory Default;TrustServerCertificate=True");
                try
                {
                    connection.Open();
                    Console.WriteLine("Connected to SQL Server");
                    //Fetch data from the table SalesLT.Customer
                    Console.WriteLine("Fetching data from table SalesLT.Customer");

                    String sql = "SELECT * from SalesLT.Customer";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //Print the FirstName column in the Console
                                Console.WriteLine("{0} ", reader.GetString(3));
                            }
                        }
                    }
                    
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {
                    connection.Close();
                }

                //**Code to connect to Azure SQL Database ends here**

                // sleep and retry periodically
                Thread.Sleep(600000);
            }
        }
    }
}
