namespace TodoAPI.Models
{
    public class TodoItem
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Text { get; set; }
        public required TodoItemFlag Flag { get; set; }
        public required DateTime Deadline { get; set; }
        public required TodoUser CreatedBy { get; set; }
    }

    public enum TodoItemFlag
    {
        Active,
        Finished,
        Canceled
    }
}
