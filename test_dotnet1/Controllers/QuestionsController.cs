using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using test_dotnet_Data_Access.Identity;
using test_dotnet1_Models.Identity;
using test_dotnet1_Models.Models;
[Authorize]
public class QuestionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public QuestionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Questions
    public async Task<IActionResult> Index()
    {
        var questions = await _context.Questions
            .Where(q => !q.IsDeleted)
            .Include(q => q.User)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
        return View(questions);
    }

    //[HttpGet]
    //[Authorize(Roles = "Student")]
    //public IActionResult AskQuestion()
    //{
    //    return View();
    //}

    //[HttpPost]
    //[Authorize(Roles = "Student")]
    //public async Task<IActionResult> AskQuestion(Question question)
    //{
    //    if (ModelState.IsValid)
    //    {
    //        question.UserId = _userManager.GetUserId(User);
    //        question.CreatedAt = DateTime.Now;
    //        question.IsAnswered = false;
    //        question.IsDeleted = false;

    //        _context.Questions.Add(question);
    //        await _context.SaveChangesAsync();

    //        return RedirectToAction(nameof(Index));
    //    }
    //    return View(question);
    //}

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> AnswerQuestion(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null || question.IsDeleted)
        {
            return NotFound();
        }
        return View(question);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> AnswerQuestion(int id, string answerContent)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null || question.IsDeleted)
        {
            return NotFound();
        }

        var answer = new Answer
        {
            Content = answerContent,
            UserId = _userManager.GetUserId(User),
            CreatedAt = DateTime.Now,
            QuestionId = id
        };

        _context.Answers.Add(answer);
        question.IsAnswered = true;
        _context.Update(question);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null || question.IsAnswered)
        {
            return NotFound();
        }

        question.IsDeleted = true;
        _context.Update(question);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // Additional actions for viewing personal questions can be added here...
}