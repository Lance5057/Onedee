using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBot.Data;
using LiteDB;

namespace DiscordBot.Modules
{
    [Group("P")]
    public class PointsModule : ModuleBase<SocketCommandContext>
    {
        // This property will be filled in at runtime by the IoC container (Program.cs:49)
        public LiteDatabase Database { get; set; }
        Random rnd = new Random();

        // Access the current user's points, under '!points' or '!points me'
        [Command, Alias("me")]
        public Task SelfAsync()
            => SendPointsAsync(Context.User);

        // Access another user's points, under '!points @user'
        [Command]
        public Task UserAsync(IUser user)
            => SendPointsAsync(user);

        [Command("add")]
        public Task GivePP(IUser user) => AddPlotPoints(Context, user, 1);

        [Command("sub"), Alias("subtract")]
        public Task TakePP(IUser user) => SubtractPlotPoints(Context, user, 1);

        [Command("add")]
        public Task GivePP(IUser user, int pp) => AddPlotPoints(Context, user, pp);

        [Command("sub"), Alias("subtract")]
        public Task TakePP(IUser user, int pp) => SubtractPlotPoints(Context, user, pp);

        [Command("add")]
        public Task GivePP() => AddPlotPoints(Context, Context.User, 1);

        [Command("sub"), Alias("subtract")]
        public Task TakePP() => SubtractPlotPoints(Context, Context.User, 1);

        [Command("add")]
        public Task GivePP(int pp) => AddPlotPoints(Context, Context.User, pp);

        [Command("sub"), Alias("subtract")]
        public Task TakePP(int pp) => SubtractPlotPoints(Context, Context.User, pp);

        [Command("addall")]
        public Task GiveAllPP() => AddPointsAll(Context, 1);

        [Command("suball"), Alias("subtractall")]
        public Task TakeAllPP() => SubPointsAll(Context, 1);

        [Command("addall")]
        public Task GiveAllPP(int pp) => AddPointsAll(Context, pp);

        [Command("suball"), Alias("subtractall")]
        public Task TakeAllPP(int pp) => SubPointsAll(Context, pp);

        [Command("set")]
        public Task ChangePP(IUser user, int pp) => SetPlotPoints(Context, user, pp);

        [Command("set")]
        public Task ChangePP(int pp) => SetPlotPoints(Context, Context.User, pp);

        [Command("list")]
        public Task ListPP() => SendAllPointsAsync(Context);

        private async Task SendAllPointsAsync(SocketCommandContext context)
        {
            var u = context.Guild.GetTextChannel(context.Channel.Id).Users;

            foreach(var v in u)
            {
                foreach (var r in v.Roles)
                {
                    if (r.Name == "Black Snows Player")
                    {
                        await SendPointsAsync(v);
                        break;
                    }
                }
            }
        }

        private async Task AddPointsAll(SocketCommandContext context, int pp)
        {
            var u = context.Guild.GetTextChannel(context.Channel.Id).Users;

            foreach (var v in u)
            {
                foreach (var r in v.Roles)
                {
                    if (r.Name == "Black Snows Player")
                    {
                        await AddPlotPoints(context, v, pp);
                        break;
                    }
                }
            }
        }

        private async Task SubPointsAll(SocketCommandContext context, int pp)
        {
            var u = context.Guild.GetTextChannel(context.Channel.Id).Users;

            foreach (var v in u)
            {
                foreach (var r in v.Roles)
                {
                    if (r.Name == "Black Snows Player")
                    {
                        await SubtractPlotPoints(context, v, pp);
                        break;
                    }
                }
            }
        }

        private async Task SendPointsAsync(IUser user)
        {
            var users = Database.GetCollection<User>("users");
            var model = users.FindOne(u => u.Id == user.Id);

            var points = model?.PP ?? 0;

            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = user.ToString(),
                    IconUrl = user.GetAvatarUrl()
                },
                Color = new Color(Convert.ToUInt32(rnd.Next(16777215))), // color of the day
            };
            embed.AddField("PP", points);
            // Levels?

            await ReplyAsync("", embed: embed);
        }

        public async Task AddPlotPoints(SocketCommandContext context, IUser user, int pp)
        {
            var users = Database.GetCollection<User>("users");
            var use = users.FindOne(u => u.Id == user.Id) ?? new User { Id = user.Id };
            use.PP+=pp;
            users.Upsert(use);

            await SendPointsAsync(user);
        }

        private async Task SubtractPlotPoints(SocketCommandContext context, IUser user, int pp)
        {
            var users = Database.GetCollection<User>("users");
            var use = users.FindOne(u => u.Id == user.Id) ?? new User { Id = user.Id };
            if (use.PP > 0)
            {
                use.PP-=pp;
                users.Upsert(use);

                await SendPointsAsync(user);
            }
            else
                await ReplyAsync(user.Mention + " doesn't have enough PP to subtract!");
        }

        private async Task SetPlotPoints(SocketCommandContext context, IUser user, int ppIn)
        {
            var users = Database.GetCollection<User>("users");
            var use = users.FindOne(u => u.Id == user.Id) ?? new User { Id = user.Id };

            use.PP =  ppIn;
            users.Upsert(use);

            await SendPointsAsync(user);
        }
    }
}
