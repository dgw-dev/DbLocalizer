using System.Net;

namespace Entities.Plugins.TranslationManagement.Smartling
{
    public class SmartlingAuthResponse
    {
        public bool IsSuccessStatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public authResponse response { get; set; }

        public SmartlingAuthResponse()
        {
            if (response != null && response.code == "SUCCESS")
            {
                IsSuccessStatusCode = true;
            }
        }
    }

    public class authResponse
    {
        public string code { get; set; }
        public authData data { get; set; }
    }

    public class authData
    {
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
        public int expiresIn { get; set; }
        public int refreshExpiresIn { get; set; }
        public string tokenType { get; set; }
    }
}
