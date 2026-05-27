using MySqlConnector;

namespace CarDealershipAdoNet.Database;

public interface IDbConnectionFactory
{
    MySqlConnection CreateConnection();
}
