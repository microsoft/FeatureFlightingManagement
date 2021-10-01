using System;
using System.Linq;
using StackExchange.Redis;
using System.Threading.Tasks;
using AppInsights.EnterpriseTelemetry;
using System.Collections.Generic;
using AppInsights.EnterpriseTelemetry.Context;
using Microsoft.PS.FlightingService.Common.Caching;

namespace Microsoft.PS.FlightingService.Caching
{
    public class RedisCache : ICache
    {
        protected readonly IConnectionMultiplexer _connectionMultiplexer;
        protected readonly IDatabase _database;
        protected readonly ILogger _logger;
        protected readonly string _host;

        public RedisCache(IConnectionMultiplexer connectionMultiplexer, ILogger logger, string host)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _database = _connectionMultiplexer.GetDatabase();
            _logger = logger;
            _host = host;
        }

        public RedisCache(string connectionString, int timeout, ILogger logger)
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.Ssl = true;
            options.SyncTimeout = timeout;
            options.AsyncTimeout = timeout;
            _host = options.SslHost;
            _connectionMultiplexer = ConnectionMultiplexer.Connect(options);
            _database = _connectionMultiplexer.GetDatabase();
            _logger = logger;
        }

#pragma warning disable CA1031 // Do not catch general exception types
        public async Task<List<string>> GetList(string key, string correlationId, string transactionId)
        {
            var dependencyContext = new DependencyContext(CacheLogContext.GetMetadata(_host, "LRANGE", key));
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentNullException(nameof(key));
                var redisItems = await _database.ListRangeAsync(key, 0, -1);
                var items = redisItems.Select(item => item.ToString()).ToList();
                dependencyContext.CompleteDependency("200", string.Join(',', items));
                _logger.Log(dependencyContext);
                return items.Select(item => item.ToString()).ToList();
            }
            catch (Exception exception)
            {
                dependencyContext.FailDependency(exception, "");
                _logger.Log(dependencyContext);
                return null;
            }
        }
#pragma warning restore CA1031 // Do not catch general exception types

        public async Task Delete(string key, string correlationId, string transactionId)
        {
            var dependencyContext = new DependencyContext(CacheLogContext.GetMetadata(_host, "DEL", key));
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentNullException(nameof(key));
                await _database.KeyDeleteAsync(key);
                dependencyContext.CompleteDependency("200", bool.TrueString);
                _logger.Log(dependencyContext);
            }
            catch (Exception exception)
            {
                dependencyContext.FailDependency(exception, "");
                _logger.Log(dependencyContext);
                throw;
            }
        }

        public async Task SetList(string key, List<string> values, string correlationId, string transactionId, int relativeExpirationMins = -1)
        {
            //TODO - Use expiration value
            var dependencyContext = new DependencyContext(CacheLogContext.GetMetadata(_host, "DEL", key));
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentNullException(nameof(key));
                await Delete(key, correlationId, transactionId);
                var itemCount = await _database.ListRightPushAsync(key, values.Select(value => (RedisValue)value).ToArray());
                dependencyContext.CompleteDependency("200", itemCount.ToString());
                _logger.Log(dependencyContext);
            }
            catch (Exception exception)
            {
                dependencyContext.FailDependency(exception, "");
                _logger.Log(dependencyContext);
                throw;
            }
        }
    }
}
