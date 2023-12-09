namespace ChurchDiscordBot.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplets(this IServiceCollection services,
            HostConfig config)
        {
            return services;
        }
    }
}
