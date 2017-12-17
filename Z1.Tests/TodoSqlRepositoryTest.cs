using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Z1.Tests
{

    [TestClass]
    public class TodoSqlRepositoryTest
    {

        private const int ItemInstancesNo = 20;
        private const int RandomElementNo = 5;
        private const string SampleText = "sample text";

        private ICollection<TodoItem> _itemList;

        private TodoSqlRepository _repo;

        private void InitTest()
        {
            _repo = new TodoSqlRepository(new TodoDbContext());
            _itemList = new List<TodoItem>();

            for (var i = 0; i < ItemInstancesNo / 2; ++i)
            {
                _itemList.Add(new TodoItem($"{SampleText}{i}", Guid.NewGuid()));
                _repo.Add(_itemList.ElementAt(i));
            }

            var userGuid = Guid.NewGuid();
            for (var i = ItemInstancesNo / 2; i < ItemInstancesNo; ++i)
            {
                _itemList.Add(new TodoItem($"{SampleText}{i}", userGuid));
                _repo.Add(_itemList.ElementAt(i));
                Thread.Sleep(1000);
            }
        }

        [TestMethod]
        public void GetTest()
        {
            InitTest();

            Assert.IsNull(_repo.Get(Guid.NewGuid(), Guid.NewGuid()));

            try
            {
                _repo.Get(_itemList.ElementAt(RandomElementNo).Id, Guid.NewGuid());
                Assert.Fail();
            }
            catch (TodoAccessDeniedException) { }
            
            using (var context = new TodoDbContext())
            {
                Assert.AreEqual(context.Entry(_itemList.ElementAt(RandomElementNo)).Entity.Id, 
                    _repo.Get(_itemList.ElementAt(RandomElementNo).Id, _itemList.ElementAt(RandomElementNo).UserId).Id);
            }
        }

        [TestMethod]
        public void AddTest()
        {
            InitTest();

            try
            {
                _repo.Add(_itemList.ElementAt(RandomElementNo));
                Assert.Fail();
            }
            catch (DuplicateTodoItemException) { }

            var addItem = new TodoItem(SampleText, Guid.NewGuid());
            _repo.Add(addItem);
            Assert.AreEqual(_repo.Get(addItem.Id, addItem.UserId).Id, addItem.Id);
        }

        [TestMethod]
        public void RemoveTest()
        {
            InitTest();
            
            Assert.IsFalse(_repo.Remove(Guid.NewGuid(), Guid.NewGuid()));

            try
            {
                _repo.Remove(_itemList.ElementAt(RandomElementNo).Id, Guid.NewGuid());
                Assert.Fail();
            }
            catch (TodoAccessDeniedException) { }

            _repo.Remove(_itemList.ElementAt(RandomElementNo).Id, _itemList.ElementAt(RandomElementNo).UserId);
            Assert.IsNull(_repo.Get(_itemList.ElementAt(RandomElementNo).Id, Guid.Empty));
        }

        [TestMethod]
        public void UpdateTest()
        {
            InitTest();

            var updateItem = new TodoItem(SampleText, Guid.NewGuid());
            _repo.Update(updateItem, Guid.NewGuid());
            Assert.AreEqual(_repo.Get(updateItem.Id, updateItem.UserId).Id, updateItem.Id);

            try
            {
                _repo.Update(_itemList.ElementAt(RandomElementNo), Guid.NewGuid());
                Assert.Fail();
            }
            catch (TodoAccessDeniedException) { }
            
            using (var context = new TodoDbContext())
            {
                var item = context.Items.Find(_itemList.ElementAt(RandomElementNo).Id) ??
                    throw new NullReferenceException();
                item.Text = SampleText;
                _repo.Update(item, item.UserId);
                Assert.AreEqual(context.Entry(item).Entity.Text, SampleText);
            }
        }

        [TestMethod]
        public void MarkAsCompletedTest()
        {
            InitTest();

            Assert.IsFalse(_repo.MarkAsCompleted(Guid.NewGuid(), Guid.NewGuid()));

            try
            {
                _repo.MarkAsCompleted(_itemList.ElementAt(RandomElementNo).Id, Guid.NewGuid());
                Assert.Fail();
            }
            catch (TodoAccessDeniedException) { }

            using (var context = new TodoDbContext())
            {
                var item = context.Items.Find(_itemList.ElementAt(RandomElementNo).Id) ?? throw new NullReferenceException();
                Assert.IsTrue(_repo.MarkAsCompleted(item.Id, item.UserId));
                Assert.IsTrue(_repo.Get(item.Id, item.UserId).IsCompleted);

                Assert.IsFalse(_repo.MarkAsCompleted(item.Id, item.UserId));
            }
        }

        [TestMethod]
        public void GetAllTest()
        {
            InitTest();

            Assert.AreEqual(_repo.GetAll(Guid.NewGuid()).Count, 0);

            Assert.AreEqual(_repo.GetAll(_itemList.ElementAt(RandomElementNo).UserId).Count, 1);

            var items = _repo.GetAll(_itemList.ElementAt(ItemInstancesNo - 1).UserId);
            Assert.AreEqual(items.Count, ItemInstancesNo / 2 + ItemInstancesNo % 2);

            for (var i = 0 ; i < items.Count - 1 ; ++i)
            {
                Assert.IsTrue(items.ElementAt(i).DateCreated > items.ElementAt(i + 1).DateCreated);
            }
        }

        [TestMethod]
        public void GetActiveTest()
        {
            InitTest();

            Assert.AreEqual(_repo.GetActive(Guid.NewGuid()).Count, 0);

            var items = _repo.GetActive(_itemList.ElementAt(ItemInstancesNo - 1).UserId);
            Assert.AreEqual(items.Count, ItemInstancesNo / 2 + ItemInstancesNo % 2);
            foreach (var item in items)
            {
                Assert.IsTrue(!item.IsCompleted);
            }
        }

        [TestMethod]
        public void GetCompletedTest()
        {
            InitTest();

            Assert.AreEqual(_repo.GetCompleted(Guid.NewGuid()).Count, 0);

            _repo.MarkAsCompleted(_itemList.ElementAt(ItemInstancesNo - 2).Id,
                _itemList.ElementAt(ItemInstancesNo - 2).UserId);
            _repo.MarkAsCompleted(_itemList.ElementAt(ItemInstancesNo - 3).Id,
                _itemList.ElementAt(ItemInstancesNo - 3).UserId);
            var items = _repo.GetCompleted(_itemList.ElementAt(ItemInstancesNo - 1).UserId);
            Assert.AreEqual(items.Count, 2);
            foreach (var item in items)
            {
                Assert.IsTrue(item.IsCompleted);
            }
        }

        [TestMethod]
        public void GetFilteredTest()
        {
            InitTest();

            Assert.AreEqual(_repo.GetFiltered(item => !item.IsCompleted, Guid.NewGuid()).Count, 0);
            
            var items = _repo.GetFiltered(item => !item.IsCompleted, _itemList.ElementAt(ItemInstancesNo - 1).UserId);
            Assert.AreEqual(items.Count, ItemInstancesNo / 2 + ItemInstancesNo % 2);
            foreach (var item in items)
            {
                Assert.IsTrue(!item.IsCompleted);
            }
        }

    }

}
