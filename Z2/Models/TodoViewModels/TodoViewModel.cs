using System;

namespace Z2.Models.TodoViewModels
{

    public class TodoViewModel
    {

        public Guid Id { get; set; }
        public string Text { get; set; }
        public DateTime? DateDue { get; set; }
        public bool IsCompleted { get; set; }

        public TodoViewModel(Guid id, string text, DateTime? dateDue, bool isCompleted)
        {
            Id = id;
            Text = text;
            DateDue = dateDue;
            IsCompleted = isCompleted;
        }

        public string Remaining()
        {
            if (DateDue == null)
                return string.Empty;

            if (DateDue < DateTime.Now)
                return "You're overdue.";

            var left = (DateTime)DateDue - DateTime.Now;
            
            return left.ToString();
        }

        public bool ShowReminder() => !DateDue.Equals(null)
                                      && DateDue > DateTime.Now
                                      && ((DateTime)DateDue - DateTime.Now).Days < 7;

    }

}
