using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineAPI.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineAPI.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly AppContext _context;

        public ReportsController(AppContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Проверяем доступ к проекту
            var hasAccess = await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

            if (!hasAccess)
            {
                TempData["ErrorMessage"] = "У вас нет доступа к этому проекту";
                return RedirectToAction("Index", "Projects");
            }

            return View(projectId);
        }

        // API методы для данных отчетов
        [HttpGet]
        public async Task<IActionResult> GetProjectMetrics(int projectId, string period = "30")
        {
            int totalTasks = await _context.Tasks.CountAsync(t => t.ProjectId == projectId);
            int completedTasks = await _context.Tasks.CountAsync(t => t.ProjectId == projectId && t.Status == Entities.TaskStatus.Done);
            Console.WriteLine(totalTasks);

            var metrics = new
            {
                totalTasks = totalTasks,
                completedTasks = completedTasks,
                completionRate = totalTasks/100*completedTasks, // Рассчитайте на основе данных
                avgCompletionTime = 3.2,
                trends = new { tasks = 12, completed = 8, rate = 5, time = -10 }
            };

            return Ok(metrics);
        }

        [HttpGet]
        public async Task<IActionResult> GetStatusDistribution(int projectId)
        {
            var distribution = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .GroupBy(t => t.Status)
                .Select(g => new
                {
                    status = g.Key.ToString(),
                    count = g.Count(),
                    color = GetStatusColor(g.Key)
                })
                .ToListAsync();

            return Ok(distribution);
        }

        [HttpGet]
        public async Task<IActionResult> GetPriorityDistribution(int projectId)
        {
            var distribution = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .GroupBy(t => t.Priority)
                .Select(g => new
                {
                    priority = g.Key.ToString(),
                    count = g.Count(),
                    color = GetPriorityColor(g.Key)
                })
                .ToListAsync();

            return Ok(distribution);
        }

        private string GetStatusColor(Entities.TaskStatus status)
        {
            return status switch
            {
                Entities.TaskStatus.Done => "#36b37e",
                Entities.TaskStatus.InProgress => "#ffab00",
                Entities.TaskStatus.ToDo => "#dfe1e6",
                _ => "#6b778c"
            };
        }

        private string GetPriorityColor(TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.High => "#ff5630",
                TaskPriority.Medium => "#ffab00",
                TaskPriority.Low => "#36b37e",
                _ => "#6b778c"
            };
        }
    }
}