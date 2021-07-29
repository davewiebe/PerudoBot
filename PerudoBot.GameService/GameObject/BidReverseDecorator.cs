using Microsoft.EntityFrameworkCore;
using PerudoBot.Database.Data;
using PerudoBot.GameService.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PerudoBot.GameService
{
    public class BidReverseDecorator : Decorator
    {
        public BidReverseDecorator(IGameObject game) : base(game)
        {
        }

    }
}