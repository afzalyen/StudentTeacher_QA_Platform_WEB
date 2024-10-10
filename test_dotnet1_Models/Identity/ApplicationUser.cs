using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace test_dotnet1_Models.Identity;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public UserType UserType { get; set; }
    public string? Name { get; set; }
    public string? InstituteName { get; set; }
    public string? InstituteIDCardNo { get; set; }
}

public enum UserType
{
    Student,
    Teacher
}

