
using System.Data.SqlClient;


namespace IbeApi.Services
{


    public class CodigoCleanupService : IHostedService, IDisposable
    {
        private readonly string _connectionString;
        private Timer _timer;

        public CodigoCleanupService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(RemoverCodigosAntigos, null, TimeSpan.Zero, TimeSpan.FromMinutes(1)); // Executa a cada 1 minuto
            return Task.CompletedTask;
        }

        private void RemoverCodigosAntigos(object state)
        {
            var agora = DateTime.UtcNow;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("UPDATE MOBILE_AUTH SET EXPIROU = 1 WHERE DATAGERACAO < @Limite", connection);
                command.Parameters.AddWithValue("@Limite", agora.AddMinutes(-2));  // Limite de 2 minutos atrás
                command.ExecuteNonQuery();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
