using CarWash.Api.Models;
using MySqlConnector;

namespace CarWash.Api.Services;

public interface IServiceMirror
{
    Task EnsureSchemaAsync(CancellationToken cancellationToken);
    Task UpsertAsync(Service service, CancellationToken cancellationToken);
    Task RemoveAsync(int serviceId, CancellationToken cancellationToken);
}

public class MySqlServiceMirror : IServiceMirror
{
    private readonly IConfiguration _configuration;

    public MySqlServiceMirror(IConfiguration configuration) => _configuration = configuration;

    public async Task EnsureSchemaAsync(CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var createCommand = connection.CreateCommand();
        createCommand.CommandText = """
            CREATE TABLE IF NOT EXISTS service_card (
                service_id INT NULL,
                service_name VARCHAR(255) NOT NULL,
                price VARCHAR(100) NOT NULL,
                service_detail VARCHAR(1000) NOT NULL,
                phone_number VARCHAR(30) NULL
            );
            """;
        await createCommand.ExecuteNonQueryAsync(cancellationToken);

        await using var columnCommand = connection.CreateCommand();
        columnCommand.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'service_card'
              AND COLUMN_NAME = 'service_id';
            """;
        var hasServiceId = Convert.ToInt32(await columnCommand.ExecuteScalarAsync(cancellationToken)) > 0;

        if (!hasServiceId)
        {
            await using var addColumnCommand = connection.CreateCommand();
            addColumnCommand.CommandText = "ALTER TABLE service_card ADD COLUMN service_id INT NULL FIRST;";
            await addColumnCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var phoneColumnCommand = connection.CreateCommand();
        phoneColumnCommand.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'service_card'
              AND COLUMN_NAME = 'phone_number';
            """;
        var hasPhoneNumber = Convert.ToInt32(await phoneColumnCommand.ExecuteScalarAsync(cancellationToken)) > 0;

        if (!hasPhoneNumber)
        {
            await using var addPhoneColumnCommand = connection.CreateCommand();
            addPhoneColumnCommand.CommandText = "ALTER TABLE service_card ADD COLUMN phone_number VARCHAR(30) NULL;";
            await addPhoneColumnCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var alterColumnsCommand = connection.CreateCommand();
        alterColumnsCommand.CommandText = """
            ALTER TABLE service_card
                MODIFY COLUMN service_name VARCHAR(255) NOT NULL,
                MODIFY COLUMN price VARCHAR(100) NOT NULL,
                MODIFY COLUMN service_detail VARCHAR(1000) NOT NULL;
            """;
        await alterColumnsCommand.ExecuteNonQueryAsync(cancellationToken);

        await using var indexCommand = connection.CreateCommand();
        indexCommand.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.STATISTICS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'service_card'
              AND INDEX_NAME = 'UX_service_card_service_id';
            """;
        var hasIndex = Convert.ToInt32(await indexCommand.ExecuteScalarAsync(cancellationToken)) > 0;

        if (!hasIndex)
        {
            await using var addIndexCommand = connection.CreateCommand();
            addIndexCommand.CommandText = "CREATE UNIQUE INDEX UX_service_card_service_id ON service_card (service_id);";
            await addIndexCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task UpsertAsync(Service service, CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO service_card (service_id, service_name, price, service_detail, phone_number)
            VALUES (@id, @name, @price, @description, @phoneNumber)
            ON DUPLICATE KEY UPDATE
                service_name = VALUES(service_name),
                price = VALUES(price),
                service_detail = VALUES(service_detail),
                phone_number = VALUES(phone_number);
            """;
        command.Parameters.AddWithValue("@id", service.Id);
        command.Parameters.AddWithValue("@name", service.Name);
        command.Parameters.AddWithValue("@price", service.PriceLabel);
        command.Parameters.AddWithValue("@description", service.Description);
        command.Parameters.AddWithValue("@phoneNumber", (object?)service.PhoneNumber ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RemoveAsync(int serviceId, CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM service_card WHERE service_id = @id;";
        command.Parameters.AddWithValue("@id", serviceId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private MySqlConnection CreateConnection()
    {
        var section = _configuration.GetSection("SignupMySql");
        return new MySqlConnection(new MySqlConnectionStringBuilder
        {
            Server = section["Server"],
            Port = section.GetValue<uint>("Port"),
            Database = section["Database"],
            UserID = section["User"],
            Password = section["Password"],
            SslMode = MySqlSslMode.Preferred
        }.ConnectionString);
    }
}