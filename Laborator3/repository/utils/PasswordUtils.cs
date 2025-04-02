using System;
using BCrypt.Net;

namespace Laborator3.utils
{
    
    public class PasswordUtils
    {
        // The default work factor for BCrypt
        private const int WorkFactor = 12;

        /// <summary>
        /// Hashes a password using BCrypt
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>A BCrypt hash string</returns>
        public static string HashPassword(string password)
        {
            // Generate a salt and hash the password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
            return hashedPassword;
        }

        /// <summary>
        /// Verifies a password against a stored BCrypt hash
        /// </summary>
        /// <param name="password">The password to verify</param>
        /// <param name="hashedPassword">The stored password hash</param>
        /// <returns>True if the password matches, false otherwise</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // Verify the password against the hash
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        /// <summary>
        /// Simple method to provide backwards compatibility with older plaintext passwords
        /// </summary>
        /// <param name="plainPassword">The plaintext password entered by the user</param>
        /// <param name="storedPassword">The password stored in the database</param>
        /// <returns>True if the passwords match (either directly or via hash verification)</returns>
        public static bool VerifyPasswordWithFallback(string plainPassword, string storedPassword)
        {
            // First try to verify as a BCrypt hashed password
            try
            {
                // Check if it looks like a BCrypt hash (starts with $2a$, $2b$, or $2y$)
                if (storedPassword.StartsWith("$2"))
                {
                    return VerifyPassword(plainPassword, storedPassword);
                }
            }
            catch
            {
                // If verification fails, fall back to plain comparison
            }

            // Fallback to plain comparison for legacy passwords
            return plainPassword == storedPassword;
        }
    }
}