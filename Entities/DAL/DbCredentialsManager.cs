

using System.Collections.Generic;
using System.Linq;

namespace Entities.DAL
{
    public class DbCredentialsManager
    {
        private readonly Dictionary<string, DbCredentials> _dbCredentials = new Dictionary<string, DbCredentials>()
        {
            {
                "LocalizationTest", new DbCredentials() { ServerName = "localhost", DatabaseName = "LocalizationTest", Username = "LocalizationUser", Password = "@LocalizationTest#"}
            }
        };
        public DbCredentials GetDbCredentials(string connectionStringName)
        {
            return _dbCredentials?.FirstOrDefault(c => c.Key == connectionStringName).Value;
        }
    }

    public class DbCredentials
    {
        public string DatabaseName { get; set; }
        public string ServerName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
