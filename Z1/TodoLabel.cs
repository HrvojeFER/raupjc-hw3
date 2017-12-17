using System;
using System.Collections.Generic;

namespace Z1
{

    public class TodoLabel
    {

        public Guid Id { get; set; }
        public List<TodoItem> TodoItems { get; set; }

        public string Value { get; set; }

        public TodoLabel(string value)
        {
            Value = value ?? throw new ArgumentNullException();

            Id = Guid.NewGuid();
            TodoItems = new List<TodoItem>();
        }

    }

}
