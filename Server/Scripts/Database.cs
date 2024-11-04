using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using System.Data;

class Database


{
    static string mConnectString;
    static CancellationTokenSource mCancelToken;
    static CancellationToken CancelToken => mCancelToken.Token;

    public static async Task Init()
    {
        string host = "localhost";

        mConnectString = GetConnectionString(host);

        mCancelToken = new CancellationTokenSource();

        using (var conn = OpenConn().Result)
        {

        }
        Form.Inst.AddLog($"DB 연결됨... ({host})");
    }

    public static async Task AccountUpdate(string id, string data)
    {
        var check = await Check(TableName.user, id);

        Form.Inst.AddLog($"AccountUpdate , id {id} , data :{data}");
        if (check)
        {
            await Update(TableName.user, id, data);
        }
        else
        {
            await Insert(TableName.user, id, data);
            
        }
    }
    static async Task<MySqlConnection> OpenConn()
    {
        var conn = new MySqlConnection(mConnectString);

        await conn.OpenAsync(CancelToken);

        return conn;
    }
    static string GetConnectionString(string host)
    {
        var option = new Dictionary<string, object>();

        // Basic
        option["Server"] = host;
        option["Port"] = 3306;
        option["Database"] = "test1";
        option["Uid"] = "root";
        option["Password"] = "123456";

        return String.Join(";", option.Select(x => $"{x.Key}={x.Value}"));
    }
    public enum TableName
    {
        user,
    }
    public static async Task<bool> Insert(TableName name, string id, string data)
    {
        using (var conn = await OpenConn())
        {
            string sql = $"INSERT INTO {name.ToString()} (id,data,loginTime) VALUES (@ID,@DATA,@Time)";

            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.Parameters.AddWithValue("@DATA", data);
                cmd.Parameters.AddWithValue("@Time", DateTime.UtcNow);

                int rows = await cmd.ExecuteNonQueryAsync();

                return rows == 1;
            }
        }
    }

    public static async Task<string?> Select(TableName name, string id)
    {
        string sql = $"SELECT COUNT(*) FROM {name.ToString()} WHERE id = '{id}'";

        try
        {
            using (var conn = await OpenConn())
            {
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync(CancelToken))
                    {
                        if (await reader.ReadAsync(CancelToken))
                        {
                            return (string)reader.GetValue(0);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Form.Inst.AddLog($"{e.Message.ToString()}");
            return null;
        }
        

        return default;
    }

    public static async Task<bool> Check(TableName name, string id)
    {
        string sql = $"SELECT COUNT(*) FROM {name.ToString()} WHERE id = '{id}'";

        using (var conn = await OpenConn())
        {
            using (var cmd = new MySqlCommand(sql, conn))
            {
                using (var reader = await cmd.ExecuteReaderAsync(CancelToken))
                {
                    if(await reader.ReadAsync(CancelToken))
                    {
                        return reader.GetValue(0).ToString()=="1";
                    }
                    
                }
            }
        }

        return false;
    }

    public static async Task<bool> Update(TableName name, string id, string data)
    {
        using (var conn = await OpenConn())
        {
            string sql = $"UPDATE {name.ToString()} SET data=@data,loginTime=@time WHERE id=@id";

            using (var cmd = new MySqlCommand(sql, conn))
            {
                ////cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@data", data);
                cmd.Parameters.AddWithValue("@time", DateTime.UtcNow);
                int rows = cmd.ExecuteNonQuery();

                //return rows == 1;
                return true;
            }
        }
    }
}

