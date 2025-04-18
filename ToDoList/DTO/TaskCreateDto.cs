﻿namespace ToDoList.DTO
{
    public class TaskCreateDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public List<int>? CategoryIds { get; set; }
    }
}
