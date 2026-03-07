using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using cs464_project.Model;

namespace cs464_project.DataAccess
{
    public static class DbHelper
    {
        public static QLCH1Entities GetContext()
        {
            return new QLCH1Entities();
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["QLCH"].ConnectionString);
        }

        public static bool VerifyPassword(string password, byte[] storedHash, byte[] salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltedPassword = new byte[salt.Length + passwordBytes.Length];
            Buffer.BlockCopy(salt, 0, saltedPassword, 0, salt.Length);
            Buffer.BlockCopy(passwordBytes, 0, saltedPassword, salt.Length, passwordBytes.Length);

            using (var sha256 = SHA256.Create())
            {
                byte[] computedHash = sha256.ComputeHash(saltedPassword);

                if (computedHash.Length != storedHash.Length)
                    return false;

                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i])
                        return false;
                }

                return true;
            }
        }
    }
}
