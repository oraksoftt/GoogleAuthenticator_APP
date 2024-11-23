
//using Newtonsoft.Json;
using GoogleAuthenticator_APP.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;






namespace GoogleAuthenticator_APP
{


    public class DBOperations
    {

        public SqlConnection SqlConn
        {
            get
            {
                return new SqlConnection(ConnectionString);
            }
        }
        private readonly IConfiguration _configration;
        public DBOperations(IConfiguration configuration)
        {
            _configration = configuration;

        }
        private string ConnectionString
        {
            get
            {
                return _configration.GetConnectionString("DefaultConnection");
            }
        }

        public void SaveUser2FA(User2FA user2FA )
        {
            using (var cmd = new SqlCommand("dbo.SP_User2FA_Save", SqlConn))
            {
                try
                {

                    cmd.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    //      cmd.Parameters.AddWithValue("@UserId", user2FA.Id);
                    cmd.Parameters.AddWithValue("@UserEmail", user2FA.Email);
                    cmd.Parameters.AddWithValue("@SecretKey", user2FA.SecretKey);
                    cmd.Parameters.AddWithValue("@Is2FAEnabled", user2FA.Is2FAEnabled);
                    cmd.Parameters.AddWithValue("@Code", user2FA.Code);
                    cmd.Parameters.AddWithValue("@KeyIndex", user2FA.KeyIndex);
                    cmd.Parameters.AddWithValue("@Issuer", user2FA.Issuer);


                    if (cmd.Connection.State != ConnectionState.Open)
                    {
                        cmd.Connection.Open();
                    }


                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff} - Error: {ex.Message}");
                }
                finally
                {
                    // Ensure connection is closed after operation
                    if (cmd.Connection.State != ConnectionState.Closed)
                    {
                        cmd.Connection.Close();
                    }
                }
            }
        }
        public List<User2FA> GetUser2FAKeys(string email)
        {
            var user2FAKeys = new List<User2FA>();


            using (SqlConnection connection = SqlConn)
            {                

                using (SqlCommand cmd = new SqlCommand("SP_User2FA_SelectByEmail", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Email", email);                    

                    if (cmd.Connection.State != ConnectionState.Open)
                    {
                        cmd.Connection.Open();
                    }

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Map the retrieved data to the User2FA model
                            user2FAKeys.Add(new User2FA
                            {
                                Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("Id")),
                                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? string.Empty : reader.GetString(reader.GetOrdinal("Email")),
                                SecretKey = reader.IsDBNull(reader.GetOrdinal("SecretKey")) ? string.Empty : reader.GetString(reader.GetOrdinal("SecretKey")),
                          //      Code = reader.IsDBNull(reader.GetOrdinal("Code")) ? string.Empty : reader.GetString(reader.GetOrdinal("Code")),
                                Is2FAEnabled = !reader.IsDBNull(reader.GetOrdinal("Is2FAEnabled")) && reader.GetBoolean(reader.GetOrdinal("Is2FAEnabled")),
                                KeyIndex = reader.IsDBNull(reader.GetOrdinal("KeyIndex")) ? 0 : reader.GetInt32(reader.GetOrdinal("KeyIndex")),
                             //   CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            //    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                            });

                        }
                        reader.Close();
                    }
                }
            }

            return user2FAKeys;
        }

        public User2FA GetUser2FAByEmailAndKeyIndex(string email, int keyIndex)
        {
            User2FA user2FA = null;

            using (SqlConnection connection = SqlConn)
            {                

                using (SqlCommand cmd = new SqlCommand("SP_User2FA_SelectByEmailAndKeyIndex", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@KeyIndex", keyIndex);

                    if (cmd.Connection.State != ConnectionState.Open)
                    {
                        cmd.Connection.Open();
                    }

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Map the retrieved data to the User2FA model
                            user2FA = new User2FA
                            {
                                Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("Id")),
                                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? string.Empty : reader.GetString(reader.GetOrdinal("Email")),
                                SecretKey = reader.IsDBNull(reader.GetOrdinal("SecretKey")) ? string.Empty : reader.GetString(reader.GetOrdinal("SecretKey")),
                            //    Code = reader.IsDBNull(reader.GetOrdinal("Code")) ? string.Empty : reader.GetString(reader.GetOrdinal("Code")),
                                Is2FAEnabled = !reader.IsDBNull(reader.GetOrdinal("Is2FAEnabled")) && reader.GetBoolean(reader.GetOrdinal("Is2FAEnabled")),
                                KeyIndex = reader.IsDBNull(reader.GetOrdinal("KeyIndex")) ? 0 : reader.GetInt32(reader.GetOrdinal("KeyIndex")),
                          //      CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                          //      UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                            };

                        }
                        reader.Close();
                    }
                }
            }

            return user2FA;
        }

         /*
        public User2FA GetUser2FABySecretKey(string secretKey)
        {
            User2FA user2FA = null;


            using (SqlConnection connection = SqlConn)
            {


                using (SqlCommand cmd = new SqlCommand("SP_User2FA_SelectBySecretKey", connection))
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    //cmd.Parameters.AddWithValue("@SecretKey", secretKey);

                    cmd.Parameters.AddWithValue("@SecretKey", string.IsNullOrEmpty(secretKey) ? DBNull.Value : secretKey);


                    if (cmd.Connection.State != ConnectionState.Open)
                    {
                        cmd.Connection.Open();
                    }

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Map the retrieved data to the User2FA model
                            user2FA = new User2FA
                            {
                                Id = reader.IsDBNull(reader.GetOrdinal("Id")) ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("Id")),
                                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? string.Empty : reader.GetString(reader.GetOrdinal("Email")),
                                SecretKey = reader.IsDBNull(reader.GetOrdinal("SecretKey")) ? string.Empty : reader.GetString(reader.GetOrdinal("SecretKey")),
                           //     Code = reader.IsDBNull(reader.GetOrdinal("Code")) ? string.Empty : reader.GetString(reader.GetOrdinal("Code")),
                                Is2FAEnabled = !reader.IsDBNull(reader.GetOrdinal("Is2FAEnabled")) && reader.GetBoolean(reader.GetOrdinal("Is2FAEnabled")),
                             //   CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                             //   UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                            };

                        }
                        reader.Close();
                    }
                    if (cmd.Connection.State != ConnectionState.Closed)
                    {
                        cmd.Connection.Close();
                    }
                }
            }

            return user2FA;
        }

        */
    }
}

