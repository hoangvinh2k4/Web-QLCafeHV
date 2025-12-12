namespace QLCafeHV
{
    public static class Config
    {
        public static IConfiguration configuration { get; set; }
        public class AppSettings
        {
            private static readonly IConfigurationRoot configuration;

            static AppSettings()
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                configuration = builder.Build();
            }

            public static string Get(string key)
            {
                return configuration[key];
            }
        }
    }
}
