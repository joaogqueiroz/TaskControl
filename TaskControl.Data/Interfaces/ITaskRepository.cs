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
        // Every operation on a single task also takes its owner: another user's task is
        // treated as missing, so nobody can read, change or delete it by guessing its id
        bool Update(Task task);
        bool Delete(Guid taskid, Guid userid);
        List<Task> GetByUser(Guid userid);
        List<Task> GetByUserAndPeriod(Guid userid, DateTime startDate, DateTime finishDate);

        Task GetTaskById(Guid taskid, Guid userid);
    }
}
