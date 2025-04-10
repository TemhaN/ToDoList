namespace ToDoList.Models
{
    public class UserCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public List<TaskCategory> TaskCategories { get; set; } = new();
    }
}