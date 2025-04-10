namespace ToDoList.DTO
{
    public class TaskUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? DueDate { get; set; }
        public List<int>? CategoryIds { get; set; }
    }
}
