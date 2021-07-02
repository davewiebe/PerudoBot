using Microsoft.EntityFrameworkCore;
using PerudoBot.Database.Data;
using PerudoBot.GameService.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PerudoBot.GameService
{
    public class ExampleDecorator : Decorator
    {
        public ExampleDecorator(IGameObject game) : base(game)
        {
        }

        public override void OnEndOfRound()
        {
            base.OnEndOfRound();
                        
            Console.WriteLine("Do stuff");
        }
    }
}