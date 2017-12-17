using System;

namespace Z2.Models.TodoViewModels
{

    public class AddTodoViewModel
    {
        
        public string Text { get; set; }
        public string Label { get; set; }

        public DateTime? DateDue { get; set; }

    }

}
