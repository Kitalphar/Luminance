using Microsoft.Data.Sqlite;

internal class GuardedSQLiteConnection : IDisposable
{
    private readonly SqliteConnection _innerConnection;
    private readonly string _callerContext;
    private static bool _accessAllowed = false;

    public static bool IsAccessAllowed => _accessAllowed;

    public static void AllowAccess() => _accessAllowed = true;
    public static void DenyAccess() => _accessAllowed = false;

    public GuardedSQLiteConnection(string connectionString, string callerContext = "Unknown")
    {
        if (!_accessAllowed)
            throw new InvalidOperationException($"Unauthorized SQLite access detected from: {callerContext}");

        _innerConnection = new SqliteConnection(connectionString);
        _callerContext = callerContext;
        _innerConnection.Open();
    }

    public SqliteCommand CreateCommand() => _innerConnection.CreateCommand();

    public void Dispose() => _innerConnection.Dispose();
}
