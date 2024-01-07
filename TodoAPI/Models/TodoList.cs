namespace TodoAPI.Models
{
    public class TodoList
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public List<TodoItem> TodoItems { get; } = new List<TodoItem>();
        public List<TodoUser> TodoUsers { get; } = new List<TodoUser>();
    }
}
