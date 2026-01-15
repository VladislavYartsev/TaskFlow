using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnlineAPI.Entities
{
    public class TaskCreateViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskPriority Priority { get; set; }
        public int ProjectID { get; set; }
        public List<int> SelectedUsers { get; set; }
        public List<SelectListItem> AvailableUsers { get; set; }

    }
}
