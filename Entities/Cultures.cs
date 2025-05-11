using Entities.Configuration;
using Entities.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Entities
{
    public class Cultures : ICultures
    {
        private readonly IConfiguration _config;
        public List<Culture> CultureCollection { get; set; } = new List<Culture>();

        public Cultures(IConfiguration config)
        {
            _config = config;
            BuildCultures();
        }

        public void BuildCultures()
        {
            var cultureConfig = _config.Get<CultureConfig>();
            CultureCollection = cultureConfig.CultureCollection;
        }
    }
}
