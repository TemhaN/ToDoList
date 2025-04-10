namespace ToDoList.Models
{
    public class GlobalCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<TaskCategory> TaskCategories { get; set; } = new();
    }
}