using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Data;
using LiteDB;

namespace DiscordBot.Modules
{
    public class RollModule : ModuleBase<SocketCommandContext>
    {
        // This property will be filled in at runtime by the IoC container (Program.cs:49)
        //public LiteDatabase Database { get; set; }
        Random rnd = new Random();
        //SocketGuildUser dm;

        [Command("R"), Alias("roll")]
        public Task RD(params string[] content) => RollDice(Context, true, 0, content);

        [Command("test")]
        public Task TEST(params string[] content) => RollDice(Context, false, 0, content);

        [Command("R"), Alias("roll")]
        public Task RD(int pp, params string[] content) => RollDice(Context, true, pp, content);

        [Command("test")]
        public Task TEST(int pp, params string[] content) => RollDice(Context, false, pp, content);

        private async Task RollDice(SocketCommandContext context, bool b, int pd, params string[] content)
        {
            List<int> rolls = new List<int>();
            List<int> plotRolls = new List<int>();
            int ones = 0;

            foreach (string s in content)
            {
                if (s.Contains("pd"))
                {
                    string str = Regex.Replace(s, "[^0-9.]", "");

                    int i = Int32.Parse(str);
                    plotRolls.Add(rnd.Next(1, i));
                }
                else
                {
                    string str = Regex.Replace(s, "[^0-9.]", "");

                    int i = Int32.Parse(str);
                    rolls.Add(rnd.Next(1, i));
                }
            }

            //count the 1's
            foreach(int num in rolls)
            {
                if(num == 1)
                {
                    ones++;
                }
            }

            List<int> numbersNoLowest = new List<int>();
            List<int> numbersDropped = new List<int>();

            //if (rolls.Count < 2 + pd)
            //{
            //    await ReplyAsync("You didn't add enough dice! Expected: " + (2 + pd) + " or more, Received: " + rolls.Count);
            //}

            if (rolls.Count > 1)
            {
                rolls.Sort();
                rolls.Reverse();
                numbersNoLowest.AddRange(rolls.GetRange(0, 2 + pd));
                if (rolls.Count > 2 + pd)
                    numbersDropped = rolls.GetRange(2 + pd, rolls.Count - (2 + pd));
            }

            int total = 0;
            if (rolls.Count > 1)
            {
                foreach (double num in numbersNoLowest)
                {
                    total += (int)num;
                }
            }
            else
            {
                total += rolls[0];
            }

            foreach (double num in plotRolls)
            {
                total += (int)num;
            }

            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = Context.User.ToString(),
                    IconUrl = Context.User.GetAvatarUrl()
                },
                Color = new Color(Convert.ToUInt32(rnd.Next(16777215))), // color of the day
            };

            string dice = "";
            string plot = "";

            if (rolls.Count > 1)
            {
                foreach (double i in numbersNoLowest)
                {
                    dice += i + " ";
                }
                embed.AddInlineField("Top Picks:", dice);

                if (plotRolls.Count > 1)
                {
                    foreach (double i in plotRolls)
                    {
                        plot += i + " ";
                    }
                    embed.AddInlineField("Plot Dice:", plot);
                }

                string dropped = "";

                foreach (double i in numbersDropped)
                {
                    dropped += i + " ";
                }

                if (dropped != "")
                    embed.AddInlineField("Dropped: ", dropped);

                embed.AddInlineField("Total: ", total);
            }
            else
            {
                embed.AddInlineField("Top Picks:", rolls[0]);

                if (plotRolls.Count > 1)
                {
                    foreach (double i in plotRolls)
                    {
                        plot += i + " ";
                    }
                    embed.AddInlineField("Plot Dice:", plot);
                }

                embed.AddInlineField("Total: ", total);
            }
            
            await ReplyAsync("", embed: embed);

            if (ones > 0 && b)
            {
                var msg1 = await ReplyAsync("~p add " + context.User.Mention + " 1");
                var msg2 = await ReplyAsync("~p add " + FindDM().Mention + " " + ones);

                await msg1.DeleteAsync();
                await msg2.DeleteAsync();
                //await AddPlotPoints(context, dm, ones);
                //await AddPlotPoints(context, context.User, 1);
            }

            ones = 0;
        }

        //public async Task AddPlotPoints(SocketCommandContext context, IUser user, int pp)
        //{
        //    var users = Database.GetCollection<User>("users");
        //    var use = users.FindOne(u => u.Id == user.Id) ?? new User { Id = user.Id };
        //    use.PP += pp;
        //    users.Upsert(use);

        //    await SendPointsAsync(user);
        //}

        //private async Task SendPointsAsync(IUser user)
        //{
        //    var users = Database.GetCollection<User>("users");
        //    var model = users.FindOne(u => u.Id == user.Id);

        //    var points = model?.PP ?? 0;

        //    var embed = new EmbedBuilder
        //    {
        //        Author = new EmbedAuthorBuilder
        //        {
        //            Name = user.ToString(),
        //            IconUrl = user.GetAvatarUrl()
        //        },
        //        Color = new Color(Convert.ToUInt32(rnd.Next(16777215))), // color of the day
        //    };
        //    embed.AddField("PP", points);
        //    // Levels?

        //    await ReplyAsync("", embed: embed);
        //}

        SocketGuildUser FindDM()
        {
                var u = Context.Guild.GetTextChannel(Context.Channel.Id).Users;

            foreach (var v in u)
            {
                foreach (var r in v.Roles)
                {
                    if (r.Name == "Storyteller")
                    {
                        return v;
                    }
                }
            }

            return null;
        }
    }
}
