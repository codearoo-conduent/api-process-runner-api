namespace api_process_runner_api.Helpers
{
    internal static class Helper
    {
        private static IConfiguration configuration
            = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("local.settings.json").Build();

        public static string GetEnvironmentVariable(string variableName)
        {
            return configuration[variableName] ?? "";
        }

        public static string GetSqlConnectionString(string name)
        {
            var conStr = Environment.GetEnvironmentVariable($"ConnectionStrings:{name}", EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(conStr)) // Azure Functions App Service naming convention
                conStr = Environment.GetEnvironmentVariable($"SQLCONNSTR_{name}", EnvironmentVariableTarget.Process);
            return conStr ?? "";
        }

        public static string GetSqlAzureConnectionString(string name)
        {
            var conStr = Environment.GetEnvironmentVariable($"ConnectionStrings:{name}", EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(conStr)) // Azure Functions App Service naming convention
                conStr = Environment.GetEnvironmentVariable($"SQLAZURECONNSTR_{name}", EnvironmentVariableTarget.Process);
            return conStr ?? "";
        }

        public static string GetMySqlConnectionString(string name)
        {
            var conStr = Environment.GetEnvironmentVariable($"ConnectionStrings:{name}", EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(conStr)) // Azure Functions App Service naming convention
                conStr = Environment.GetEnvironmentVariable($"MYSQLCONNSTR_{name}", EnvironmentVariableTarget.Process);
            return conStr ?? "";
        }

        public static string GetCustomConnectionString(string name)
        {
            var conStr = Environment.GetEnvironmentVariable($"ConnectionStrings:{name}", EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(conStr)) // Azure Functions App Service naming convention
                conStr = Environment.GetEnvironmentVariable($"CUSTOMCONNSTR_{name}", EnvironmentVariableTarget.Process);
            return conStr ?? "";
        }
    }
    public class LogFileGenerator
    {
        public static string GenerateLogFileName()
        {
            DateTime today = DateTime.Today;
            string logFileName = $"LogResults_{today.ToString("yyyy-MM-dd")}_{Guid.NewGuid().ToString().Substring(0, 8)}.json";
            return logFileName;
        }
    }
}