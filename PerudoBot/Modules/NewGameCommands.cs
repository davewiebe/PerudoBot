using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
//using Game = PerudoBot.Data.Game;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("new")]
        public async Task NewGameAsync(params string[] stringArray)
        {
            var game = _gameHandler.CreateGame(Context.Channel.Id, Context.Guild.Id);

            var commands =
                $"`!add/remove @player` to add/remove players.\n" +
                $"`!option xyz` to set round options.\n" +
                $"`!status` to view current status.\n" +
                $"`!start` to start the game.";

            var builder = new EmbedBuilder()
                            .WithTitle($"New game #{game.GetGameNumber()} created")
                            .AddField("Commands", commands, inline: false);
            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

            //await DisplaySetupStatus();
        }
    }
}