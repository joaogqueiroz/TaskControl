using System;

namespace TaskControl.Data.Interfaces
{
    // Password reset links. Only a hash of each token is stored, so the database alone
    // cannot be used to reset anyone's password.
    public interface IPasswordResetRepository
    {
        // Saves a new token for the user and discards any unused one they had
        void Create(Guid userId, string tokenHash, DateTime expiresAtUtc);

        // Marks the token as used and returns its user, or null when it is unknown,
        // expired or already used. One statement, so a token can only be used once.
        Guid? Consume(string tokenHash, DateTime nowUtc);
    }
}
