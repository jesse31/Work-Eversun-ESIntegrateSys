using System;
using System.Threading.Tasks;
using System.Transactions;

namespace ESIntegrateSys.Utilities
{
    /// <summary>
    /// 事務輔助類
    /// </summary>
    public static class TransactionHelper
    {
        /// <summary>
        /// 在事務範圍內執行操作
        /// </summary>
        /// <typeparam name="T">返回值類型</typeparam>
        /// <param name="action">要執行的操作</param>
        /// <param name="isolationLevel">事務隔離級別</param>
        /// <param name="timeoutSeconds">超時時間（秒）</param>
        /// <returns>操作結果</returns>
        public static T ExecuteInTransaction<T>(Func<T> action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int timeoutSeconds = 60)
        {
            using (TransactionScope scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = isolationLevel,
                    Timeout = TimeSpan.FromSeconds(timeoutSeconds)
                }))
            {
                T result = action();
                scope.Complete();
                return result;
            }
        }

        /// <summary>
        /// 在事務範圍內執行操作（無返回值）
        /// </summary>
        /// <param name="action">要執行的操作</param>
        /// <param name="isolationLevel">事務隔離級別</param>
        /// <param name="timeoutSeconds">超時時間（秒）</param>
        public static void ExecuteInTransaction(Action action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int timeoutSeconds = 60)
        {
            using (TransactionScope scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = isolationLevel,
                    Timeout = TimeSpan.FromSeconds(timeoutSeconds)
                }))
            {
                action();
                scope.Complete();
            }
        }

        /// <summary>
        /// 在事務範圍內非同步執行操作
        /// </summary>
        /// <typeparam name="T">返回值類型</typeparam>
        /// <param name="asyncAction">要執行的非同步操作</param>
        /// <param name="isolationLevel">事務隔離級別</param>
        /// <param name="timeoutSeconds">超時時間（秒）</param>
        /// <returns>操作結果</returns>
        public static async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> asyncAction, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int timeoutSeconds = 60)
        {
            // 在較舊版本的 .NET 中，我們不使用 TransactionScopeAsyncFlowOption
            // 而是手動管理事務
            T result;
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = isolationLevel,
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };

            using (var transaction = new CommittableTransaction(transactionOptions))
            {
                Transaction.Current = transaction;
                try
                {
                    result = await asyncAction().ConfigureAwait(false);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    Transaction.Current = null;
                }
            }

            return result;
        }

        /// <summary>
        /// 在事務範圍內非同步執行操作（無返回值）
        /// </summary>
        /// <param name="asyncAction">要執行的非同步操作</param>
        /// <param name="isolationLevel">事務離離級別</param>
        /// <param name="timeoutSeconds">超時時間（秒）</param>
        public static async Task ExecuteInTransactionAsync(Func<Task> asyncAction, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, int timeoutSeconds = 60)
        {
            // 在較舊版本的 .NET 中，我們不使用 TransactionScopeAsyncFlowOption
            // 而是手動管理事務
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = isolationLevel,
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };

            using (var transaction = new CommittableTransaction(transactionOptions))
            {
                Transaction.Current = transaction;
                try
                {
                    await asyncAction().ConfigureAwait(false);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    Transaction.Current = null;
                }
            }
        }
    }
}
