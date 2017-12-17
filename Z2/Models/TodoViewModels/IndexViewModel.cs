using System.Collections.Generic;

namespace Z2.Models.TodoViewModels
{

    public class IndexViewModel
    {

        public List<TodoViewModel> Models { get; }

        public IndexViewModel(List<TodoViewModel> models)
        {
            Models = models;
        }

    }

}
