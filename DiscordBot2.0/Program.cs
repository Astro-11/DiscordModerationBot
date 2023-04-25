using DSharpPlus;
using System.Security.Cryptography.X509Certificates;

namespace DiscordBot2._0
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DiscordClient discord;

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<DiscordClient>((serviceProvider) =>
                    {
                        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                        var discord = new DiscordClient(new DiscordConfiguration()
                        {
                            Token = "", //Insert your Discord Bot Token here
                            TokenType = TokenType.Bot,
                            Intents = DiscordIntents.All
                        });
                        return discord;
                    });
                    services.AddHostedService<Worker>();
                })
                .Build();

            Console.WriteLine("Token");

            host.Run();
        }
    }
}