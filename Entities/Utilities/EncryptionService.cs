using Entities.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace Entities.Utilities
{
    public class EncryptionService: IEncryptionService
    {
        private readonly IDataProtector _protector;

        public EncryptionService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("DbLocalizer.Entities.Security.Encryption");
        }

        public string Encrypt(string plainText)
        {
            return _protector.Protect(plainText);
        }

        public string Decrypt(string encryptedData)
        {
            return _protector.Unprotect(encryptedData);
        }
    }
}
