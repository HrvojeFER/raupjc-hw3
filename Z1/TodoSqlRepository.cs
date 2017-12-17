using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Z1
{
    public class TodoSqlRepository : ITodoRepository
    {

        private readonly TodoDbContext _context;

        public TodoSqlRepository(TodoDbContext context)
        {
            _context = context;
        }

        public TodoItem Get(Guid todoId, Guid userId)
        {
            var item = _context.Items.Find(todoId);

            if (item == null) return null;

            var itemUserId = _context.Entry(item).Property(x => x.UserId).CurrentValue;
            if (!itemUserId.Equals(userId))
            {
                throw new TodoAccessDeniedException(
                    $"access denied: given user id({userId}) is not equal to user id({itemUserId}) in database");
            }

            return _context.Entry(item).Entity;
        }

        public void Add(TodoItem todoItem)
        {
            if (_context.Items.Find(todoItem.Id) != null)
            {
                throw new DuplicateTodoItemException($"duplicate id : {todoItem.Id}");
            }

            _context.Entry(todoItem).State = EntityState.Added;
            _context.SaveChanges();
        }

        public void AddLabel(string value, Guid id)
        {
            if (_context.Labels.Count(label => label.Value == value && label.Id == id) == 0)
            {
                _context.Labels.Add(new TodoLabel(value));
            }
        }

        public bool Remove(Guid todoId, Guid userId)
        {
            var item = _context.Items.Find(todoId);

            if (item == null) return false;

            var itemUserId = _context.Entry(item).Property(x => x.UserId).CurrentValue;
            if (!itemUserId.Equals(userId))
            {
                throw new TodoAccessDeniedException(
                    $"access denied: given user id({userId}) is not equal to user id({itemUserId}) in database");
            }

            _context.Entry(item).State = EntityState.Deleted;
            _context.SaveChanges();
            return true;
        }

        public void Update(TodoItem todoItem, Guid userId)
        {
            var item = _context.Items.Find(todoItem.Id);

            if (item == null)
            {
                Add(todoItem);
                return;
            }

            var itemUserId = _context.Entry(item).Property(x => x.UserId).CurrentValue;
            if (!itemUserId.Equals(userId))
            {
                throw new TodoAccessDeniedException(
                    $"access denied: given user id({userId}) is not equal to user id({itemUserId}) in database");
            }

            item.UserId = todoItem.UserId;
            item.DateCreated = todoItem.DateCreated;
            item.Text = todoItem.Text;

            _context.Entry(item).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public bool MarkAsCompleted(Guid todoId, Guid userId)
        {
            var item = _context.Items.Find(todoId);

            if (item == null) return false;

            var itemUserId = _context.Entry(item).Property(x => x.UserId).CurrentValue;
            if (!itemUserId.Equals(userId))
            {
                throw new TodoAccessDeniedException(
                    $"access denied: given user id({userId}) is not equal to user id({itemUserId}) in database");
            }

            var sol = _context.Entry(item).Entity.MarkAsCompleted();
            _context.Entry(item).State = EntityState.Modified;
            _context.SaveChanges();
            return sol;
        }

        public List<TodoItem> GetAll(Guid userId)
        {
            return _context.Items
                .Where(item => item.UserId.Equals(userId))
                .OrderByDescending(item => item.DateCreated).ToList();
        }

        public List<TodoItem> GetActive(Guid userId)
        {
            return GetFiltered(item => !item.DateCompleted.HasValue, userId);
        }

        public List<TodoItem> GetCompleted(Guid userId)
        {
            return GetFiltered(item => item.DateCompleted.HasValue, userId);
        }

        public List<TodoItem> GetFiltered(Func<TodoItem, bool> filterFunction, Guid userId)
        {
            var items = new List<TodoItem>();
            
            for (var i = 0 ; i < _context.Items.Count() ; ++i)
            {
                var item = _context.Items.ElementAt(i);
                if (item.UserId == userId && filterFunction(item)) items.Add(item);
            }

            return items;
        }
    }

    public class TodoAccessDeniedException : Exception
    {
        public TodoAccessDeniedException() { }

        public TodoAccessDeniedException(string message) : base(message) { }
    }

    public class DuplicateTodoItemException : Exception
    {
        public DuplicateTodoItemException() { }

        public DuplicateTodoItemException(string message) : base(message) { }
    }
}