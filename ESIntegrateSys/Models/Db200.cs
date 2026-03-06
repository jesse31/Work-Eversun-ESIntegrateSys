using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

namespace ESIntegrateSys.Models
{
    /// <summary>
    /// AMES 1.0 資料庫操作類別
    /// </summary>
    public class Db200
    {
        #region paramater method

        // 開發測試連線字串
        //private static readonly String connStr = "server=localhost;database=AMES_DB;uid=sa;pwd=A12345678;Connect Timeout = 480";
        // 正式環境連線字串
        //private static readonly String connStr = "server=192.168.4.200;database=AMES_DB;uid=fa;pwd=fa;Connect Timeout = 480";

        /// <summary>
        /// AMES 1.0 連線字串，從組態檔取得
        /// </summary>
        private static readonly String connStr = System.Configuration.ConfigurationManager.ConnectionStrings["connStr"].ConnectionString;

        /// <summary>
        /// 執行 SQL 指令 (insert/update/delete)，回傳受影響的資料列數
        /// </summary>
        /// <param name="sql">要執行的 SQL 語句或儲存過程名稱</param>
        /// <param name="cmdType">命令型態 (Text 或 StoredProcedure)</param>
        /// <param name="pms">SQL 參數陣列</param>
        /// <returns>受影響的資料列數</returns>
        public static int ExecueNonQuery(string sql, CommandType cmdType, params SqlParameter[] pms)
        {
            // 建立 SQL 連線物件
            using (SqlConnection con = new SqlConnection(connStr))
            {
                // 建立 SQL 命令物件
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    // 設定命令型態 (SQL 或儲存過程)
                    cmd.CommandType = cmdType;
                    // 若有參數則加入命令物件
                    if (pms != null)
                    {
                        cmd.Parameters.AddRange(pms);
                    }

                    // 開啟資料庫連線
                    con.Open();
                    // 執行命令並回傳受影響的資料列數
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 執行 SQL 查詢並回傳 SqlDataReader 物件
        /// </summary>
        /// <param name="sql">要執行的 SQL 語句或儲存過程名稱</param>
        /// <param name="cmdType">命令型態 (Text 或 StoredProcedure)</param>
        /// <param name="pms">SQL 參數陣列</param>
        /// <returns>SqlDataReader 物件，包含查詢結果</returns>
        public static SqlDataReader ExecuteReader(string sql, CommandType cmdType, params SqlParameter[] pms)
        {
            // 建立 SQL 連線物件
            SqlConnection con = new SqlConnection(connStr);
            // 使用 SQL 命令物件
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                // 設定命令型態 (SQL 或儲存過程)
                cmd.CommandType = cmdType;
                // 若有參數則加入命令物件
                if (pms != null)
                {
                    cmd.Parameters.AddRange(pms);
                }
                try
                {
                    // 開啟資料庫連線
                    con.Open();
                    // 執行查詢並回傳 SqlDataReader，查詢結束自動關閉連線
                    return cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch
                {
                    // 發生例外時關閉並釋放連線資源
                    con.Close();
                    con.Dispose();
                    throw;
                }
            }
        }

        /// <summary>
        /// 執行 SQL 查詢並回傳 DataTable 物件
        /// </summary>
        /// <param name="sql">要執行的 SQL 語句或儲存過程名稱</param>
        /// <param name="cmdType">命令型態 (Text 或 StoredProcedure)</param>
        /// <param name="pms">SQL 參數陣列</param>
        /// <returns>查詢結果的 DataTable 物件</returns>
        public static DataTable ExecuteDataTable(string sql, CommandType cmdType, params SqlParameter[] pms)
        {
            // 建立 DataTable 物件，用來儲存查詢結果
            DataTable dt = new DataTable();
            // 使用 SqlDataAdapter，自動建立 SQL 連線，不需手動建立 Connection
            using (SqlDataAdapter adapter = new SqlDataAdapter(sql, connStr))
            {
                // 設定命令型態 (SQL 或儲存過程)
                adapter.SelectCommand.CommandType = cmdType;
                if (pms != null)
                {
                    // 逐一處理 SQL 參數，避免參數重複使用造成錯誤
                    foreach (var param in pms)
                    {
                        // 複製參數物件，並設定參數屬性
                        var newParam = new SqlParameter(param.ParameterName, param.SqlDbType)
                        {
                            Value = param.Value, // 設定參數值
                            Direction = param.Direction // 設定參數方向
                        };

                        // 加入參數至命令物件
                        adapter.SelectCommand.Parameters.Add(newParam);
                    }
                }
                // 執行查詢並將結果填入 DataTable
                adapter.Fill(dt);
                // 清除參數，避免記憶體洩漏
                adapter.SelectCommand.Parameters.Clear();
                // 回傳查詢結果
                return dt;
            }
        }

        /// <summary>
        /// 執行 SQL 查詢並回傳 DataSet 物件
        /// </summary>
        /// <param name="sql">要執行的 SQL 語句或儲存過程名稱</param>
        /// <param name="cmdType">命令型態 (Text 或 StoredProcedure)</param>
        /// <param name="pms">SQL 參數陣列</param>
        /// <returns>查詢結果的 DataSet 物件</returns>
        public static DataSet ExecuteDataSet(string sql, CommandType cmdType, params SqlParameter[] pms)
        {
            // 建立 DataSet 物件，用來儲存查詢結果
            DataSet ds = new DataSet();
            // 使用 SqlDataAdapter，自動建立 SQL 連線，不需手動建立 Connection
            using (SqlDataAdapter adapter = new SqlDataAdapter(sql, connStr))
            {
                // 設定命令型態 (SQL 或儲存過程)
                adapter.SelectCommand.CommandType = cmdType;
                if (pms != null)
                {
                    // 逐一處理 SQL 參數，避免參數重複使用造成錯誤
                    foreach (var param in pms)
                    {
                        // 複製參數物件，並設定參數屬性
                        var newParam = new SqlParameter(param.ParameterName, param.SqlDbType)
                        {
                            Value = param.Value, // 設定參數值
                            Direction = param.Direction // 設定參數方向
                        };

                        // 加入參數至命令物件
                        adapter.SelectCommand.Parameters.Add(newParam);
                    }
                }
                // 執行查詢並將結果填入 DataSet
                adapter.Fill(ds);
                // 清除參數，避免記憶體洩漏
                adapter.SelectCommand.Parameters.Clear();
                // 回傳查詢結果
                return ds;
            }
        }

        /// <summary>
        /// 執行 SQL 查詢並回傳 DataSet 物件 (參數型態為 List)
        /// </summary>
        /// <param name="sql">要執行的 SQL 語句或儲存過程名稱</param>
        /// <param name="cmdType">命令型態 (Text 或 StoredProcedure)</param>
        /// <param name="pms">SQL 參數 List</param>
        /// <returns>查詢結果的 DataSet 物件</returns>
        public static DataSet ExecuteDataSetPmsList(string sql, CommandType cmdType, List<SqlParameter> pms)
        {
            // 建立 DataSet 物件
            DataSet ds = new DataSet();
            // 使用 SqlDataAdapter，自動建立 SQL 連線
            using (SqlDataAdapter adapter = new SqlDataAdapter(sql, connStr))
            {
                // 設定命令型態 (SQL 或儲存過程)
                adapter.SelectCommand.CommandType = cmdType;
                if (pms != null)
                {
                    // 加入參數至命令物件
                    adapter.SelectCommand.Parameters.AddRange(pms.ToArray<SqlParameter>());
                }
                // 執行查詢並將結果填入 DataSet
                adapter.Fill(ds);
                // 回傳查詢結果
                return ds;
            }
        }

        /// <summary>
        /// 執行 SQL 查詢並回傳 SqlDataReader 物件 (參數型態為 List)
        /// </summary>
        /// <param name="sql">要執行的 SQL 語句或儲存過程名稱</param>
        /// <param name="cmdType">命令型態 (Text 或 StoredProcedure)</param>
        /// <param name="pms">SQL 參數 List</param>
        /// <returns>SqlDataReader 物件，包含查詢結果</returns>
        public static SqlDataReader ExecuteReaderPmsList(string sql, CommandType cmdType, List<SqlParameter> pms)
        {
            // 建立 SQL 連線物件
            SqlConnection con = new SqlConnection(connStr);
            // 使用 SQL 命令物件
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                // 設定命令型態 (SQL 或儲存過程)
                cmd.CommandType = cmdType;
                if (pms != null)
                {
                    // 加入參數至命令物件
                    cmd.Parameters.AddRange(pms.ToArray<SqlParameter>());
                }
                try
                {
                    // 開啟資料庫連線
                    con.Open();
                    // 執行查詢並回傳 SqlDataReader，查詢結束自動關閉連線
                    return cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch
                {
                    // 發生例外時關閉並釋放連線資源
                    con.Close();
                    con.Dispose();
                    throw;
                }
            }
        }
        #endregion

    }
}