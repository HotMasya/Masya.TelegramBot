using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Masya.TelegramBot.Api.Dtos;
using Masya.TelegramBot.Api.Services.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Masya.TelegramBot.Api.Services
{
    public sealed class DatabaseLogsService : IDatabaseLogsService
    {
        public IConfiguration Configuration { get; }

        public DatabaseLogsService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(Configuration.GetConnectionString("RemoteDb"));
        }

        private async Task<IEnumerable<LogDto>> MapLogsToDtoAsync(string query, int? agencyId = null)
        {
            using SqlConnection conn = GetConnection();
            var command = new SqlCommand(query, conn);
            if (agencyId.HasValue)
            {
                command.Parameters.AddWithValue("@agencyId", agencyId);
            }
            await conn.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            var result = new List<LogDto>();
            while (reader.Read())
            {
                var dto = new LogDto
                {
                    Id = int.Parse(reader["Id"].ToString()),
                    Message = reader["Message"].ToString(),
                    Level = reader["Level"].ToString(),
                    TimeStamp = DateTime.Parse(reader["TimeStamp"].ToString())
                };
                result.Add(dto);
            }
            return result;
        }

        public async Task<IEnumerable<LogDto>> GetBotLogsAsync(int? agencyId = null)
        {
            string query = string.Format(
                "SELECT * FROM Serilogs WHERE AgencyId {0}",
                agencyId.HasValue ? "= @agencyId" : "IS NULL"
            );
            return await MapLogsToDtoAsync(query, agencyId);
        }

        public async Task<IEnumerable<LogDto>> GetBotLogsForLastHourAsync(int? agencyId = null)
        {
            string query = string.Format(
                "SELECT * FROM Serilogs WHERE Timestamp >= DATEADD(hour, -1, GETDATE()) AgencyId {0}",
                agencyId.HasValue ? "= @agencyId" : "IS NULL"
            );
            return await MapLogsToDtoAsync(query, agencyId);
        }
    }
}