using Microsoft.AspNetCore.Identity;

namespace TodoAPI.Models
{
    public class TodoUser : IdentityUser
    {
        public List<TodoList>? TodoLists { get; set; }
    }
}
