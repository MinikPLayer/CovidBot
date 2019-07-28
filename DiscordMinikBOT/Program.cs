using System;

using Discord;
using Discord.WebSocket;

using System.IO;

using System.Threading.Tasks;

using System.Collections.Generic;

namespace DiscordMinikBOT
{
    public enum Ranks
    {
        none,
        normal,
        VIP,
        SVIP,
        Admin,
        Krol,
        Bot
    }

    public class User
    {
        ulong id;

        Ranks rank;
        
        public User(ulong _id, Ranks _rank)
        {
            id = _id;
            rank = _rank;
        }
    }

    


    class Program
    {
        static string token = "NTY0MDYwNTU4OTc5MjM1ODYx.XTM8WQ.yx9OySa3L_qUhg5tb5D52fXwFpo";
        public static string prefix = "m!";

        List<User> users = new List<User>();

        private DiscordSocketClient discord = new DiscordSocketClient();

        private void RespondToCommand(string response, SocketMessage message)
        {
            SendMessage(message.Author.Mention + ", " + response, message.Channel);
        }

        public Task MessageReceived(SocketMessage message)
        {
            Console.WriteLine("Message received: " + message.Content);

            string msgStr = message.Content;

            if(msgStr.StartsWith(discord.CurrentUser.Mention))
            {
                SendMessage("<@" + message.Author.Id + ">, master is listening, you don't have to shout for him...", message.Channel);

                msgStr = msgStr.Remove(0, ("<@" + discord.CurrentUser.Id + ">").Length);

                msgStr = RemoveSpaces(msgStr);

                //return Task.CompletedTask;
            }

            if(!msgStr.StartsWith(prefix))
            {
                return Task.CompletedTask;
            }
            string userMessage = msgStr.Remove(0, prefix.Length);

            string command = userMessage;//userMessage.ToLower();
            for(int i = 0;i< userMessage.Length;i++)
            {
                if(userMessage[i] == ' ')
                {
                    command = userMessage.Remove(i);
                    break;
                }
            }

            command = command.ToLower();

            string rest = userMessage.Remove(0, command.Length);
            if (rest.Length > 0) rest = rest.Remove(0, 1);

            switch (command)
            {
                case "h":
                case "help":
                    RespondToCommand(Help.message + "\n\nBest player in the world: " + message.Author.Mention + " ;)", message);
                    break;
                case "aliases":
                    RespondToCommand(Help.aliasesMessage, message);
                    break;
                case "whois":
                case "whoami":
                    SocketUser user;
                    if(rest.Length == 0)
                    {
                        user = message.Author;
                    }
                    else
                    {
                        if(rest.StartsWith("<@") || rest.StartsWith("<!@"))
                        {
                            
                            if (rest.StartsWith("<@"))
                            {
                                Debug.Log("Whoami starts with <@: \"" + rest + "\"");
                                rest = rest.Remove(0, "<@".Length);
                            }
                            else
                            {
                                Debug.Log("Whoami starts with <!@");
                                rest = rest.Remove(0, "<!@".Length);

                            }

                            if (rest[0] == '!') rest = rest.Remove(0, 1);

                            bool found = false;
                            for(int i = 0;i<rest.Length;i++)
                            {
                                if(rest[i] == '>')
                                {
                                    found = true;
                                    rest = rest.Remove(i);
                                    break;
                                }
                            }
                            if(!found)
                            {
                                Debug.Log("idk, cos jest nie tak");
                                return Task.CompletedTask;
                            }
                            ulong uid = 0;
                            if (ulong.TryParse(rest, out uid))
                            {
                                user = discord.GetUser(uid);
                            }
                            else
                            {
                                Debug.Log("Unable to parse " + rest + " to ulong");
                                return Task.CompletedTask;
                            }
                        }
                        else
                        {
                            Debug.Log("Invalid argument");
                            RespondToCommand("Invalid parameter: " + rest, message);
                            return Task.CompletedTask;
                        }
                    }

                    string response = "user **" + user.Username + "**\n";
                    response += "Discord id: **" + user.Id + "**\n";
                    response += "Account created at **" + user.CreatedAt + "**\n";
                    response += "Status: **" + user.Status + "**\n";

                    if (user.Id == 315602795895980038)
                    {
                        response += "Bot: **True**\n";
                    }
                    else
                    {
                        response += "Bot: **" + user.IsBot + "**\n";
                    }

                    if (user.Activity == null || user.Activity.ToString() == "")
                    {
                        response += "Activity: **none**\n";
                    }
                    else
                    {
                        response += "Activity: ***  " + user.Activity.Type + "***   **" + user.Activity + "**\n";
                    }

                    response += "Discriminator: **#" + user.Discriminator + "**\n";
                    response += "Liked by " + discord.CurrentUser.Mention + ": **true**\n";
                    string avatarURL = user.GetAvatarUrl(ImageFormat.Png);
                    if (avatarURL != null)
                    {
                        response += "Avatar: **" + avatarURL + "**\n";
                    }
                    else
                    {
                        response += "Avatar: **I'm sorry, but avatar is not set :disappointed: **\n";
                    }
                    
                   

                    RespondToCommand(response, message);
                    break;

                case "kitek":
                    //RespondToCommand("it's " + discord.GetUser(544876161331625994).Mention + " :3", message);
                    SendMessage("It's " + discord.GetUser(544876161331625994).Mention + " :3", message.Channel);
                    break;
                case "misio":
                case "misiu":
                case "mis":
                case "miś":
                    SendMessage("It's " + discord.GetUser(308705467209875456).Mention + " :3", message.Channel);
                    break;
                case "weather":
                    RespondToCommand("Coming soon :)", message);
                    break;
                default:
                    //SendMessage("<@" + message.Author.Id + ">, command **" + command + "** not found, try **" + prefix + "help**", message.Channel);
                    if (command.Length != 0)
                    {
                        RespondToCommand("command **" + command + "** not found, try **" + prefix + "help **", message);
                    }
                    else
                    {
                        RespondToCommand("you're joking right? :stuck_out_tongue: Maybe type any command? You can also try **" + prefix + "help** to get help", message);
                    }
                    break;
            }

            return Task.CompletedTask;
        }

        public void SendMessage(string message, ISocketMessageChannel channel)
        {
            channel.SendMessageAsync(message);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }

        private string RemoveSpaces(string line)
        {
            for(int i = 0;i<line.Length;i++)
            {
                /*if(line[i] == '\"' && i>0 && line[i-1] != '\\')
                {
                    quoteMark = !quoteMark;
                }*/
                if(line[i] == ' ')
                {
                    line = line.Remove(i, 1);
                    i--;
                }
            }

            return line;
        }

        private bool CheckIfStartsAndTrim(string src, out string trimmed, string startsWith, int moreCharsToTrim = 0)
        {
            trimmed = src;

            if(src.StartsWith(startsWith))
            {
                trimmed = src.Remove(0, startsWith.Length + moreCharsToTrim);
                
                return true;
            }

            return false;
        }

        private void ParseConfigLine(string line)
        {
            line = RemoveSpaces(line);

            Debug.Log("Parsing line: " + line);

            /*if(line.StartsWith("token="))
            {
                line = line.Remove(0, "token=".Length);
                token = line;
                Debug.Log("New token: \"" + token + "\"");
            }*/
            string value;

            if(CheckIfStartsAndTrim(line, out value, "token", 1))
            {
                token = value;
                Debug.Log("New token: \"" + token + "\"");
            }
            if (CheckIfStartsAndTrim(line, out value, "prefix", 1))
            {
                prefix = value;
                Debug.Log("New prefix: \"" + value + "\"");
            }

        }

        const string configDir = "config.ini";

        private void LoadConfig()
        {
            

            if(!File.Exists(configDir))
            {
                Debug.LogWarning(configDir + " not found");
                return;
            }

            string[] lines = File.ReadAllLines(configDir);
            for(int i = 0;i<lines.Length;i++)
            {
                ParseConfigLine(lines[i]);
            }
        }

        private void SaveConfig()
        {

        }

        private void AppExit(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Exiting");

            Environment.Exit(-1);
        }

        static void Main(string[] args)
         => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Console.CancelKeyPress += AppExit;

            LoadConfig();

            try
            {
                //discord = new DiscordSocketClient();

                discord.Log += Log;
                discord.MessageReceived += MessageReceived;

                await discord.LoginAsync(TokenType.Bot, token);
                await discord.StartAsync();
            } catch(Exception e)
            {

                Debug.LogError("Fatal error in MainAsync(), " + e.Message);
                await Task.Delay(2000);

                

                return;
            }

            

            

            await Task.Delay(-1);
        }
    }
}
