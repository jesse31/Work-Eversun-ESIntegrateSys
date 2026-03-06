using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

namespace ESIntegrateSys.Models
{

    /// <summary>
    /// 提供 SQL Server 資料庫存取相關靜態方法。
    /// </summary>
    public class Db
    {
        #region paramater method

        /// <summary>
        /// 資料庫連線字串，來源於 Web.config 設定。
        /// </summary>
        private static readonly String connStr = System.Configuration.ConfigurationManager.ConnectionStrings["ESIntegrateSys"].ConnectionString;


        /// <summary>
        /// 執行查詢 SQL 指令，回傳 DataTable 物件 (參數以 List 傳入)。
        /// </summary>
        /// <param name="sql">SQL 指令或儲存過程名稱。</param>
        /// <param name="cmdType">指令型態 (Text 或 StoredProcedure)。</param>
        /// <param name="pms">SQL 參數 List。</param>
        /// <returns>查詢結果的 DataTable。</returns>
        public static DataTable ExecuteDataTablePmsList(string sql, CommandType cmdType, List<SqlParameter> pms)
        {
            // 效能優化：移除不必要的 try-catch 與 Parameters.Clear，並加強參數驗證
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("SQL 指令不可為空。", nameof(sql));
            }
            var dt = new DataTable();
            using (var adapter = new SqlDataAdapter(sql, connStr))
            {
                adapter.SelectCommand.CommandType = cmdType;
                if (pms != null && pms.Count > 0)
                {
                    // SECURITY: 僅允許有效參數，避免 SQL Injection
                    adapter.SelectCommand.Parameters.AddRange(pms.ToArray());
                }
                adapter.Fill(dt);
                // 效能註解：SqlDataAdapter 於 using 區塊自動釋放資源，不需手動清空 Parameters
                return dt;
            }
        }

        /// <summary>
        /// 執行查詢 SQL 指令，回傳 DataSet 物件。
        /// </summary>
        /// <param name="sql">SQL 指令或儲存過程名稱。</param>
        /// <param name="cmdType">指令型態 (Text 或 StoredProcedure)。</param>
        /// <param name="pms">SQL 參數陣列。</param>
        /// <returns>查詢結果的 DataSet。</returns>
        public static DataSet ExecuteDataSet(string sql, CommandType cmdType, params SqlParameter[] pms)
        {
            // 效能優化：移除不必要的 Parameters.Clear，並加強 SQL 指令驗證
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("SQL 指令不可為空。", nameof(sql));
            }
            var ds = new DataSet();
            using (var adapter = new SqlDataAdapter(sql, connStr))
            {
                adapter.SelectCommand.CommandType = cmdType;
                if (pms != null && pms.Length > 0)
                {
                    // SECURITY: 僅允許有效參數，避免 SQL Injection
                    adapter.SelectCommand.Parameters.AddRange(pms);
                }
                adapter.Fill(ds);
                // 效能註解：SqlDataAdapter 於 using 區塊自動釋放資源，不需手動清空 Parameters
                return ds;
            }
        }

        /// <summary>
        /// 執行查詢 SQL 指令，回傳 DataSet 物件 (參數以 List 傳入)。
        /// </summary>
        /// <param name="sql">SQL 指令或儲存過程名稱。</param>
        /// <param name="cmdType">指令型態 (Text 或 StoredProcedure)。</param>
        /// <param name="pms">SQL 參數 List。</param>
        /// <returns>查詢結果的 DataSet。</returns>
        public static DataSet ExecuteDataSetPmsList(string sql, CommandType cmdType, List<SqlParameter> pms)
        {
            // 效能優化：加入 SQL 指令驗證，僅在有參數時 AddRange
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("SQL 指令不可為空。", nameof(sql));
            }
            var ds = new DataSet();
            using (var adapter = new SqlDataAdapter(sql, connStr))
            {
                adapter.SelectCommand.CommandType = cmdType;
                if (pms != null && pms.Count > 0)
                {
                    // SECURITY: 僅允許有效參數，避免 SQL Injection
                    adapter.SelectCommand.Parameters.AddRange(pms.ToArray());
                }
                adapter.Fill(ds);
                // 效能註解：SqlDataAdapter 於 using 區塊自動釋放資源，不需手動清空 Parameters
                return ds;
            }
        }

        /// <summary>
        /// 執行查詢 SQL 指令，回傳 SqlDataReader 物件 (參數以 List 傳入，需由呼叫端關閉)。
        /// </summary>
        /// <param name="sql">SQL 指令或儲存過程名稱。</param>
        /// <param name="cmdType">指令型態 (Text 或 StoredProcedure)。</param>
        /// <param name="pms">SQL 參數 List。</param>
        /// <returns>SqlDataReader 物件。</returns>
        public static SqlDataReader ExecuteReaderPmsList(string sql, CommandType cmdType, List<SqlParameter> pms)
        {
            // 效能優化：加入 SQL 指令驗證，僅在有參數時 AddRange，簡化 try-catch
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("SQL 指令不可為空。", nameof(sql));
            }
            var con = new SqlConnection(connStr);
            var cmd = new SqlCommand(sql, con)
            {
                CommandType = cmdType
            };
            if (pms != null && pms.Count > 0)
            {
                // SECURITY: 僅允許有效參數，避免 SQL Injection
                cmd.Parameters.AddRange(pms.ToArray());
            }
            try
            {
                con.Open();
                // 效能註解：CommandBehavior.CloseConnection 於 DataReader 關閉時自動釋放連線
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch
            {
                con.Close();
                con.Dispose();
                throw;
            }
        }

        /// <summary>
        /// 執行 INSERT/UPDATE/DELETE 等非查詢 SQL 指令 (支援外部交易)，回傳受影響的資料列數。
        /// </summary>
        /// <param name="sql">SQL 指令或儲存過程名稱。</param>
        /// <param name="cmdType">指令型態 (Text 或 StoredProcedure)。</param>
        /// <param name="transaction">SqlTransaction 物件，可為 null。</param>
        /// <param name="pms">SQL 參數陣列。</param>
        /// <returns>受影響的資料列數。</returns>
        public static int ExecuteNonQuery(string sql, CommandType cmdType, SqlTransaction transaction, params SqlParameter[] pms)
        {
            // 效能優化：加入 SQL 指令驗證，僅在有參數時 AddRange
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("SQL 指令不可為空。", nameof(sql));
            }
            var con = transaction?.Connection ?? new SqlConnection(connStr);
            bool needDispose = transaction == null;
            try
            {
                using (var cmd = new SqlCommand(sql, con, transaction))
                {
                    cmd.CommandType = cmdType;
                    if (pms != null && pms.Length > 0)
                    {
                        // SECURITY: 僅允許有效參數，避免 SQL Injection
                        cmd.Parameters.AddRange(pms);
                    }
                    if (needDispose)
                    {
                        con.Open();
                    }
                    // 效能註解：外部交易時不釋放連線，否則 using 結束自動釋放
                    return cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (needDispose)
                {
                    con.Close();
                    con.Dispose();
                }
            }
        }

        /// <summary>
        /// 執行查詢 SQL 指令 (支援外部交易)，回傳 SqlDataReader 物件 (需由呼叫端關閉)。
        /// </summary>
        /// <param name="sql">SQL 指令或儲存過程名稱。</param>
        /// <param name="cmdType">指令型態 (Text 或 StoredProcedure)。</param>
        /// <param name="transaction">SqlTransaction 物件，可為 null。</param>
        /// <param name="pms">SQL 參數陣列。</param>
        /// <returns>SqlDataReader 物件。</returns>
        public static SqlDataReader ExecuteReader(string sql, CommandType cmdType, SqlTransaction transaction, params SqlParameter[] pms)
        {
            // 效能優化：加入 SQL 指令驗證，僅在有參數時 AddRange
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("SQL 指令不可為空。", nameof(sql));
            }
            var con = transaction?.Connection ?? new SqlConnection(connStr);
            bool needDispose = transaction == null;
            var cmd = new SqlCommand(sql, con, transaction)
            {
                CommandType = cmdType
            };
            if (pms != null && pms.Length > 0)
            {
                // SECURITY: 僅允許有效參數，避免 SQL Injection
                cmd.Parameters.AddRange(pms);
            }
            try
            {
                if (needDispose)
                {
                    con.Open();
                }
                // 效能註解：外部交易時不釋放連線，否則 DataReader 關閉時自動釋放
                return cmd.ExecuteReader(needDispose ? CommandBehavior.CloseConnection : CommandBehavior.Default);
            }
            catch
            {
                if (needDispose)
                {
                    con.Close();
                    con.Dispose();
                }
                throw;
            }
        }

        /// <summary>
        /// 執行查詢 SQL 指令 (支援外部交易)，回傳 DataTable 物件。
        /// </summary>
        /// <param name="sql">SQL 指令或儲存過程名稱。</param>
        /// <param name="cmdType">指令型態 (Text 或 StoredProcedure)。</param>
        /// <param name="transaction">SqlTransaction 物件，可為 null。</param>
        /// <param name="pms">SQL 參數陣列。</param>
        /// <returns>查詢結果的 DataTable。</returns>
        public static DataTable ExecuteDataTable(string sql, CommandType cmdType, SqlTransaction transaction, params SqlParameter[] pms)
        {
            // 效能優化：加入 SQL 指令驗證，僅在有參數時 AddRange
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("SQL 指令不可為空。", nameof(sql));
            }
            var dt = new DataTable();
            var con = transaction?.Connection ?? new SqlConnection(connStr);
            bool needDispose = transaction == null;
            try
            {
                using (var cmd = new SqlCommand(sql, con, transaction))
                {
                    cmd.CommandType = cmdType;
                    if (pms != null && pms.Length > 0)
                    {
                        // SECURITY: 僅允許有效參數，避免 SQL Injection
                        cmd.Parameters.AddRange(pms);
                    }
                    if (needDispose)
                    {
                        con.Open();
                    }
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
                // 效能註解：外部交易時不釋放連線，否則 using 結束自動釋放
                return dt;
            }
            finally
            {
                if (needDispose)
                {
                    con.Close();
                    con.Dispose();
                }
            }
        }

        #endregion

        #region
        /// <summary>
        /// 執行 INSERT/UPDATE/DELETE 等非查詢 SQL 指令，回傳受影響的資料列數。
        /// </summary>
        /// <param name="sql">SQL 指令或儲存過程名稱。</param>
        /// <param name="cmdType">指令型態 (Text 或 StoredProcedure)。</param>
        /// <param name="pms">SQL 參數陣列。</param>
        /// <returns>受影響的資料列數。</returns>
        //public static int ExecueNonQuery(string sql, CommandType cmdType, params SqlParameter[] pms)
        //{
        //    using (SqlConnection con = new SqlConnection(connStr))
        //    {
        //        using (SqlCommand cmd = new SqlCommand(sql, con))
        //        {
        //            //設置目前執行的是「存儲過程? 還是帶參數的sql 語句?」
        //            cmd.CommandType = cmdType;
        //            if (pms != null)
        //            {
        //                cmd.Parameters.AddRange(pms);
        //            }

        //            con.Open();
        //            return cmd.ExecuteNonQuery();
        //        }
        //    }
        //}

        /// <summary>
        /// 執行查詢 SQL 指令，回傳 SqlDataReader 物件 (需由呼叫端關閉)。
        /// </summary>
        /// <param name="sql">SQL 指令或儲存過程名稱。</param>
        /// <param name="cmdType">指令型態 (Text 或 StoredProcedure)。</param>
        /// <param name="pms">SQL 參數陣列。</param>
        /// <returns>SqlDataReader 物件。</returns>
        //public static SqlDataReader ExecuteReader(string sql, CommandType cmdType, params SqlParameter[] pms)
        //{
        //    SqlConnection con = new SqlConnection(connStr);
        //    using (SqlCommand cmd = new SqlCommand(sql, con))
        //    {
        //        cmd.CommandType = cmdType;
        //        if (pms != null)
        //        {
        //            cmd.Parameters.AddRange(pms);
        //        }
        //        try
        //        {
        //            con.Open();
        //            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        //        }
        //        catch
        //        {
        //            con.Close();
        //            con.Dispose();
        //            throw;
        //        }
        //    }
        //}

        /// <summary>
        /// 執行查詢 SQL 指令，回傳 DataTable 物件。
        /// </summary>
        /// <param name="sql">SQL 指令或儲存過程名稱。</param>
        /// <param name="cmdType">指令型態 (Text 或 StoredProcedure)。</param>
        /// <param name="pms">SQL 參數陣列。</param>
        /// <returns>查詢結果的 DataTable。</returns>
        //public static DataTable ExecuteDataTable(string sql, CommandType cmdType, params SqlParameter[] pms)
        //{
        //    DataTable dt = new DataTable();
        //    //use SqlDataAdapter ,it will establish Sql connection.So ,it no need to create Connection by yourself.
        //    using (SqlDataAdapter adapter = new SqlDataAdapter(sql, connStr))
        //    {
        //        adapter.SelectCommand.CommandType = cmdType;
        //        if (pms != null)
        //        {
        //            adapter.SelectCommand.Parameters.AddRange(pms);

        //        }
        //        adapter.Fill(dt);
        //        return dt;
        //    }
        //}
        #endregion

    }
}