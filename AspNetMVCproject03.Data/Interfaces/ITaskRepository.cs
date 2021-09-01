using AspNetMVCproject03.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AspNetMVCproject03.Data.Interfaces
{
    public interface ITaskRepository
    {
        void Create(Task task);
        void Update(Task task);
        void Delete(Task task);
        List<Task> GetByUser(Guid userid);

        Task GetTaskById(Guid taskid);
    }
}
