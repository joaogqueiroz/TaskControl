using TaskControl.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskControl.Data.Interfaces
{
    public interface ITaskRepository
    {
        void Create(Task task);
        void Update(Task task);
        void Delete(Task task);
        List<Task> GetByUser(Guid userid);
        List<Task> GetByUserAndPeriod(Guid userid, DateTime startDate, DateTime finishDate);

        Task GetTaskById(Guid taskid);
    }
}
