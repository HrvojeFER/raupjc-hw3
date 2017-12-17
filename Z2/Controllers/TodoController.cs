using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Z1;
using Z2.Models;
using Z2.Models.TodoViewModels;
using static System.String;

namespace Z2.Controllers
{
    [Authorize]
    public class TodoController : Controller
    {
        private readonly ITodoRepository _repository;
        private readonly UserManager<ApplicationUser> _userManager;

        public TodoController(ITodoRepository repository, UserManager<ApplicationUser> userManager)
        {
            _repository = repository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var model = new IndexViewModel(_repository.GetActive(new Guid((await GetCurrentUser()).Id))
                                           .Select(item => new TodoViewModel(item.Id, item.Text, item.DateDue, item.DateCompleted.HasValue))
                                           .ToList());
            return View(model);
        }

        public async Task<IActionResult> Completed()
        {
            var model = new IndexViewModel(_repository.GetCompleted(new Guid((await GetCurrentUser()).Id))
                                           .Select(item => new TodoViewModel(item.Id, item.Text, item.DateDue, item.DateCompleted.HasValue))
                                           .ToList());
            return View(model);
        }


        public async Task<IActionResult> ToggleCompleted(Guid id)
        {
            var currentUser = await GetCurrentUser();
            var item = _repository.Get(id, new Guid(currentUser.Id));
            
            item.DateCompleted = DateTime.Now;
            _repository.Update(item, new Guid(currentUser.Id));

            return Redirect("Index");
        }

        public IActionResult Add()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Add(AddTodoViewModel added)
        {
            if (!ModelState.IsValid) return View(added);

            var item = new TodoItem(added.Text, new Guid((await GetCurrentUser()).Id))
            {
                DateCreated = DateTime.Now,
                DateDue = added.DateDue
            };
            _repository.Add(item);

            if (IsNullOrEmpty(added.Label)) return RedirectToAction("Index");

            var labels = added.Label.Split(',');
            foreach (var s in labels)
                _repository.AddLabel(s.Trim().ToLower(), item.Id);

            return RedirectToAction("Index");
        }

        public async Task<ApplicationUser> GetCurrentUser()
        {
            return await _userManager.GetUserAsync(HttpContext.User);
        }

    }

}