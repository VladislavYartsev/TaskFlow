using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineAPI.Entities;
using System;
using System.Security.Claims;

namespace OnlineAPI.Controllers
{

    [Authorize]
    public class TasksController : Controller
        {
            private readonly AppContext _context;

            public TasksController(AppContext context)
            {
                _context = context;
            }

        [HttpGet]
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

            var project = await _context.Projects.FindAsync(projectId);
            var tasks = await _context.Tasks.Where(t => t.ProjectId == projectId).ToListAsync();

            ViewBag.Project = project;
            return View(tasks);
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Entities.Task task)
        {
            Console.WriteLine("=== CREATE ACTION STARTED ===");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== MODEL STATE ERRORS ===");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"Field: {key}, Error: {error.ErrorMessage}");
                    }
                }


                return View(task);
            }

            try
            {
                Console.WriteLine("=== TRYING TO SAVE ===");

                // Генерация кода задачи
                var lastTask = await _context.Tasks.OrderByDescending(t => t.Id).FirstOrDefaultAsync();
                var nextId = lastTask?.Id + 1 ?? 1;
                task.TaskCode = $"TASK-{nextId}";
                task.Status = Entities.TaskStatus.ToDo;
                task.CreatedDate = DateTime.UtcNow;

                _context.Add(task);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== SAVE SUCCESSFUL ===");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== EXCEPTION: {ex.Message} ===");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                ModelState.AddModelError("", $"Ошибка при сохранении: {ex.Message}");
                return View(task);
            }
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var task = await _context.Tasks.FindAsync(id);
                if (task == null)
                {
                    return NotFound();
                }
                return View(task);
            }

            // POST: Tasks/Edit/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, Entities.Task task)
            {
                if (id != task.Id)
                {
                    return NotFound();
                }

                ModelState.Remove("CreatedDate");
                ModelState.Remove("TaskCode");

            if (ModelState.IsValid)
                {

                    try
                    {
                        var existingTask = await _context.Tasks.FindAsync(id);
                        if (existingTask == null)
                        {
                            return NotFound();
                        }

                        existingTask.Description = task.Description;
                        existingTask.Status = task.Status;
                        existingTask.Priority = task.Priority;
                        existingTask.Assignee = task.Assignee;
                        existingTask.UpdatedDate = DateTime.UtcNow;

                        Console.WriteLine($"Trying save data with created time {task.CreatedDate} TASK-{task.Id}");
                        task.UpdatedDate = DateTime.UtcNow;
                        task.TaskCode = $"TASK-{task.Id}";
                        _context.Update(existingTask);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!TaskExists(task.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                return View(task);
            }

            // POST: Tasks/Move
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Move([FromForm] TaskMoveRequest moveRequest)
            {
                var task = await _context.Tasks.FindAsync(moveRequest.TaskId);
                if (task == null)
                {
                    return NotFound();
                }

                task.Status = moveRequest.NewStatus;
                task.UpdatedDate = DateTime.UtcNow;
                

                _context.Update(task);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // GET: Tasks/Delete/5 
            public async Task<IActionResult> Delete(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var task = await _context.Tasks
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (task == null)
                {
                    return NotFound();
                }

                return View(task);
            }

            // POST: Tasks/Delete/5 
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                var task = await _context.Tasks.FindAsync(id);
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            private bool TaskExists(int id)
            {
                return _context.Tasks.Any(e => e.Id == id);
            }
        }
}
