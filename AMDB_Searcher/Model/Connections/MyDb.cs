using AMDB_Searcher.Model.Constant;
using AMDB_Searcher.Model.Constants;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace AMDB_Searcher.Model
{
    public class MyDb
    {
        private readonly List<Device> deviceList;
        private readonly List<string> userList;
        private readonly string connectionString;
        private readonly string dbServer;
        private readonly string dbName;
        private readonly string dbUser;
        private readonly string dbPassword;
        private readonly string tableName;
        private readonly string columName;

        public MyDb()
        {
            this.userList = new List<string>();
            this.deviceList = new List<Device>();
            this.dbServer = Credentials.dbServer;
            this.dbName = Credentials.dbName;
            this.dbUser = Credentials.dbUser;
            this.dbPassword = Credentials.dbPassword;
            this.tableName = DefaultValues.tableObject;
            this.columName = string.Empty;
            this.connectionString = $"Data Source={this.dbServer}; initial catalog={this.dbName}; user id={this.dbUser}; password={this.dbPassword};";
        }

        public List<Device> Search(string searchText, int changedColumn)
        {
            string sqlSearchRequest = MySQLEscape(searchText);
            string sqlExpression = string.Empty;

            switch (changedColumn)
            {
                case 0:
                    sqlExpression = $"SELECT * FROM {this.tableName} WHERE {DefaultValues.columnComment} LIKE '%{sqlSearchRequest}%'";
                    break;
                case 1:
                    sqlExpression = $"SELECT * FROM {this.tableName} WHERE {DefaultValues.columnUserId} In (SELECT {DefaultValues.columnId} FROM {DefaultValues.tableUser} WHERE {DefaultValues.columnFullName} LIKE '%{sqlSearchRequest}%')";
                    break;
                case 2:
                    sqlExpression = $"SELECT * FROM {this.tableName} WHERE {DefaultValues.columnInventoryNumber} LIKE '%{sqlSearchRequest}%'";
                    break;
                default:
                    return null;
            }

            this.deviceList.Clear();

            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string inventoryNumber = @reader.GetValue(2).ToString();
                        string equipmentName = @reader.GetValue(3).ToString();
                        string comment = @reader.GetValue(21).ToString();

                        if (inventoryNumber.Trim() == string.Empty)
                            inventoryNumber = @"N/A";

                        if (equipmentName.Trim() == string.Empty)
                            equipmentName = @"unknown device";

                        if (comment.Trim() == string.Empty)
                            comment = @"No additional information.";

                        var newDevice = new Device(inventoryNumber, equipmentName, comment);
                        this.deviceList.Add(newDevice);
                    }
                }

                reader.Close();

                List<Device> sortedList = this.deviceList.OrderByDescending(o => o.InventoryId).ToList();
                return sortedList;
            }
        }

        public List<string> GetUserAccessList()
        {
            string sqlExpression = $"SELECT [Login] FROM [Access_Level] AS al JOIN [User] AS u ON [al].[UserId] = [u].[Id] ORDER BY [UserId]";

            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string login = @reader.GetValue(0).ToString();
                        this.userList.Add(login);
                    }
                }

                reader.Close();

                return this.userList;
            }
        }

        private static string MySQLEscape(string str)
        {
            return str.Replace("'", "").Replace("\\", "");
        }
    }
}
