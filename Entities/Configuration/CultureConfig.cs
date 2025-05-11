using System.Collections.Generic;

namespace Entities.Configuration
{
    public class CultureConfig
    {
        public List<Culture> CultureCollection { get; set; } = new List<Culture>();
    }

    public class Culture
    {
        public string CultureCode { get; set; }
        public string SmartlingCultureCode { get; set; }
    }
}
