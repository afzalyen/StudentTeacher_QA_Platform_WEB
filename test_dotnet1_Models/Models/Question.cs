using System;
using System.Collections.Generic;
using test_dotnet1_Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace test_dotnet1_Models.Models;
public class Question
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public DateTime CreatedAt { get; set; }

    // New properties for handling operations
    public bool IsDeleted { get; set; }
    public bool IsAnswered { get; set; }

    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}