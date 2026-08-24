using MySqlConnector;

namespace CarWash.Api.Services;

public interface ICustomerRegistrationMirror
{
    Task EnsureSchemaAsync(CancellationToken cancellationToken);
    Task AddAsync(string name, string phone, string address, string email, CancellationToken cancellationToken);
}

public class MySqlCustomerRegistration : ICustomerRegistrationMirror
{
    private readonly IConfiguration _configuration;

    public MySqlCustomerRegistration(IConfiguration configuration) => _configuration = configuration;

    public async Task EnsureSchemaAsync(CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS register (
                id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                name VARCHAR(255) NOT NULL,
                phone_no VARCHAR(30) NOT NULL,
                address VARCHAR(500) NOT NULL,
                gmail VARCHAR(255) NOT NULL
            );
            ALTER TABLE register
                MODIFY COLUMN id INT NOT NULL AUTO_INCREMENT,
                MODIFY COLUMN name VARCHAR(255) NOT NULL,
                MODIFY COLUMN phone_no VARCHAR(30) NOT NULL,
                MODIFY COLUMN address VARCHAR(500) NOT NULL,
                MODIFY COLUMN gmail VARCHAR(255) NOT NULL;
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task AddAsync(
        string name,
        string phone,
        string address,
        string email,
        CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO register (name, phone_no, address, gmail)
            VALUES (@name, @phone, @address, @email);
            """;
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@phone", phone);
        command.Parameters.AddWithValue("@address", address);
        command.Parameters.AddWithValue("@email", email);
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