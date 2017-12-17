using System.Collections.Generic;

namespace Z2.Models.TodoViewModels
{

    public class CompletedViewModel
    {

        public List<TodoViewModel> Models { get; }

        public CompletedViewModel(List<TodoViewModel> models)
        {
            Models = models;
        }

    }

}
