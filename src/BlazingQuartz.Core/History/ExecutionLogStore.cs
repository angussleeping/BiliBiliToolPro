using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ray.BiliBiliTool.Domain;
using Ray.BiliBiliTool.Infrastructure.EF;

namespace BlazingQuartz.Core.History
{
    public class ExecutionLogStore : IExecutionLogStore
    {
        private readonly ILogger<ExecutionLogStore> _logger;
        private readonly BiliDbContext _dbContext;

        public ExecutionLogStore(ILogger<ExecutionLogStore> logger, BiliDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task AddExecutionLog(ExecutionLog log, CancellationToken cancelToken = default)
        {
            await _dbContext.ExecutionLogs.AddAsync(log, cancelToken);
        }

        public bool Exists(ExecutionLog log)
        {
            return _dbContext.ExecutionLogs.Any(l => l.RunInstanceId == log.RunInstanceId);
        }

        /// <summary>
        /// 删除超过保留天数的执行记录。
        ///
        /// <para>
        /// 4.2.x 这里走的是原生 SQL（<c>DELETE ... WHERE date_added_utc &lt; @p</c>），列名照搬上游的
        /// PostgreSQL/MySQL 命名，而我们的表是 EF 生成的 <c>DateAddedUtc</c>，而且这一列被
        /// <c>DateTimeOffsetToBinaryConverter</c> 编码成了 long —— 结果每天凌晨的清理任务都报
        /// <c>no such column: date_added_utc</c>，执行记录一天都没被删过。
        /// </para>
        /// <para>
        /// 改用 EF 的 <c>ExecuteDeleteAsync</c>：列名、类型转换、参数化全部交给模型，不会再对不上。
        /// </para>
        /// </summary>
        public async Task<int> DeleteLogsByDays(
            int daysToKeep,
            CancellationToken cancelToken = default
        )
        {
            var cutoff = DateTimeOffset.UtcNow.Date.AddDays(-(daysToKeep + 1));
            return await _dbContext
                .ExecutionLogs.Where(log => log.DateAddedUtc < cutoff)
                .ExecuteDeleteAsync(cancelToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancelToken = default)
        {
            await _dbContext.SaveChangesAsync(cancelToken);
        }

        public ValueTask UpdateExecutionLog(ExecutionLog log)
        {
            var entry = _dbContext
                .ExecutionLogs.Where(l => l.RunInstanceId == log.RunInstanceId)
                .FirstOrDefault();

            if (entry != null)
            {
                entry.ExecutionLogDetail = log.ExecutionLogDetail;
                entry.ErrorMessage = log.ErrorMessage;
                entry.ExecutionLogDetail = log.ExecutionLogDetail;
                entry.IsVetoed = log.IsVetoed;
                entry.JobRunTime = log.JobRunTime;
                entry.Result = log.Result;
                entry.IsException = log.IsException;
                entry.IsSuccess = log.IsSuccess;
                entry.ReturnCode = log.ReturnCode;

                _dbContext.ExecutionLogs.Update(entry);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to UpdateExecutionLog. Cannot find run instance id [{runInstanceId}]",
                    log.RunInstanceId
                );
            }

            return ValueTask.CompletedTask;
        }

        public async Task MarkExecutingJobAsIncomplete(CancellationToken cancellToken = default)
        {
            var isSuccessNullJobs = _dbContext.ExecutionLogs.Where(l =>
                !l.IsSuccess.HasValue && l.LogType == LogType.ScheduleJob
            );

            foreach (var log in isSuccessNullJobs)
            {
                log.IsSuccess = false;
                log.ErrorMessage = "Incomplete execution.";
                log.JobRunTime = null;
            }

            await _dbContext.SaveChangesAsync(cancellToken);
        }
    }
}
