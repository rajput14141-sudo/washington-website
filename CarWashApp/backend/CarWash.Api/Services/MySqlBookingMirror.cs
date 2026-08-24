using MySqlConnector;

namespace CarWash.Api.Services;

public record MySqlBookingRecord(
    int Id,
    string CustomerName,
    string PhoneNumber,
    string VehicleName,
    string ServiceName,
    string ServicePrice,
    string Address,
    string City,
    string Pincode,
    DateTime ScheduledAt,
    DateTime ExpireDate,
    string Status);

public interface IBookingMirror
{
    Task EnsureSchemaAsync(CancellationToken cancellationToken);
    Task AddAsync(
        int id,
        string customerName,
        string vehicleName,
        string serviceName,
        string servicePrice,
        string address,
        string city,
        string pincode,
        DateTime scheduledAt,
        string phoneNumber,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<MySqlBookingRecord>> GetAllAsync(CancellationToken cancellationToken);
    Task UpdateStatusAsync(int id, string status, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}

public class MySqlBookingMirror : IBookingMirror
{
    private readonly IConfiguration _configuration;

    public MySqlBookingMirror(IConfiguration configuration) => _configuration = configuration;

    public async Task EnsureSchemaAsync(CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var createCommand = connection.CreateCommand();
        createCommand.CommandText = """
            CREATE TABLE IF NOT EXISTS customerser_booked (
                id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                VehicleName VARCHAR(100) NOT NULL,
                Address VARCHAR(300) NOT NULL,
                City VARCHAR(100) NOT NULL,
                Pincode VARCHAR(20) NOT NULL,
                `Date` DATETIME NOT NULL,
                Phone_no VARCHAR(30) NOT NULL,
                CustomerName VARCHAR(255) NOT NULL DEFAULT '',
                ServiceName VARCHAR(255) NOT NULL DEFAULT '',
                ServicePrice VARCHAR(100) NOT NULL DEFAULT '',
                ExpireDate DATETIME NOT NULL,
                Status VARCHAR(30) NOT NULL DEFAULT 'Pending'
            );
            """;
        await createCommand.ExecuteNonQueryAsync(cancellationToken);

        foreach (var column in new[]
        {
            (Name: "Phone_no", Definition: "VARCHAR(30) NULL"),
            (Name: "CustomerName", Definition: "VARCHAR(255) NOT NULL DEFAULT ''"),
            (Name: "ServiceName", Definition: "VARCHAR(255) NOT NULL DEFAULT ''"),
            (Name: "ServicePrice", Definition: "VARCHAR(100) NOT NULL DEFAULT ''"),
            (Name: "ExpireDate", Definition: "DATETIME NULL"),
            (Name: "Status", Definition: "VARCHAR(30) NOT NULL DEFAULT 'Pending'")
        })
        {
            await using var columnCommand = connection.CreateCommand();
            columnCommand.CommandText = """
                SELECT COUNT(*)
                FROM information_schema.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'customerser_booked'
                  AND COLUMN_NAME = @column;
                """;
            columnCommand.Parameters.AddWithValue("@column", column.Name);
            var exists = Convert.ToInt32(await columnCommand.ExecuteScalarAsync(cancellationToken)) > 0;

            if (!exists)
            {
                await using var addColumnCommand = connection.CreateCommand();
                addColumnCommand.CommandText =
                    $"ALTER TABLE customerser_booked ADD COLUMN `{column.Name}` {column.Definition};";
                await addColumnCommand.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        await using var legacyPhoneColumnCommand = connection.CreateCommand();
        legacyPhoneColumnCommand.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'customerser_booked'
              AND COLUMN_NAME = 'Phnone_no';
            """;
        var hasLegacyPhoneNumber = Convert.ToInt32(
            await legacyPhoneColumnCommand.ExecuteScalarAsync(cancellationToken)) > 0;

        if (hasLegacyPhoneNumber)
        {
            await using var migratePhoneCommand = connection.CreateCommand();
            migratePhoneCommand.CommandText = """
                UPDATE customerser_booked
                SET Phone_no = COALESCE(NULLIF(Phone_no, ''), Phnone_no, '');
                ALTER TABLE customerser_booked DROP COLUMN Phnone_no;
                """;
            await migratePhoneCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var populateExpiryCommand = connection.CreateCommand();
        populateExpiryCommand.CommandText = """
            UPDATE customerser_booked
            SET ExpireDate = DATE_ADD(`Date`, INTERVAL 30 DAY)
            WHERE ExpireDate IS NULL;
            """;
        await populateExpiryCommand.ExecuteNonQueryAsync(cancellationToken);

        await using var alterCommand = connection.CreateCommand();
        alterCommand.CommandText = """
            ALTER TABLE customerser_booked
                MODIFY COLUMN id INT NOT NULL AUTO_INCREMENT,
                MODIFY COLUMN VehicleName VARCHAR(100) NOT NULL,
                MODIFY COLUMN Address VARCHAR(300) NOT NULL,
                MODIFY COLUMN City VARCHAR(100) NOT NULL,
                MODIFY COLUMN Pincode VARCHAR(20) NOT NULL,
                MODIFY COLUMN `Date` DATETIME NOT NULL,
                MODIFY COLUMN Phone_no VARCHAR(30) NOT NULL,
                MODIFY COLUMN CustomerName VARCHAR(255) NOT NULL DEFAULT '',
                MODIFY COLUMN ServiceName VARCHAR(255) NOT NULL DEFAULT '',
                MODIFY COLUMN ServicePrice VARCHAR(100) NOT NULL DEFAULT '',
                MODIFY COLUMN ExpireDate DATETIME NOT NULL,
                MODIFY COLUMN Status VARCHAR(30) NOT NULL DEFAULT 'Pending';
            """;
        await alterCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task AddAsync(
        int id,
        string customerName,
        string vehicleName,
        string serviceName,
        string servicePrice,
        string address,
        string city,
        string pincode,
        DateTime scheduledAt,
        string phoneNumber,
        CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO customerser_booked
                (id, CustomerName, VehicleName, ServiceName, ServicePrice, Address, City,
                 Pincode, `Date`, Phone_no, ExpireDate, Status)
            VALUES
                (@id, @customerName, @vehicleName, @serviceName, @servicePrice, @address,
                 @city, @pincode, @scheduledAt, @phoneNumber, @expireDate, 'Pending')
            ON DUPLICATE KEY UPDATE
                CustomerName = VALUES(CustomerName),
                VehicleName = VALUES(VehicleName),
                ServiceName = VALUES(ServiceName),
                ServicePrice = VALUES(ServicePrice),
                Address = VALUES(Address),
                City = VALUES(City),
                Pincode = VALUES(Pincode),
                `Date` = VALUES(`Date`),
                Phone_no = VALUES(Phone_no),
                ExpireDate = VALUES(ExpireDate),
                Status = VALUES(Status);
            """;
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@customerName", customerName);
        command.Parameters.AddWithValue("@vehicleName", vehicleName);
        command.Parameters.AddWithValue("@serviceName", serviceName);
        command.Parameters.AddWithValue("@servicePrice", servicePrice);
        command.Parameters.AddWithValue("@address", address);
        command.Parameters.AddWithValue("@city", city);
        command.Parameters.AddWithValue("@pincode", pincode);
        command.Parameters.AddWithValue("@scheduledAt", scheduledAt);
        command.Parameters.AddWithValue("@phoneNumber", phoneNumber);
        command.Parameters.AddWithValue("@expireDate", scheduledAt.AddDays(30));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MySqlBookingRecord>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, CustomerName, Phone_no, VehicleName, ServiceName, ServicePrice,
                   Address, City, Pincode, `Date`, ExpireDate, Status
            FROM customerser_booked
            ORDER BY id DESC;
            """;

        var records = new List<MySqlBookingRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            records.Add(new MySqlBookingRecord(
                reader.GetInt32("id"),
                reader.GetString("CustomerName"),
                reader.GetString("Phone_no"),
                reader.GetString("VehicleName"),
                reader.GetString("ServiceName"),
                reader.GetString("ServicePrice"),
                reader.GetString("Address"),
                reader.GetString("City"),
                reader.GetString("Pincode"),
                reader.GetDateTime("Date"),
                reader.GetDateTime("ExpireDate"),
                reader.GetString("Status")));
        }

        return records;
    }

    public async Task UpdateStatusAsync(int id, string status, CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE customerser_booked SET Status = @status WHERE id = @id;";
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@status", status);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM customerser_booked WHERE id = @id;";
        command.Parameters.AddWithValue("@id", id);
        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
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