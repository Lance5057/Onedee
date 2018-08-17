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

        //Invoke with ~v d10 d12 d4 pd6...
        public async Task GenerateProbabilityCode(SocketCommandContext context, bool b, int pd, params string[] content)
        {
            string functionLine = "function: highest N:n of";
            int normalDice = 0;
            List<string> dice = new List<string>();
            List<string> plotDice = new List<string>();
            string parameterList = "";
            string sortLine = "result: {1..N}@[sort {";
            List<string> letterList = new List<string>();
            string outputLine = "output [highest 2 of ";
            //Loop through command and grab all of the normal die and plot die
            foreach (string s in content){
                if (s.Contains("pd")){
                    plotDice.Add(s);
                }
                else if (s.Contains("d"))
                {
                    normalDice += 1;
                    dice.Add(s);
                }
            }
            //Generate function parameter list
            for (int i = 65; i < normalDice + 65; i++)
            {
                char character = (char) unicode;
                letterList.Add(character.ToString());
            }
            //Generate sorting line
            foreach (string s in letterList)
            {
                parameterList += s + ":s ";
            }
            parameterList += "{";
            sortLine += string.Join(", ", letterList.ToArray()) + "}]";
            //Generate output line
            outputLine += string.Join(" ", normalDice.ToArray()) + "] + " + string.Join(" ", plotDice.ToArray());
        }
    }
}
