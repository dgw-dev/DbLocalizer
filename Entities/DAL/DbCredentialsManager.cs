

using System.Collections.Generic;
using System.Linq;

namespace Entities.DAL
{
    public class DbCredentialsManager
    {
        private readonly Dictionary<string, DbCredentials> _dbCredentials = new Dictionary<string, DbCredentials>()
        {
            {
                "ChorelyMVCContextConnection", new DbCredentials() { Username = "ChorelyUser", Password = "MyG0d1tsfull0fst@rs"}
            } ,
            {
                "hka", new DbCredentials() { Username = "HKArt", Password = "MyG0d1tsfull0fst@rs"}
            },
            {
                "LocalizationTest", new DbCredentials() { Username = "LocalizationUser", Password = "@LocalizationTest#"}
            }
        };
        public DbCredentials GetDbCredentials(string connectionStringName)
        {
            return _dbCredentials?.FirstOrDefault(c => c.Key == connectionStringName).Value;
        }
    }

    public class DbCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
