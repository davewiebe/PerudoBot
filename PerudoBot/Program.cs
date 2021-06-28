using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PerudoBot.Database.Data;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace PerudoBot
{
    partial class Program
    {
        private static void Main(string[] args)
        {
            var p = new Program();
            p.RunBotASync().GetAwaiter().GetResult();
        }

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private IConfigurationRoot _configuration;
        private ILogger<Program> _logger;

        public async Task RunBotASync()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

            var config = new DiscordSocketConfig { MessageCacheSize = 100, LogLevel = LogSeverity.Verbose };
            _client = new DiscordSocketClient(config);
            _commands = new CommandService();

            // Specifying the configuration for serilog
            Log.Logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(_configuration)
                            .Enrich.FromLogContext()
                            .WriteTo.SQLite(_configuration.GetConnectionString("SerilogDb"))
                            .WriteTo.Console()
                            .MinimumLevel.Debug()
                            .CreateLogger();

            var memory = new MemoryCache(new MemoryCacheOptions());
            _services = new ServiceCollection()
                .AddMemoryCache()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_configuration)
                .AddLogging(configure => configure.AddSerilog())
                .AddEntityFrameworkSqlite()
                .AddDbContext<PerudoBotDbContext>(options =>
                    options.UseSqlite(_configuration.GetConnectionString("PerudoBotDb")))
                .BuildServiceProvider();


            _logger = _services.GetRequiredService<ILogger<Program>>();
            _logger.LogInformation("App Starting");

            var token = _configuration.GetSection("DiscordToken").Value;
            _client.Log += OnLogAsync;

            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task OnLogAsync(LogMessage msg)
        {
            string logText = $": {msg.Exception?.ToString() ?? msg.Message}";
            switch (msg.Severity.ToString())
            {
                case "Critical":
                    {
                        _logger.LogCritical(logText);
                        break;
                    }
                case "Warning":
                    {
                        _logger.LogWarning(logText);
                        break;
                    }
                case "Info":
                    {
                        _logger.LogInformation(logText);
                        break;
                    }
                case "Verbose":
                    {
                        _logger.LogInformation(logText);
                        break;
                    }
                case "Debug":
                    {
                        _logger.LogDebug(logText);
                        break;
                    }
                case "Error":
                    {
                        _logger.LogError(logText);
                        break;
                    }
            }


            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            _client.ReactionAdded += HandleReactionAddedAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> before, ISocketMessageChannel after, SocketReaction reaction)
        {
            var message = await before.GetOrDownloadAsync();

            if (message == null) return;
            if (IsReactionMine(reaction) || !IsMessageMine(message)) return;

            var context = new CommandContext(_client, message);
            Console.WriteLine($"Handling reaction of {reaction.Emote.Name}");
            var result = await _commands.ExecuteAsync(context, reaction.Emote.Name, _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (arg is not SocketUserMessage message) return;
            var context = new SocketCommandContext(_client, message);

            IResult result = null;

            var prefix = _configuration.GetSection("BotCommandPrefix").Value;

            var argPos = 0;
            if (message.HasStringPrefix(prefix, ref argPos))
            {
                result = await _commands.ExecuteAsync(context, argPos, _services);
            }
            else if (message.HasStringPrefix(context.Client.CurrentUser.Mention, ref argPos))
            {
                result = await _commands.ExecuteAsync(context, "version", _services);
            }
            if (result != null)
            {
                if (!result.IsSuccess)
                {

                    switch (result.Error)
                    {
                        case CommandError.BadArgCount:
                            //await context.Channel.SendMessageAsync("Bad argument count.");
                            break;
                        case CommandError.UnknownCommand:
                            break;
                        case CommandError.Exception:
                            if (result is ExecuteResult execResult)
                            {
                                //you can now access execResult.Exception to see what happened
                                _logger.LogError(exception: execResult.Exception, message: execResult.ErrorReason, args: null);
                            }
                            break;
                        default:
                            _logger.LogError(result.ErrorReason);
                            break;
                    }
                    

                }
            }
        }

        private bool IsMessageMine(IUserMessage message)
        {
            return _client.CurrentUser.Id == message.Author.Id;
        }

        private bool IsReactionMine(SocketReaction reaction)
        {
            return _client.CurrentUser.Id == reaction.UserId;
        }
    }
}