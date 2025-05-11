
using Entities.Configuration;
using System.Collections.Generic;

namespace Entities.Interfaces
{
    public interface ICultures
    {
        List<Culture> CultureCollection { get; set; }
    }
}
