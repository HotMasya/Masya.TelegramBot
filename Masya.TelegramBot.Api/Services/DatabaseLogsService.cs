using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Masya.TelegramBot.Api.Dtos;
using Masya.TelegramBot.Api.Services.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Masya.TelegramBot.Api.Services
{
    public sealed class DatabaseLogsService : IDatabaseLogsService
    {
        public IConfiguration Configuration { get; }

        private readonly ILogger<IDatabaseLogsService> _logger;

        public DatabaseLogsService(IConfiguration configuration, ILogger<IDatabaseLogsService> logger)
        {
            Configuration = configuration;
            _logger = logger;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(Configuration.GetConnectionString("RemoteDb"));
        }

        private IEnumerable<LogDto> MapLogsAsync(SqlDataReader reader)
        {
            var result = new List<LogDto>();
            int count = 0;
            try
            {
                while (reader.Read())
                {
                    count++;
                    var dto = new LogDto
                    {
                        Message = reader["Message"].ToString(),
                        Level = reader["Level"].ToString(),
                        TimeStamp = DateTime.Parse(reader["TimeStamp"].ToString())
                    };
                    result.Add(dto);
                }
            }
            catch
            {
                _logger.LogError("Something went wrong");
            }

            _logger.LogInformation("Total rows found: {0}", count);
            return result;
        }

        public async Task<IEnumerable<LogDto>> GetBotLogsAsync(int? agencyId = null)
        {
            using SqlConnection conn = GetConnection();
            string query = string.Format(
                "SELECT * FROM Serilogs WHERE AgencyId {0}",
                agencyId.HasValue ? "= @agencyId" : "IS NULL"
            );
            var command = new SqlCommand(query, conn);
            if (agencyId.HasValue)
            {
                command.Parameters.AddWithValue("@agencyId", agencyId);
            }
            await conn.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            return MapLogsAsync(reader);
        }
    }
}