using System;
using System.Threading.Tasks;
using PerudoBot.GameService;
//using Discord;

namespace Perudo.GameHandler
{
    public class GameExtensions
    {
        private GameObject _game;
        public GameExtensions(GameObject game)
        {
            _game = game;
        }

        //public async Task Poppity(Func<string, bool, Embed, RequestOptions, AllowedMentions, MessageReference, Task<IUserMessage>> replyAsync)
        //{
        //    await replyAsync("poppity", false, null, null, null, null);
        //}
    }
}
