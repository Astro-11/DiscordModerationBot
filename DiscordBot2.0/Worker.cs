using DSharpPlus;
using DSharpPlus.CommandsNext;
using MyFirstBot;

namespace DiscordBot2._0
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly DiscordClient discordClient;

        public Worker(ILogger<Worker> logger, DiscordClient discordClient)
        {
            _logger = logger;
            this.discordClient = discordClient;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            Spreadsheet.GoogleSheet();
            MyFirstModule.RefreshSpreadsheet();

            var commands = discordClient.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!" }
            });

            commands.RegisterCommands<MyFirstModule>();

            await discordClient.ConnectAsync();
            Console.WriteLine("Connected");
            await Task.Delay(-1);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await discordClient.DisconnectAsync(); 
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
    }
}