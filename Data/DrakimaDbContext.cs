using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DrakimaWebsite.Data
{
    public class DrakimaDbContext : IdentityDbContext
    {
        public DrakimaDbContext(DbContextOptions<DrakimaDbContext> options)
            : base(options)
        {
        }
    }
}