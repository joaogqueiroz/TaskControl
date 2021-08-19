using AspNetMVCproject03.Data.Entities;
using AspNetMVCproject03.Data.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;


namespace AspNetMVCproject03.Data.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Create(User user)
        {
            var query = @"
                        INSERT INTO USER_TB(
                            USERID, 
                            NAME, 
                            EMAIL, 
                            PASSWORD, 
                            REGISTRATIONDATE)
                        VALUES(
                            NEWID(), 
                            @Name, 
                            @Email,                               
                            CONVERT(VARCHAR(32), HASHBYTES('MD5', @PassWord), 2), 
                            GETDATE())";                                   //2 FOR HEXADECIMAL
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(query, user);
            }
        
        }

        public void Update(User user)
        {
            var query = @"UPDATE USER_TB
                          SET
                             NAME = @Name
                             EMAIL = @Email
                             PASSWORD = CONVERT(VARCHAR(32), HASHBYTES('MD5', @PassWord), 2)
                          WHERE
                               USERID = @UserID";
            using (var connection = new SqlConnection(_connectionString)) 
            {
                connection.Execute(query, user);
            }
        }

        public void Delete(User user)
        {
            var query = @"DELETE FROM USER_TB
                          WHERE USERID = @UserID";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(query, user);
            }
        }

        public List<User> Read()
        {
            var query = @"SELECT * FROM USER_TB
                          ORDER BY NAME";

            using (var connection = new SqlConnection(_connectionString)) 
            {
                return connection.Query<User>(query).ToList();
            }
        }


        public User GetById(Guid id)
        {
            var query = @"SELECT * FROM USER_TB
                          WHERE USERID = @id";

            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<User>(query, new { id }).FirstOrDefault();
            }
        }

        public User Get(string email)
        {
            var query = @"SELECT * FROM USER_TB
                          WHERE EMAIL = @email";

            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<User>(query, new { email }).FirstOrDefault();
            }
        }

        public User Get(string email, string password)
        {
            var query = @"SELECT * FROM USER_TB
                          WHERE EMAIL = @email
                          AND PASSWORD = CONVERT(VARCHAR(32), HASHBYTES('MD5', @PassWord), 2)
                          ";

            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<User>(query, new { email, password }).FirstOrDefault();
            }
        }
    }
}
