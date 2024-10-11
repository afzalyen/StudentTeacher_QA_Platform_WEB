using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test_dotnet1_Models.Models;
using test_dotnet_Data_Access; // Ensure this matches your data access layer namespace
using test_dotnet_Data_Access.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using test_dotnet1_Models.Identity;

namespace test_dotnet1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        private async Task<UserType> GetCurrentUserTypeAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.UserType ?? UserType.Student;
        }

        public async Task<IActionResult> Index()
        {
            var userType = await GetCurrentUserTypeAsync();
            _logger.LogInformation("##################");
            _logger.LogInformation("isTeacher: {isTeacher}", userType == UserType.Teacher);

            ViewData["UserType"] = userType;

            var questions = _context.Questions.ToList();
            return View(questions);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> AskQuestion()
        {
            var userType = await GetCurrentUserTypeAsync();
            if (userType == UserType.Teacher)
            {
                return Forbid(); // Teachers cannot ask questions
            }

            return View("~/Views/Questions/AskQuestion.cshtml");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AskQuestion(Question question)
        {
            var userType = await GetCurrentUserTypeAsync();
            if (userType == UserType.Teacher)
            {
                return Forbid();
            }

            question.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogError($"Error in {state.Key}: {error.ErrorMessage}");
                    }
                }
                return View("~/Views/Questions/AskQuestion.cshtml", question);
            }

            question.CreatedAt = DateTime.Now;
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public IActionResult AnswerQuestion(int id)
        {
            var question = _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefault(q => q.Id == id);

            if (question == null)
            {
                return NotFound();
            }
            // Get the UserType of the current user and set it in ViewData
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            ViewData["UserType"] = user?.UserType;

            return View("~/Views/Questions/AnswerQuestion.cshtml", question);

           
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SubmitAnswer(Answer answer)
        {
            var userType = await GetCurrentUserTypeAsync();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var question = await _context.Questions.FindAsync(answer.QuestionId);

            // Ensure only teachers or the question's author can submit an answer
            if (userType != UserType.Teacher && question.UserId != userId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                answer.UserId = userId;
                answer.CreatedAt = DateTime.Now;

                _context.Answers.Add(answer);

                // Set IsAnswered to true if the answer is submitted by a teacher
                if (userType == UserType.Teacher)
                {
                    question.IsAnswered = true;
                    _context.Questions.Update(question);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("AnswerQuestion", new { id = answer.QuestionId });
            }

            var questionWithAnswers = _context.Questions
                                               .Include(q => q.Answers)
                                               .FirstOrDefault(q => q.Id == answer.QuestionId);
            return View("~/Views/Questions/AnswerQuestion.cshtml", questionWithAnswers);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [Authorize]
        public async Task<IActionResult> Activities()
        {
            var userType = await GetCurrentUserTypeAsync();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            List<Question> questions;

            if (userType == UserType.Student)
            {
                // Show questions asked by the student
                questions = await _context.Questions
                    .Where(q => q.UserId == userId)
                    .ToListAsync();
            }
            else if (userType == UserType.Teacher)
            {
                // Show questions answered by the teacher
                questions = await _context.Questions
                    .Where(q => q.Answers.Any(a => a.UserId == userId))
                    .ToListAsync();
            }
            else
            {
                questions = new List<Question>(); // Empty list for other user types, if any
            }

            return View("~/Views/Questions/Activities.cshtml", questions);
        }
    }
}
