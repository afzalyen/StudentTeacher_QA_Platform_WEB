using System;
using test_dotnet1_Models.Identity;

namespace test_dotnet1_Models.Models;
public class Answer
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public DateTime CreatedAt { get; set; }
    public int QuestionId { get; set; }
    public Question? Question { get; set; }
}