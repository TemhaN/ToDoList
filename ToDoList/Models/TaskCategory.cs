namespace ToDoList.Models
{
    public enum CategoryType
    {
        Global = 1,
        User = 2
    }

    public class TaskCategory
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public TaskItem Task { get; set; }
        public CategoryType CategoryType { get; set; }
        public int? GlobalCategoryId { get; set; }
        public GlobalCategory GlobalCategory { get; set; }
        public int? UserCategoryId { get; set; }
        public UserCategory UserCategory { get; set; }
    }
}