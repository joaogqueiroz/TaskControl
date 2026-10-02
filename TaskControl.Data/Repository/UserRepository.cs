using TaskControl.Data.Entities;
using TaskControl.Data.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;


namespace TaskControl.Data.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

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
                            @PassWord,
                            GETDATE())";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(query, new { user.Name, user.Email, PassWord = _passwordHasher.HashPassword(user, user.PassWord) });
            }
        
        }

        public void Update(User user)
        {
            var query = @"UPDATE USER_TB
                          SET
                             NAME = @Name,
                             EMAIL = @Email,
                             PASSWORD = @PassWord
                          WHERE
                               USERID = @UserID";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(query, new { user.Name, user.Email, PassWord = _passwordHasher.HashPassword(user, user.PassWord), user.UserID });
            }
        }
        public void Update(Guid userId, string newPassWord)
        {
            var query = @"UPDATE USER_TB
                          SET                             
                             PASSWORD = @newPassWord
                          WHERE
                               USERID = @userId";
            using (var connection = new SqlConnection(_connectionString))
            {
                var hashedPassWord = _passwordHasher.HashPassword(new User { UserID = userId }, newPassWord);
                connection.Execute(query, new { userId, newPassWord = hashedPassWord });
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


        public User GetById(Guid userId)
        {
            var query = @"SELECT * FROM USER_TB
                          WHERE USERID = @userId";

            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<User>(query, new { userId }).FirstOrDefault();
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
            var user = Get(email);
            if (user == null)
            {
                return null;
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PassWord, password);
            return result == PasswordVerificationResult.Failed ? null : user;
        }


    }
}
