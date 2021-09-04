using AspNetMVCproject03.Data.Interfaces;
using AspNetMVCproject03.Data.Entities;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace AspNetMVCproject03.Data.Repository
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

        public void Update(Task task)
        {
            var query = @" 
                UPDATE TASK_TB SET
                    NAME = @Name,
                    DATE = @Date,
                    HOUR = @Hour,
                    DESCRIPTION = @Description,
                    PRIORITY = @Priority
                WHERE
                    TASKID = @TaskID";
            using (var connetionString = new SqlConnection(_connectionString))
            {
                connetionString.Execute(query, task);
            }

        }

        public void Delete(Task task)
        {
            var query = @"
                        DELETE FROM TASK_TB
                        WHERE TASKID = @TaskID";
            using (var connetionString = new SqlConnection(_connectionString))
            {
                connetionString.Execute(query, task);
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

        public Task GetTaskById(Guid taskid)
        {
            var query = @"
                        SELECT * FROM TASK_TB
                        WHERE taskid = @taskid";
            using (var connetionString = new SqlConnection(_connectionString))
            {
                return connetionString.Query<Task>(query, new { taskid }).FirstOrDefault();
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
