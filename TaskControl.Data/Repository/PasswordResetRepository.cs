using Dapper;
using System;
using System.Data.SqlClient;
using System.Linq;
using TaskControl.Data.Interfaces;

namespace TaskControl.Data.Repository
{
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly string _connectionString;

        public PasswordResetRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Create(Guid userId, string tokenHash, DateTime expiresAtUtc)
        {
            var query = @"
                        DELETE FROM PASSWORD_RESET_TB
                        WHERE USERID = @userId AND USEDAT IS NULL;

                        INSERT INTO PASSWORD_RESET_TB(TOKENHASH, USERID, EXPIRESAT)
                        VALUES(@tokenHash, @userId, @expiresAtUtc);";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(query, new { userId, tokenHash, expiresAtUtc });
            }
        }

        public Guid? Consume(string tokenHash, DateTime nowUtc)
        {
            var query = @"
                        UPDATE PASSWORD_RESET_TB
                        SET USEDAT = @nowUtc
                        OUTPUT INSERTED.USERID
                        WHERE TOKENHASH = @tokenHash
                          AND USEDAT IS NULL
                          AND EXPIRESAT > @nowUtc";
            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<Guid>(query, new { tokenHash, nowUtc }).Select(id => (Guid?)id).FirstOrDefault();
            }
        }
    }
}
