using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace cs464_project.DataAccess
{
    public static class DbHelper
    {
        public static string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["QLCH"].ConnectionString;
            }
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static byte[] HashPassword(string password, byte[] salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] pwBytes = Encoding.UTF8.GetBytes(password);
                byte[] combined = salt.Concat(pwBytes).ToArray();
                return sha256.ComputeHash(combined);
            }
        }

        public static bool VerifyPassword(string password, byte[] storedHash, byte[] salt)
        {
            byte[] computed = HashPassword(password, salt);
            return computed.SequenceEqual(storedHash);
        }
    }
}
