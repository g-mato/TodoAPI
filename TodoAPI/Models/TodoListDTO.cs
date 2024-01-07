using System.Diagnostics.CodeAnalysis;

namespace TodoAPI.Models
{
    public class TodoListDTO
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public List<TodoItemDTO>? TodoItems { get; set; }

        public TodoListDTO() { }

        [SetsRequiredMembers]
        public TodoListDTO(TodoList todoList, bool includeItems = true)
        {
            Id = todoList.Id;
            Title = todoList.Title;
            TodoItems = includeItems ? todoList.TodoItems.Select(item => new TodoItemDTO(item)).ToList() : null;
        }
    }
}
