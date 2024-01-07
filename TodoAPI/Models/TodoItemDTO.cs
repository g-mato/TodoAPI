using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TodoAPI.Models
{
    public class TodoItemDTO
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Text { get; set; }
        public required DateTime Deadline { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TodoItemFlag Flag { get; set; }
        public TodoUserDTO? CreatedBy { get; set; }

        public TodoItemDTO()
        {
        }

        [SetsRequiredMembers]
        public TodoItemDTO(TodoItem todoItem)
        {
            Id = todoItem.Id;
            Title = todoItem.Title;
            Text = todoItem.Text;
            Flag = todoItem.Flag;
            Deadline = todoItem.Deadline;
            CreatedBy = new TodoUserDTO()
            {
                name = todoItem.CreatedBy.UserName ?? "[UNKNOWN]"
            };
        }
    }
}
