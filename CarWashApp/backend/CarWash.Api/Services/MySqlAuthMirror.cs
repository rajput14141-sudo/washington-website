using MySqlConnector;

namespace CarWash.Api.Services;

public record MySqlSignupRecord(string Email, string Name, string PasswordHash);
public record CustomerSignupDetails(int Id, string FullName, string Email, string PhoneNumber, string Address);

public interface IAuthMirror
{
    Task EnsureSchemaAsync(CancellationToken cancellationToken);
    Task AddSignupAsync(
        string email,
        string name,
        string phoneNumber,
        string address,
        string passwordHash,
        CancellationToken cancellationToken);
    Task AddLoginAsync(string email, string name, string passwordHash, CancellationToken cancellationToken);
    Task AddAdminSignupAsync(string email, string name, string passwordHash, CancellationToken cancellationToken);
    Task AddAdminLoginAsync(string email, string name, string passwordHash, CancellationToken cancellationToken);
    Task<MySqlSignupRecord?> FindSignupAsync(string email, CancellationToken cancellationToken);
    Task<MySqlSignupRecord?> FindAdminSignupAsync(string email, CancellationToken cancellationToken);
    Task<IReadOnlyList<CustomerSignupDetails>> GetCustomerSignupsAsync(CancellationToken cancellationToken);
}

public class MySqlAuthMirror : IAuthMirror
{
    private readonly IConfiguration _configuration;

    public MySqlAuthMirror(IConfiguration configuration) => _configuration = configuration;

    public async Task EnsureSchemaAsync(CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var customerSignupCommand = connection.CreateCommand();
        customerSignupCommand.CommandText = """
            CREATE TABLE IF NOT EXISTS customer_signup (
                id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                full_name VARCHAR(255) NOT NULL,
                gmail VARCHAR(255) NOT NULL,
                password VARCHAR(255) NOT NULL,
                confirm_password VARCHAR(255) NOT NULL
            );
            """;
        await customerSignupCommand.ExecuteNonQueryAsync(cancellationToken);

        await using var customerConfirmColumnCommand = connection.CreateCommand();
        customerConfirmColumnCommand.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'customer_signup'
              AND COLUMN_NAME = 'confirm_password';
            """;
        var hasCustomerConfirmPassword = Convert.ToInt32(
            await customerConfirmColumnCommand.ExecuteScalarAsync(cancellationToken)) > 0;

        if (!hasCustomerConfirmPassword)
        {
            await using var addCustomerConfirmCommand = connection.CreateCommand();
            addCustomerConfirmCommand.CommandText = """
                ALTER TABLE customer_signup
                    ADD COLUMN confirm_password VARCHAR(255) NULL;
                """;
            await addCustomerConfirmCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var column in new[] { (Name: "full_name", Type: "VARCHAR(255)"), (Name: "password", Type: "VARCHAR(255)") })
        {
            await using var canonicalColumnCommand = connection.CreateCommand();
            canonicalColumnCommand.CommandText = """
                SELECT COUNT(*)
                FROM information_schema.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'customer_signup'
                  AND COLUMN_NAME = @column;
                """;
            canonicalColumnCommand.Parameters.AddWithValue("@column", column.Name);
            var hasCanonicalColumn = Convert.ToInt32(
                await canonicalColumnCommand.ExecuteScalarAsync(cancellationToken)) > 0;

            if (!hasCanonicalColumn)
            {
                await using var addCanonicalColumnCommand = connection.CreateCommand();
                addCanonicalColumnCommand.CommandText =
                    $"ALTER TABLE customer_signup ADD COLUMN `{column.Name}` {column.Type} NULL;";
                await addCanonicalColumnCommand.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        await using var legacyPasswordColumnCommand = connection.CreateCommand();
        legacyPasswordColumnCommand.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'customer_signup'
              AND COLUMN_NAME = 'passwod';
            """;
        var hasLegacyPassword = Convert.ToInt32(
            await legacyPasswordColumnCommand.ExecuteScalarAsync(cancellationToken)) > 0;

        await using var syncCanonicalCustomerCommand = connection.CreateCommand();
        syncCanonicalCustomerCommand.CommandText = hasLegacyPassword
            ? """
                UPDATE customer_signup
                SET full_name = COALESCE(NULLIF(full_name, ''), name),
                    password = COALESCE(NULLIF(password, ''), passwod, '')
                WHERE full_name IS NULL OR full_name = '' OR password IS NULL OR password = '';
                """
            : """
                UPDATE customer_signup
                SET full_name = COALESCE(NULLIF(full_name, ''), name),
                    password = COALESCE(password, '')
                WHERE full_name IS NULL OR full_name = '' OR password IS NULL;
                """;
        await syncCanonicalCustomerCommand.ExecuteNonQueryAsync(cancellationToken);

        await using var alterCustomerSignupCommand = connection.CreateCommand();
        alterCustomerSignupCommand.CommandText = """
            ALTER TABLE customer_signup
                MODIFY COLUMN id INT NOT NULL AUTO_INCREMENT,
                MODIFY COLUMN full_name VARCHAR(255) NOT NULL,
                MODIFY COLUMN gmail VARCHAR(255) NOT NULL,
                MODIFY COLUMN password VARCHAR(255) NOT NULL,
                MODIFY COLUMN confirm_password VARCHAR(255) NULL;
            """;
        await alterCustomerSignupCommand.ExecuteNonQueryAsync(cancellationToken);

        foreach (var column in new[]
        {
            (Name: "name", Type: "VARCHAR(255)"),
            (Name: "phoneno", Type: "VARCHAR(30)"),
            (Name: "address", Type: "VARCHAR(500)"),
            (Name: "passwod", Type: "VARCHAR(255)"),
            (Name: "conformpassword", Type: "VARCHAR(255)")
        })
        {
            await using var customerColumnCommand = connection.CreateCommand();
            customerColumnCommand.CommandText = """
                SELECT COUNT(*)
                FROM information_schema.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'customer_signup'
                  AND COLUMN_NAME = @column;
                """;
            customerColumnCommand.Parameters.AddWithValue("@column", column.Name);
            var hasColumn = Convert.ToInt32(
                await customerColumnCommand.ExecuteScalarAsync(cancellationToken)) > 0;

            if (!hasColumn)
            {
                await using var addCustomerColumnCommand = connection.CreateCommand();
                addCustomerColumnCommand.CommandText =
                    $"ALTER TABLE customer_signup ADD COLUMN `{column.Name}` {column.Type} NULL;";
                await addCustomerColumnCommand.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        await using var alterLegacyCustomerColumnsCommand = connection.CreateCommand();
        alterLegacyCustomerColumnsCommand.CommandText = """
            ALTER TABLE customer_signup
                MODIFY COLUMN name VARCHAR(255) NULL,
                MODIFY COLUMN passwod VARCHAR(255) NULL,
                MODIFY COLUMN conformpassword VARCHAR(255) NULL,
                MODIFY COLUMN phoneno VARCHAR(30) NULL,
                MODIFY COLUMN address VARCHAR(500) NULL;
            """;
        await alterLegacyCustomerColumnsCommand.ExecuteNonQueryAsync(cancellationToken);

        await using var syncCustomerNameCommand = connection.CreateCommand();
        syncCustomerNameCommand.CommandText = """
            UPDATE customer_signup
            SET name = full_name
            WHERE name IS NULL OR name = '';
            """;
        await syncCustomerNameCommand.ExecuteNonQueryAsync(cancellationToken);

        await using var legacyCustomerConfirmColumnCommand = connection.CreateCommand();
        legacyCustomerConfirmColumnCommand.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'customer_signup'
              AND COLUMN_NAME = 'conform_password';
            """;
        var hasLegacyCustomerConfirmPassword = Convert.ToInt32(
            await legacyCustomerConfirmColumnCommand.ExecuteScalarAsync(cancellationToken)) > 0;

        if (hasLegacyCustomerConfirmPassword)
        {
            await using var alterLegacyCustomerConfirmCommand = connection.CreateCommand();
            alterLegacyCustomerConfirmCommand.CommandText = """
                ALTER TABLE customer_signup
                    MODIFY COLUMN conform_password VARCHAR(255) NULL;
                """;
            await alterLegacyCustomerConfirmCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var compactCustomerConfirmColumnCommand = connection.CreateCommand();
        compactCustomerConfirmColumnCommand.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'customer_signup'
              AND COLUMN_NAME = 'conformpassword';
            """;
        var hasCompactCustomerConfirmPassword = Convert.ToInt32(
            await compactCustomerConfirmColumnCommand.ExecuteScalarAsync(cancellationToken)) > 0;

        if (hasCompactCustomerConfirmPassword)
        {
            await using var alterCompactCustomerConfirmCommand = connection.CreateCommand();
            alterCompactCustomerConfirmCommand.CommandText = """
                ALTER TABLE customer_signup
                    MODIFY COLUMN conformpassword VARCHAR(255) NULL;
                """;
            await alterCompactCustomerConfirmCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var table in new[] { "signup", "login" })
        {
            await using var createLegacyCommand = connection.CreateCommand();
            createLegacyCommand.CommandText = $"""
                CREATE TABLE IF NOT EXISTS `{table}` (
                    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    gmail VARCHAR(255) NOT NULL,
                    name VARCHAR(255) NOT NULL,
                    password VARCHAR(255) NOT NULL
                );
                """;
            await createLegacyCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var signupCommand = connection.CreateCommand();
        signupCommand.CommandText = """
            ALTER TABLE signup
                MODIFY COLUMN id INT NOT NULL AUTO_INCREMENT,
                MODIFY COLUMN gmail VARCHAR(255) NOT NULL,
                MODIFY COLUMN name VARCHAR(255) NOT NULL,
                MODIFY COLUMN password VARCHAR(255) NOT NULL;
            """;
        await signupCommand.ExecuteNonQueryAsync(cancellationToken);

        await using var columnCommand = connection.CreateCommand();
        columnCommand.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'login'
              AND COLUMN_NAME = 'pasword';
            """;
        var hasMisspelledPassword = Convert.ToInt32(
            await columnCommand.ExecuteScalarAsync(cancellationToken)) > 0;

        await using var loginNameColumnCommand = connection.CreateCommand();
        loginNameColumnCommand.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'login'
              AND COLUMN_NAME = 'name';
            """;
        var hasLoginName = Convert.ToInt32(
            await loginNameColumnCommand.ExecuteScalarAsync(cancellationToken)) > 0;

        if (!hasLoginName)
        {
            await using var addLoginNameCommand = connection.CreateCommand();
            addLoginNameCommand.CommandText = """
                ALTER TABLE login ADD COLUMN name VARCHAR(255) NULL;
                UPDATE login SET name = gmail WHERE name IS NULL OR name = '';
                """;
            await addLoginNameCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var loginCommand = connection.CreateCommand();
        loginCommand.CommandText = hasMisspelledPassword
            ? """
            ALTER TABLE login
                MODIFY COLUMN id INT NOT NULL AUTO_INCREMENT,
                MODIFY COLUMN gmail VARCHAR(255) NOT NULL,
                MODIFY COLUMN name VARCHAR(255) NOT NULL,
                CHANGE COLUMN pasword password VARCHAR(255) NOT NULL;
            """
            : """
            ALTER TABLE login
                MODIFY COLUMN id INT NOT NULL AUTO_INCREMENT,
                MODIFY COLUMN gmail VARCHAR(255) NOT NULL,
                MODIFY COLUMN name VARCHAR(255) NOT NULL,
                MODIFY COLUMN password VARCHAR(255) NOT NULL;
            """;
        await loginCommand.ExecuteNonQueryAsync(cancellationToken);

        foreach (var table in new[] { "admin_signup", "admin_login" })
        {
            await using var adminCommand = connection.CreateCommand();
            adminCommand.CommandText = $"""
                CREATE TABLE IF NOT EXISTS `{table}` (
                    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                    name VARCHAR(255) NOT NULL,
                    gmail VARCHAR(255) NOT NULL,
                    password VARCHAR(255) NOT NULL
                );
                """;
            await adminCommand.ExecuteNonQueryAsync(cancellationToken);

            await using var adminNameColumnCommand = connection.CreateCommand();
            adminNameColumnCommand.CommandText = """
                SELECT COUNT(*)
                FROM information_schema.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = @table
                  AND COLUMN_NAME = 'name';
                """;
            adminNameColumnCommand.Parameters.AddWithValue("@table", table);
            var hasNameColumn = Convert.ToInt32(
                await adminNameColumnCommand.ExecuteScalarAsync(cancellationToken)) > 0;

            if (!hasNameColumn)
            {
                await using var addNameCommand = connection.CreateCommand();
                addNameCommand.CommandText = $"ALTER TABLE `{table}` ADD COLUMN name VARCHAR(255) NOT NULL AFTER id;";
                await addNameCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await using var alterAdminCommand = connection.CreateCommand();
            alterAdminCommand.CommandText = $"""
                ALTER TABLE `{table}`
                    MODIFY COLUMN id INT NOT NULL AUTO_INCREMENT,
                    MODIFY COLUMN name VARCHAR(255) NOT NULL,
                    MODIFY COLUMN gmail VARCHAR(255) NOT NULL,
                    MODIFY COLUMN password VARCHAR(255) NOT NULL;
                """;
            await alterAdminCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var confirmPasswordColumnCommand = connection.CreateCommand();
        confirmPasswordColumnCommand.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'admin_signup'
              AND COLUMN_NAME = 'conformpassword';
            """;
        var hasConfirmPasswordColumn = Convert.ToInt32(
            await confirmPasswordColumnCommand.ExecuteScalarAsync(cancellationToken)) > 0;

        if (hasConfirmPasswordColumn)
        {
            await using var alterConfirmPasswordCommand = connection.CreateCommand();
            alterConfirmPasswordCommand.CommandText = """
                ALTER TABLE admin_signup
                    MODIFY COLUMN conformpassword VARCHAR(255) NULL;
                """;
            await alterConfirmPasswordCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public Task AddSignupAsync(
        string email,
        string name,
        string phoneNumber,
        string address,
        string passwordHash,
        CancellationToken cancellationToken) =>
        AddCustomerSignupAsync(email, name, phoneNumber, address, passwordHash, cancellationToken);

    public Task AddLoginAsync(
        string email,
        string name,
        string passwordHash,
        CancellationToken cancellationToken) =>
        AddAsync("login", email, name, passwordHash, cancellationToken);

    public Task AddAdminSignupAsync(
        string email,
        string name,
        string passwordHash,
        CancellationToken cancellationToken) =>
        AddAsync("admin_signup", email, name, passwordHash, cancellationToken);

    public Task AddAdminLoginAsync(
        string email,
        string name,
        string passwordHash,
        CancellationToken cancellationToken) =>
        AddAsync("admin_login", email, name, passwordHash, cancellationToken);

    public async Task<MySqlSignupRecord?> FindSignupAsync(
        string email,
        CancellationToken cancellationToken) =>
        await FindCustomerSignupAsync(email, cancellationToken);

    public async Task<MySqlSignupRecord?> FindAdminSignupAsync(
        string email,
        CancellationToken cancellationToken) =>
        await FindSignupAsync("admin_signup", email, cancellationToken);

    public async Task<IReadOnlyList<CustomerSignupDetails>> GetCustomerSignupsAsync(
        CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
                 SELECT id, COALESCE(NULLIF(name, ''), full_name) AS customer_name,
                     gmail, COALESCE(phoneno, '') AS phoneno, COALESCE(address, '') AS address
            FROM customer_signup
            ORDER BY id DESC;
            """;

        var customers = new List<CustomerSignupDetails>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            customers.Add(new CustomerSignupDetails(
                reader.GetInt32("id"),
                reader.GetString("customer_name"),
                reader.GetString("gmail"),
                reader.GetString("phoneno"),
                reader.GetString("address")));
        }

        return customers;
    }

    private async Task AddCustomerSignupAsync(
        string email,
        string name,
        string phoneNumber,
        string address,
        string passwordHash,
        CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO customer_signup
                (full_name, name, gmail, password, confirm_password, passwod, conformpassword, phoneno, address)
            VALUES
                (@name, @name, @email, @passwordHash, @passwordHash, @passwordHash, @passwordHash, @phoneNumber, @address);
            """;
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@passwordHash", passwordHash);
        command.Parameters.AddWithValue("@phoneNumber", phoneNumber);
        command.Parameters.AddWithValue("@address", address);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<MySqlSignupRecord?> FindCustomerSignupAsync(
        string email,
        CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT gmail, full_name, COALESCE(phoneno, '') AS phoneno
            FROM customer_signup
            WHERE gmail = @email
            ORDER BY id DESC
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("@email", email);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new MySqlSignupRecord(
            reader.GetString("gmail"),
            reader.GetString("full_name"),
            reader.GetString("phoneno"));
    }

    private async Task<MySqlSignupRecord?> FindSignupAsync(
        string table,
        string email,
        CancellationToken cancellationToken)
    {
        if (table is not ("signup" or "admin_signup"))
            throw new ArgumentOutOfRangeException(nameof(table));

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT gmail, name, password
            FROM `{table}`
            WHERE gmail = @email
            ORDER BY id DESC
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("@email", email);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new MySqlSignupRecord(
            reader.GetString("gmail"),
            reader.GetString("name"),
            reader.GetString("password"));
    }

    private async Task AddAsync(
        string table,
        string email,
        string name,
        string passwordHash,
        CancellationToken cancellationToken)
    {
        if (table is not ("signup" or "login" or "admin_signup" or "admin_login"))
            throw new ArgumentOutOfRangeException(nameof(table));

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            INSERT INTO `{table}` (gmail, name, password)
            VALUES (@email, @name, @passwordHash);
            """;
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@passwordHash", passwordHash);
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
