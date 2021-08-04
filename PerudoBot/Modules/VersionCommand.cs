using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("version")]
        public async Task Version(params string[] options)
        {
            var commands =
                $"`!exact` not implemented\n" +
                $"`!deal` not implemented\n" +
                $"`!rattles` not implemented\n" +
                $"`!note` not implemented\n" +
                "*who says you have to have new features to be 2.0?*";

            var builder = new EmbedBuilder()
                            .WithTitle($"PerudoBot2.0 - Version 2.2")
                            .AddField("Less features like...", commands, inline: false);
            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }
    }
}