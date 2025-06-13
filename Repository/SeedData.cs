using Microsoft.EntityFrameworkCore;
using QlW_BandoanOnline.Models;

namespace QlW_BandoanOnline.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {
            _context.Database.Migrate();
        }
    }
}