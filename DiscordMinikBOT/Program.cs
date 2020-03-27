using System;

using Discord;
using Discord.WebSocket;

using System.IO;

using System.Threading.Tasks;

using System.Collections.Generic;

using PLplot;

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

        public static void RespondToCommand(string response, SocketMessage message)
        {
            SendMessage(message.Author.Mention + ", " + response, message.Channel);
        }

        class CovidCommand
        {
            public enum Types
            {
                deaths,
                confirmed,
                deathPercentage,
                all,
            }

            public struct Parameters
            {
                public bool chart;
                public bool info;
            }


            public Types commandType;
            public string country;
            public SocketMessage channel;
            public DateTime date;
            public Parameters param;


            public CovidCommand(Types commandType, string country, DateTime date, SocketMessage channel, Parameters param)
            {
                this.commandType = commandType;
                this.country = country.ToLower();
                this.channel = channel;
                this.date = date;
                this.param = param;
            }

            private string ParseData(CovidParser.Country.Data data, string country)
            {
                string countryName = (char)(country[0]-32) + country.Remove(0, 1);

                return "\n" + countryName + " on " + data.date + ": \n\tDeaths:  \t\t" + data.deaths + "\n\tConfirmed:\t" + data.confirmed + "\n\tRecovered:\t" + data.recovered;
            }

            public void Execute(List<CovidParser.Country> data)
            {
                CovidParser.Country _country = null;
                CovidParser.Country.Data countryData = new CovidParser.Country.Data();
                   
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].name == country)
                    {
                        if (date == DateTime.MinValue)
                        {
                            _country = data[i];
                            countryData = data[i].data[data[i].data.Count - 1];
                        }
                        else
                        {
                            string toFind = date.Year.ToString() + "-" + date.Month.ToString() + "-" + date.Day.ToString();
                            foreach (CovidParser.Country.Data country in data[i].data)
                            {
                                if (country.date == toFind)
                                {
                                    _country = data[i];
                                    countryData = country;
                                    break;
                                }
                            }
                        }

                        break;
                    }
                }

                if(_country == null)
                {
                    RespondToCommand("Country not found", channel);
                    return;
                }

                if (param.info)
                {
                    RespondToCommand(ParseData(countryData, country), channel);
                }
                if(param.chart)
                {
                    CreateCovidChart(_country, CovidChartTypes.all);

                    channel.Channel.SendFileAsync("covid.png");
                }
            }
        }

        private DateTime covidLastUpdate = DateTime.MinValue;
        private string covidDataFilename = "covid.json";
        private List<CovidCommand> covidChannels = new List<CovidCommand>();
        private List<CovidParser.Country> covidData;
        private bool downloadingCovidData = false;
        private void ParseCovidData()
        {
            

            for(int i = 0;i<covidChannels.Count;i++)
            {
                //RespondToCommand(covidChannels[i].Execute(covidData), covidChannels[i].channel);
                covidChannels[i].Execute(covidData);
            }

            covidChannels.Clear();
            downloadingCovidData = false;
        }

        private void ReadCovidData()
        {
            string data = File.ReadAllText(covidDataFilename);
            ParseCovidData();
        }

        private void CovidDataUpdated(string data)
        {
            Debug.Log("Covid data updated");
            covidLastUpdate = DateTime.Now;

            covidData = CovidParser.Parse(data);
            ReadCovidData();
        }

        private void UpdateCovidData()
        {
            if (!downloadingCovidData)
            {
                Debug.Log("Downloading file...");
                HTTPDownloader.DownloadFile("https://pomber.github.io/covid19/timeseries.json", CovidDataUpdated, covidDataFilename);
            }
        }

        private void RequestCovidData(SocketMessage message, string parameters)
        {
            string[] msgParams = parameters.Split(' ');
            if (msgParams.Length == 0 || msgParams[0].Length == 0)
            {
                RespondToCommand("No parameters specified, usage: " + prefix + "!covid <country> [date / last] [-chart / -info]", message);
                return;
            }

            bool chart, info;
            chart = info = false;
            string country = "";
            DateTime date = DateTime.MinValue;

            country = msgParams[0];

            DateTime t = DateTime.Now;

            for(int i = 0;i<msgParams.Length;i++)
            {
                if(msgParams[i] == "-chart")
                {
                    chart = true;
                }
                if (msgParams[i] == "-info")
                {
                    info = true;
                }
            }

            if(chart == false)
            {
                info = true;
            }

            covidChannels.Add(new CovidCommand(CovidCommand.Types.all, country, date, message, new CovidCommand.Parameters() { chart = chart, info = info }));
            if (t.DayOfWeek != covidLastUpdate.DayOfWeek)
            {
                UpdateCovidData();
            }
            else
            {
                ReadCovidData();
            }
            
        }

        public enum CovidChartTypes
        {
            deaths,
            confirmed,
            recovered,
            all
        }

        public static CovidParser.Country.Data FindMax(CovidParser.Country country, CovidChartTypes type)
        {
            int maxPos = -1;
            long max = -1;
            for(int i = 0;i<country.data.Count;i++)
            {
                long c = 0;
                switch (type)
                {
                    case CovidChartTypes.deaths:
                        c = country.data[i].deaths;
                        break;
                    case CovidChartTypes.confirmed:
                        c = country.data[i].confirmed;
                        break;
                    case CovidChartTypes.recovered:
                        c = country.data[i].recovered;
                        break;
                    case CovidChartTypes.all:
                        c = country.data[i].confirmed;
                        break;
                    default:
                        break;
                }

                if(c > max)
                {
                    maxPos = i;
                }
            }

            return country.data[maxPos];
        }

        public static void CreateCovidChart(CovidParser.Country country, CovidChartTypes type)
        {

            int xMin = 0;
            int xMax = country.data.Count;
            int yMin = 0;
            CovidParser.Country.Data c = FindMax(country, type);
            long yMax = 0;

            string title = "COVID-19 in " + country.name;
            switch (type)
            {
                case CovidChartTypes.deaths:
                    yMax = c.deaths;
                    title += " deaths";
                    break;
                case CovidChartTypes.confirmed:
                    yMax = c.confirmed;
                    title += " confirmed";
                    break;
                case CovidChartTypes.recovered:
                    yMax = c.recovered;
                    title += " recovered";
                    break;
                case CovidChartTypes.all:
                    yMax = c.confirmed;
                    break;
                default:
                    break;
            }



            List<double>[] values = null; //new List<double>();
            List<double> keys = new List<double>();
            for (int i = 0; i < country.data.Count; i++)
            {
                keys.Add(i);
                switch (type)
                {
                    case CovidChartTypes.deaths:
                        if(values == null)
                        {
                            values = new List<double>[1];
                            values[0] = new List<double>();
                        }
                        values[0].Add(country.data[i].deaths);
                        
                        break;
                    case CovidChartTypes.confirmed:
                        if (values == null)
                        {
                            values = new List<double>[1];
                            values[0] = new List<double>();
                        }
                        values[0].Add(country.data[i].confirmed);
                       
                        break;
                    case CovidChartTypes.recovered:
                        if (values == null)
                        {
                            values = new List<double>[1];
                            values[0] = new List<double>();
                        }
                        values[0].Add(country.data[i].recovered);
                        
                        break;
                    case CovidChartTypes.all:
                        //Debug.LogError("All is not supported yet");
                        if (values == null)
                        {
                            values = new List<double>[3];
                            values[0] = new List<double>();
                            values[1] = new List<double>();
                            values[2] = new List<double>();
                        }
                        values[0].Add(country.data[i].confirmed);
                        values[1].Add(country.data[i].deaths);
                        values[2].Add(country.data[i].recovered);
                        break;
                    default:
                        break;
                }
            }

            var pl = new PLStream();

            pl.sdev("pngcairo");
            pl.sfnam("covid.png");

            pl.init();



            pl.env(xMin, xMax, yMin, yMax, AxesScale.Independent, AxisBox.BoxTicksLabels);

            pl.lab("Days", "Cases",  title);


            
            for (int i = 0; i < values.Length; i++)
            {
                pl.col0(15 - i);
                pl.line(keys.ToArray(), values[i].ToArray());
            }

            pl.eop();

            pl.ResetOpts();

            IDisposable disp = (IDisposable)pl;
            disp.Dispose();

            //pl.gver(out var varText);
            //Debug.Log("Plplot version: " + varText);
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
                    SendMessage("It's " + discord.GetUser(544876161331625994).Mention + " :3", message.Channel);
                    break;
                case "misio":
                case "misiu":
                case "mis":
                case "miś":
                    SendMessage("It's " + discord.GetUser(308705467209875456).Mention + " :3", message.Channel);
                    break;
                case "weather":
                    RespondToCommand("A weź spierdalaj :)", message);
                    break;

                case "covid":
                    Debug.Log("Covid");
                    RequestCovidData(message, rest);
                    
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

        public static void SendMessage(string message, ISocketMessageChannel channel)
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
            if(File.Exists(covidDataFilename))
            {
                covidLastUpdate = File.GetLastAccessTime(covidDataFilename);

                DateTime t = DateTime.Now;

                if(covidLastUpdate.Date != t.Date)
                {
                    UpdateCovidData();
                }
                else
                {
                    covidData = CovidParser.Parse(File.ReadAllText(covidDataFilename));
                }
            }

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
