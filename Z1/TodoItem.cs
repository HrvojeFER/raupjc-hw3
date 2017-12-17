using System;
using System.Collections.Generic;

namespace Z1
{

    public class TodoItem
    {

        public Guid Id { get; set; }
        public List<TodoLabel> TodoLabels { get; set; }

        public Guid UserId { get; set; }
        public string Text { get; set; }
        public DateTime DateCreated { get; set; }

        public DateTime? DateDue { get; set; }
        public DateTime? DateCompleted { get; set; }

        public TodoItem(string text, Guid userId)
        {
            Text = text ?? throw new ArgumentNullException();
            UserId = userId;

            Id = Guid.NewGuid();
            DateCreated = DateTime.UtcNow;

            TodoLabels = new List<TodoLabel>();
        }

        public TodoItem() : this(string.Empty, Guid.Empty) { }

        public TodoItem(TodoItem item)
        {
            Id = item.Id;
            UserId = item.UserId;
            Text = item.Text;
            DateCreated = item.DateCreated;
        }

        public bool MarkAsCompleted()
        {
            if (DateCompleted.HasValue) return false;
            DateCompleted = DateTime.UtcNow;
            return true;
        }

    }

}
