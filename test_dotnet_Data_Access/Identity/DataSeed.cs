// File: test_dotnet_Data_Access/Identity/DataSeed.cs
using Microsoft.AspNetCore.Identity;
using test_dotnet1_Models.Identity; // Adjust this namespace based on your project structure

namespace test_dotnet_Data_Access.Identity
{
    public static class DataSeed
    {
        public static async Task SeedSuperAdmin(UserManager<ApplicationUser> userManager)
        {
            var superAdminEmail = "superadmin@example.com"; 
            var superAdminPassword = "SuperAdmin@123"; 

            var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

            if (superAdminUser == null)
            {
                superAdminUser = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    IsSuperAdmin = true 
                };

                await userManager.CreateAsync(superAdminUser, superAdminPassword);
            }
        }
    }
}
