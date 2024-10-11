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

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userType = await GetCurrentUserTypeAsync();
            ViewData["UserType"] = userType;

            // Step 1: Get all questions that are not deleted, including answers
            var questions = await _context.Questions
                .Where(q => !q.IsDeleted)
                .Include(q => q.Answers) // Include answers to avoid multiple queries
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            // Step 2: Create a list to hold the questions with their associated user info
            var questionsWithUserInfo = new List<object>();

            // Step 3: Loop through the questions and fetch user information
            foreach (var question in questions)
            {
                var askedByUser = await _userManager.FindByIdAsync(question.UserId);

                string answeredBy = null; // Initialize answeredBy as null
                if (question.IsAnswered)
                {
                    var answer = question.Answers.FirstOrDefault(); // Get the first answer if available
                    if (answer != null) // Check if answer is not null
                    {
                        var answeringUser = await _userManager.FindByIdAsync(answer.UserId);
                        answeredBy = answeringUser?.Email?.Split('@')[0]; // Get first part of Teacher's email
                    }
                }

                questionsWithUserInfo.Add(new
                {
                    Question = question,
                    AskedBy = askedByUser?.Name, // Assuming Name is not null
                    AnsweredBy = answeredBy
                });
            }

            return View(questionsWithUserInfo);
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

        public async Task<IActionResult> AnswerQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                return NotFound();
            }

            // Get the asked by user's name
            var askedByUser = await _userManager.FindByIdAsync(question.UserId);

            // Create a new list for answers with display information
            var answersWithUserInfo = new List<object>();

            // Loop through the answers and fetch user information
            foreach (var answer in question.Answers)
            {
                var answeringUser = await _userManager.FindByIdAsync(answer.UserId);
                answersWithUserInfo.Add(new
                {
                    Answer = answer,
                    UserDisplayName = answeringUser?.Name, // Get the display name of the answerer
                    UserEmailPrefix = answeringUser?.Email?.Split('@')[0] // Get the first part of Teacher's email
                });
            }

            // Set the UserType of the current user in ViewData
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            ViewData["UserType"] = user?.UserType;

            ViewBag.AskedByName = askedByUser?.Name; // Store asked by user's name in ViewBag
            ViewBag.Answers = answersWithUserInfo; // Pass answers with user info to the view

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

            _logger.LogInformation("User ID: {UserId}, User Type: {UserType}", userId, userType);

            try
            {
                if (userType == UserType.Student)
                {
                    // Show questions asked by the student
                    questions = await _context.Questions
                        .Where(q => q.UserId == userId && !q.IsDeleted)
                        .OrderByDescending(q => q.CreatedAt)
                        .ToListAsync();
                }
                else if (userType == UserType.Teacher)
                {
                    // Show questions answered by the teacher
                    questions = await _context.Questions
                        .Where(q => q.Answers.Any(a => a.UserId == userId) && !q.IsDeleted)
                        .OrderByDescending(q => q.CreatedAt)
                        .ToListAsync();
                }
                else
                {
                    questions = new List<Question>();
                }

                // Create a list to hold questions with user info
                var questionsWithUserInfo = new List<object>();

                foreach (var question in questions)
                {
                    var askedByUser = await _userManager.FindByIdAsync(question.UserId);

                    string answeredBy = null; // Initialize answeredBy as null
                    if (question.IsAnswered)
                    {
                        var answer = question.Answers.FirstOrDefault(); // Get the first answer if available
                        if (answer != null) // Check if answer is not null
                        {
                            var answeringUser = await _userManager.FindByIdAsync(answer.UserId);
                            answeredBy = answeringUser?.Email?.Split('@')[0]; // Get first part of Teacher's email
                        }
                    }

                    questionsWithUserInfo.Add(new
                    {
                        Question = question,
                        AskedBy = askedByUser?.Name, // Get the name of the user who asked
                        AnsweredBy = answeredBy
                    });
                }

                ViewData["UserType"] = userType.ToString(); // Set the UserType in ViewData
                return View("~/Views/Questions/Activities.cshtml", questionsWithUserInfo); // Pass the modified list to the view
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading activities for User ID: {UserId}", userId);
                return StatusCode(500, "Internal server error"); // Return a server error
            }
        }



        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var userType = await GetCurrentUserTypeAsync();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var question = await _context.Questions.FindAsync(id);

            if (question == null || question.UserId != userId || question.IsAnswered)
            {
                return Forbid(); // Only allow students to delete their own unanswered questions
            }

            question.IsDeleted = true; // Mark question as deleted
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
            return RedirectToAction("Activities"); // Redirect back to Activities
        }
    }
}
