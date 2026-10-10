using TaskControl.Data.Interfaces;
using TaskControl.Data.Entities;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace TaskControl.Data.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly string _connectionString;

        public TaskRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Create(Task task)
        {
            var query = @"
                INSERT INTO TASK_TB(
                    TASKID,
                    NAME,
	                DATE,
	                HOUR,
	                DESCRIPTION,
	                PRIORITY,
	                USERID)
                VALUES(
                    NEWID(),
                    @Name,
                    @Date,
                    @Hour,
                    @Description,
                    @Priority,
                    @UserID)";
            using (var connetionString = new SqlConnection(_connectionString)) 
            {
                connetionString.Execute(query, task);
            }
        }

        // Only the owner's task is changed (task.UserID); returns false when there is none
        public bool Update(Task task)
        {
            var query = @" 
                UPDATE TASK_TB SET
                    NAME = @Name,
                    DATE = @Date,
                    HOUR = @Hour,
                    DESCRIPTION = @Description,
                    PRIORITY = @Priority
                WHERE
                    TASKID = @TaskID
                    AND USERID = @UserID";
            using (var connetionString = new SqlConnection(_connectionString))
            {
                return connetionString.Execute(query, task) > 0;
            }

        }

        public bool Delete(Guid taskid, Guid userid)
        {
            var query = @"
                        DELETE FROM TASK_TB
                        WHERE TASKID = @taskid
                          AND USERID = @userid";
            using (var connetionString = new SqlConnection(_connectionString))
            {
                return connetionString.Execute(query, new { taskid, userid }) > 0;
            }
        }

        public List<Task> GetByUser(Guid userid)
        {
            var query = @"
                        SELECT * FROM TASK_TB
                        WHERE USERID = @userid
                        ORDER BY DATE, HOUR DESC";
            using (var connetionString = new SqlConnection(_connectionString))
            {
               return connetionString.Query<Task>(query, new { userid }).ToList();
            }
        }

        public Task GetTaskById(Guid taskid, Guid userid)
        {
            var query = @"
                        SELECT * FROM TASK_TB
                        WHERE taskid = @taskid
                          AND userid = @userid";
            using (var connetionString = new SqlConnection(_connectionString))
            {
                return connetionString.Query<Task>(query, new { taskid, userid }).FirstOrDefault();
            }
        }

        public List<Task> GetByUserAndPeriod(Guid userid, DateTime startDate, DateTime finishDate)
        {
            var query = @"
                        SELECT * FROM TASK_TB
                        WHERE USERID = @userid
                        AND DATE BETWEEN @startDate AND @finishDate
                        ORDER BY DATE, HOUR DESC";
            using (var connetionString = new SqlConnection(_connectionString))
            {
                return connetionString.Query<Task>(query, new { userid, startDate ,finishDate }).ToList();
            }
        }
    }
}
