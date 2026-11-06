using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Configuration
{
    public class CorsOptions
    {
        public string[] AllowedOrigins { get; set; } = [];
    }
}