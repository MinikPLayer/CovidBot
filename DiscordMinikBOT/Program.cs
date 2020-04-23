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

        public CovidParser.Country world
        {
            get
            {
                return CovidParser.world;
            }
        }

        public static CovidParser.Country.Data GetWorldOnDate(CovidParser.Country world, DateTime date)
        {
            if(date == DateTime.MinValue)
            {
                return world.data[world.data.Count - 1];
            }

            string dt = DateToStr(date);
            for(int i = 0;i<world.data.Count;i++)
            {
                if(world.data[i].date == dt)
                {
                    return world.data[i];
                }
            }
            Debug.LogError("Cannot find world data on " + dt);
            return new CovidParser.Country.Data();
        }

        public static string DateToStr(DateTime date)
        {
            return date.Year.ToString() + "-" + date.Month.ToString() + "-" + date.Day.ToString(); ;
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
            public string[] countriesToCompare;
            public SocketMessage channel;
            public DateTime date;
            public DateTime startDate;
            public Parameters param;


            public CovidCommand(Types commandType, string country, DateTime date, SocketMessage channel, Parameters param, string[] countriesToCompare = null)
            {
                FillVariables(commandType, country, date, channel, param, DateTime.MinValue, countriesToCompare);
            }
            public CovidCommand(Types commandType, string country, DateTime date, SocketMessage channel, Parameters param, DateTime chartStartDate, string[] countriesToCompare = null)
            {
                FillVariables(commandType, country, date, channel, param, chartStartDate, countriesToCompare);
            }

            void FillVariables(Types commandType, string country, DateTime date, SocketMessage channel, Parameters param, DateTime chartStartDate, string[] countriesToCompare)
            {
                this.commandType = commandType;
                this.country = country.ToLower();
                this.channel = channel;
                this.date = date;
                this.param = param;
                this.startDate = chartStartDate;

                if (countriesToCompare == null) countriesToCompare = new string[0];
                this.countriesToCompare = new string[countriesToCompare.Length];
                for (int i = 0; i < countriesToCompare.Length; i++)
                {
                    this.countriesToCompare[i] = countriesToCompare[i].ToLower();
                }
            }


            private string ToStringWithDots(long number)
            {
                string val = number.ToString();
                for(int i = val.Length - 3;i>0;i-=3)
                {
                    val = val.Insert(i, ".");
                }

                return val;
            }

            private string GenerateSpaces(string thisStr, string[] rest)
            {
                int maxL = thisStr.Length;
                for(int i = 0;i<rest.Length;i++)
                {
                    if(rest[i].Length > maxL)
                    {
                        maxL = rest[i].Length;
                    }
                }

                string ret = "";
                for(int i = 0;i<(maxL - thisStr.Length) * 1.3 ;i++)
                {
                    ret += ' ';
                }

                Debug.Log(thisStr + " - adding " + ret.Length);
                return ret;
            }

            private string ParseData(CovidParser.Country.Data data, CovidParser.Country.Data previousData, CovidParser.Country.Data worldData, string country)
            {
                string countryName = (char)(country[0]-32) + country.Remove(0, 1);

                string confirmed = ToStringWithDots(data.confirmed);
                string deaths = ToStringWithDots(data.deaths);
                string recovered = ToStringWithDots(data.recovered);

                string confirmedPW = MathF.Round(data.confirmed * 100f / worldData.confirmed, 2).ToString();
                string deathsPW = MathF.Round(data.deaths * 100f / worldData.deaths, 2).ToString();
                string recoveredPW = MathF.Round(data.recovered * 100f / worldData.recovered, 2).ToString();

                string confirmedA = ToStringWithDots(data.confirmed - previousData.confirmed);
                string deathsA = ToStringWithDots(data.deaths - previousData.deaths);
                string recoveredA = ToStringWithDots(data.recovered - previousData.recovered);

                string deathsP = MathF.Round(data.deaths * 100f / data.confirmed, 2).ToString();
                string recoveredP = MathF.Round(data.recovered * 100f / data.confirmed, 2).ToString();

                return "\n" + countryName + " on " + data.date +
                    ":\n\t         \t\t\t\t[Cases]\t\t\t[World %]\t\t\t[Increase]\t\t\t[Cases %]" +
                    "\n\tConfirmed:\t" + confirmed + "\t\t\t\t" + GenerateSpaces(confirmed, new string[] { deaths, recovered })  + confirmedPW + "%" + GenerateSpaces(confirmedPW, new string[] { deathsPW, recoveredPW }) + "\t\t\t\t+" + confirmedA +
                    "\n\tDeaths:  \t\t" + deaths + "\t\t\t\t" + GenerateSpaces(deaths, new string[] { confirmed, recovered }) + deathsPW + "%" + GenerateSpaces(deathsPW, new string[] { confirmedPW, recoveredPW }) + "\t\t\t\t+" + deathsA + GenerateSpaces(deathsA, new string[] { confirmedA, recoveredA }) + "\t\t\t\t" + deathsP + "%" +
                    "\n\tRecovered:\t" + recovered + "\t\t\t\t" + GenerateSpaces(recovered, new string[] { deaths, confirmed }) + recoveredPW + "%" + GenerateSpaces(recoveredPW, new string[] { confirmedPW, deathsPW }) + "\t\t\t\t+" + recoveredA + GenerateSpaces(recoveredA, new string[] { confirmedA, deathsA }) + "\t\t\t\t" + recoveredP + "%";
            

                /*return "\n" + countryName + " on " + data.date +
                    ":\n\t         \t\t\t\t[Cases] [World %] [Increase] [cases %]" +
                    "\n\tConfirmed:\t" + confirmed + "\t\t\t\t" + confirmedPW + "%\t\t\t\t+" + confirmedA +
                    "\n\tDeaths:  \t\t" + deaths + "\t\t\t\t" + deathsPW + "%\t\t\t\t+" + deathsA + "\t\t\t\t" + deathsP + "%" +
                    "\n\tRecovered:\t" + recovered + "\t\t\t\t" + recoveredPW + "%\t\t\t\t+" + recoveredA + "\t\t\t\t" + recoveredP + "%";*/
            }

            public void Execute(List<CovidParser.Country> data, CovidParser.Country world)
            {
                CovidParser.Country _country = null;
                CovidParser.Country.Data countryData = new CovidParser.Country.Data();
                CovidParser.Country.Data prevCountryData = new CovidParser.Country.Data();

                CovidParser.Country[] cTc = new CovidParser.Country[countriesToCompare.Length];

                if (country == "world")
                {
                    _country = world;
                    countryData = GetWorldOnDate(world, date);
                    if (date == DateTime.MinValue)
                    {
                        if (world.data.Count > 1)
                        {
                            prevCountryData = world.data[world.data.Count - 2];
                        }
                    }
                    else
                    {
                        DateTime prevDate = date.AddDays(-1);
                        prevCountryData = GetWorldOnDate(world, prevDate);
                    }
                }
                else
                {

                    for (int i = 0; i < data.Count; i++)
                    {
                        if (data[i].name == country)
                        {
                            if (date == DateTime.MinValue)
                            {
                                _country = data[i];
                                countryData = data[i].data[data[i].data.Count - 1];

                                if (data[i].data.Count > 1)
                                {
                                    prevCountryData = data[i].data[data[i].data.Count - 2];
                                }
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

                                    prevCountryData = country;
                                }
                            }

                            //break;
                        }
                        for (int j = 0; j < countriesToCompare.Length; j++)
                        {
                            if (data[i].name == countriesToCompare[j])
                            {
                                if (date == DateTime.MinValue)
                                {
                                    cTc[j] = data[i];
                                }
                                else
                                {
                                    string toFind = DateToStr(date);
                                    foreach (CovidParser.Country.Data country in data[i].data)
                                    {
                                        if (country.date == toFind)
                                        {
                                            cTc[j] = data[i];

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if(_country == null)
                {
                    RespondToCommand("Country ``" + country + "`` not found", channel);
                    return;
                }

                if (param.info)
                {
                    RespondToCommand(ParseData(countryData, prevCountryData, GetWorldOnDate(world, date), country), channel); 
                }
                if(param.chart)
                {
                    CovidParser.Country[] countries = new CovidParser.Country[countriesToCompare.Length + 1];
                    countries[0] = _country;

                    for(int i = 0;i<cTc.Length;i++)
                    {
                        if(cTc[i] == null)
                        {
                            RespondToCommand("Cannot find country: " + countriesToCompare[i], channel);
                            Debug.LogError("Cannot find country: " + countriesToCompare[i]);
                            return;
                        }

                        countries[i + 1] = cTc[i];
                        
                    }
                    int start = 0;
                    int end = 2147483647;

                    if(startDate != DateTime.MinValue)
                    {
                        start = (startDate - StrToDate(world.data[0].date)).Days;
                        Debug.Log("Start: " + start);
                    }
                    if(date != DateTime.MinValue) // end date
                    {
                        end = (date - StrToDate(world.data[0].date)).Days;
                        Debug.Log("End: " + end);
                    }

                    CreateCovidChart(countries, CovidChartTypes.all, start, end);

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
                covidChannels[i].Execute(covidData, world);
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

            covidLastUpdate = DateTime.Now;

            covidData = CovidParser.Parse(data);
            Debug.Log("Covid data updated");
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

        private static DateTime StringToDate(string day, string month, string year)
        {
            try
            {
                DateTime t = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));

                return t;
            }
            catch(Exception e)
            {
                Debug.LogError("Invalid date format");
                return DateTime.MaxValue;
            }

            
        }

        public static DateTime StrToDate(string str, SocketMessage message = null)
        {
            string[] dateParams;
            if (str.Contains('-'))
            {
                dateParams = str.Split('-');
            }
            else if (str.Contains('.'))
            {
                dateParams = str.Split('.');
            }
            else
            {
                if (message != null)
                { 
                    RespondToCommand("Invalid date format - You have to use ``-`` or ``.`` to seperate date", message);
                }
                return DateTime.MaxValue;
            }
            if (dateParams.Length != 3)
            {
                if (message != null)
                {
                    RespondToCommand("Invalid date format - not enough information ( day / month / year )", message);
                }
                return DateTime.MaxValue;
            }

            string day, month, year;
            if (dateParams[0].Length > 2)
            {
                year = dateParams[0];
                month = dateParams[1];
                day = dateParams[2];
            }
            else if (dateParams[2].Length > 2)
            {
                day = dateParams[0];
                month = dateParams[1];
                year = dateParams[2];
            }
            else
            {
                if (message != null)
                {
                    RespondToCommand("Invalid date format - no year specified", message);
                }
                return DateTime.MaxValue;
            }

            DateTime date = StringToDate(day, month, year);
            if (date == DateTime.MaxValue)
            {
                if (message != null)
                {
                    RespondToCommand("Invalid date format - cannot parse to date", message);
                }
                return DateTime.MaxValue;
            }

            return date;
        }

        private void RequestCovidData(SocketMessage message, string parameters)
        {
            string[] msgParams = parameters.Split(' ');
            bool chart, info, infoSpecified;
            chart = info = infoSpecified = false;
            string country = "";
            DateTime date = DateTime.MinValue;
            DateTime startDate = DateTime.MinValue;

            int paramsOffset = 0;
            if (msgParams.Length == 0 || msgParams[0].Length == 0)
            {
                //RespondToCommand("No parameters specified, usage: " + prefix + "!covid <country> [date / last] [-chart / -info]", message);
                //return;
                country = "world";
            }
            else
            {
                country = msgParams[0];
                if(msgParams[0].StartsWith('\"'))
                {
                    country = country.Remove(0, 1) + ' ';

                    bool found = false;
                    for(int i = 1;i<msgParams.Length;i++)
                    {
                        country += msgParams[i] + ' ';

                        if(msgParams[i].EndsWith('\"'))
                        {
                            found = true;
                            paramsOffset = i + 1;

                            country = country.Substring(0, country.Length - 2); // Without " and ' '
                            Debug.Log("Country: \"" + country + "\"");
                            break;
                        }

                    }

                    if(!found)
                    {
                        RespondToCommand("Cannot find closing quotemark", message);
                        return;
                    }
                }
            }
            

            

            DateTime t = DateTime.Now;

            List<string> compareCountries = new List<string>();

            for(int i = paramsOffset;i<msgParams.Length;i++)
            {
                if(msgParams[i] == "-chart")
                {
                    chart = true;
                }
                else if (msgParams[i] == "-info")
                {
                    info = true;
                    infoSpecified = true;
                }
                else if(msgParams[i] == "-date" || msgParams[i] == "-endDate")
                {
                    i++;
                    if(i >= msgParams.Length || msgParams[i].StartsWith('-'))
                    {
                        RespondToCommand("You have to specify specific date", message);
                        return;
                    }

                    date = StrToDate(msgParams[i]);
                    if (date == DateTime.MaxValue) return;
                }
                else if(msgParams[i] == "-startDate")
                {
                    i++;
                    if (i >= msgParams.Length || msgParams[i].StartsWith('-'))
                    {
                        RespondToCommand("You have to specify specific date", message);
                        return;
                    }

                    startDate = StrToDate(msgParams[i]);
                    if (startDate == DateTime.MaxValue) return;
                }
                else if(msgParams[i].StartsWith("+"))
                {
                    //compareCountries.Add(msgParams[i].Remove(0, 1));
                    string addCountry = msgParams[i].Remove(0, 1);
                    if(msgParams[i].StartsWith("+\""))
                    {
                        addCountry = addCountry.Remove(0, 1) + ' ';
                        bool found = false;
                        i++;
                        for(;i<msgParams.Length;i++)
                        {
                            addCountry += msgParams[i] + ' ';
                            if(msgParams[i].EndsWith('\"'))
                            {
                                found = true;
                                addCountry = addCountry.Substring(0, addCountry.Length - 2);
                                break;
                            }
                        }
                        if(!found)
                        {
                            RespondToCommand("No closing quote mark found", message);
                            return;
                        }
                    }

                    compareCountries.Add(addCountry);

                    info = infoSpecified;
                    chart = true;
                }
            }

            for(int i = 0;i<compareCountries.Count;i++)
            {
                Debug.Log("Compare country: " + compareCountries[i]);
            }

            if(chart == false)
            {
                info = true;
            }

            covidChannels.Add(new CovidCommand(CovidCommand.Types.all, country, date, message, new CovidCommand.Parameters() { chart = chart, info = info }, startDate, compareCountries.ToArray()));
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

        public static CovidParser.Country.Data FindMax(CovidParser.Country[] countries, CovidChartTypes type, int startDay = 0, int endDay = 2147483647)
        {
            int maxPos = -1;
            int maxIndex = -1;
            long max = -1;
            for (int j = 0; j < countries.Length; j++)
            {
                if (countries[j] == null) continue;

                for (int i = startDay; i < countries[j].data.Count && i < endDay; i++)
                {
                    long c = 0;
                    switch (type)
                    {
                        case CovidChartTypes.deaths:
                            c = countries[j].data[i].deaths;
                            break;
                        case CovidChartTypes.confirmed:
                            c = countries[j].data[i].confirmed;
                            break;
                        case CovidChartTypes.recovered:
                            c = countries[j].data[i].recovered;
                            break;
                        case CovidChartTypes.all:
                            c = countries[j].data[i].confirmed;
                            break;
                        default:
                            break;
                    }

                    if (c > max)
                    {
                        maxPos = i;
                        maxIndex = j;

                        max = c;
                    }
                }
            }

            return countries[maxIndex].data[maxPos];
        }

        public static void CreateCovidChart(CovidParser.Country country, CovidChartTypes type)
        {
            CovidParser.Country[] countries = new CovidParser.Country[1];
            countries[0] = country;

            CreateCovidChart(countries, type);
        }

        public static void CreateCovidChart(CovidParser.Country[] countries, CovidChartTypes type, int startDay = 0, int endDay = 2147483647)
        {
            if(startDay < 0)
            {
                Debug.LogWarning("Start day is lower than 0, fixing");
                startDay = 0;
            }

            int xMin = startDay;
            int xMax = countries[0].data.Count;
            int yMin = 0;
            CovidParser.Country.Data c = FindMax(countries, type, startDay, endDay);
            long yMax = 0;

            string title = "COVID-19 in ";
            for(int i = 0;i<countries.Length;i++)
            {
                title += countries[i].name + " ";
            }
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
            for (int j = 0; j < countries.Length; j++)
            {
                for (int i = startDay; i < countries[j].data.Count && i < endDay; i++)
                {
                    if (j == 0)
                    {
                        keys.Add(i);
                    }
                    switch (type)
                    {
                        case CovidChartTypes.deaths:
                            if (values == null || values[j] == null)
                            {
                                values = new List<double>[countries.Length];
                                values[j] = new List<double>();
                            }
                            values[j].Add(countries[j].data[i].deaths);

                            break;
                        case CovidChartTypes.confirmed:
                            if (values == null || values[j] == null)
                            {
                                values = new List<double>[countries.Length];
                                values[j] = new List<double>();
                            }
                            values[j].Add(countries[j].data[i].confirmed);

                            break;
                        case CovidChartTypes.recovered:
                            if (values == null || values[j] == null)
                            {
                                values = new List<double>[countries.Length];
                                values[j] = new List<double>();
                            }
                            values[j].Add(countries[j].data[i].recovered);

                            break;
                        case CovidChartTypes.all:
                            //Debug.LogError("All is not supported yet");
                            if (values == null)
                            {
                                values = new List<double>[countries.Length * 3];
                            }
                            if(values[j * 3] == null)
                            { 
                                values[j*3] = new List<double>();
                                values[j*3 + 1] = new List<double>();
                                values[j*3 + 2] = new List<double>();
                            }
                            values[j*3].Add(countries[j].data[i].confirmed);
                            values[j*3+1].Add(countries[j].data[i].deaths);
                            values[j*3+2].Add(countries[j].data[i].recovered);
                            break;
                        default:
                            break;
                    }
                }

            }
            var pl = new PLStream();

            pl.sdev("pngcairo");
            pl.sfnam("covid.png");

            pl.init();
            pl.col0(15);

            // Set to use 10000 instead of 1 * 10^5
            pl.syax(10, 10);

            if (endDay < xMax)
            {
                xMax = endDay - 1;
            }
            

            pl.env(xMin, xMax, yMin, yMax, AxesScale.Independent, AxisBox.BoxTicksLabels);

            //pl.setcontlabelformat(10, 10);
            pl.col0(15);
            pl.lab("Days", "Cases",  title);

            



            Pattern[] lg_patterns = new Pattern[values.Length];
            double[] lg_scales = new double[values.Length];
            string[] lg_texts = new string[values.Length];
            int[] lg_lcolors = new int[values.Length];
            double[] lg_lwidths = new double[values.Length];
            int[] lg_scolors = new int[values.Length];
            int[] lg_snumbers = new int[values.Length];
            string[] lg_symbols = new string[values.Length];
            LineStyle[] lg_lstyles = new LineStyle[values.Length];
            LegendEntry[] lg_entries = new LegendEntry[values.Length];

            double lg_spacing = 2.8 / (countries.Length + 0.5);
            double lg_tscale = 1.4 / (countries.Length + 0.5);
            double lg_toffset = 0.5;



            switch (type)
            {
                case CovidChartTypes.deaths:
                    pl.scmap0(new int[] { 255, 255 }, new int[] { 255, 0 }, new int[] { 0, 0 });
                    break;
                case CovidChartTypes.confirmed:
                    break;
                case CovidChartTypes.recovered:
                    break;
                case CovidChartTypes.all:
                    pl.scmap0(new int[] { 255, 255, 0, 204, 128, 0}, new int[] { 255, 0, 255, 153, 0, 128 }, new int[] { 0, 0, 0, 0, 0, 0 });
                    break;
                default:
                    break;
            }

            for (int i = 0; i < values.Length; i++)
            {
                
                pl.col0(i);

                pl.line(keys.ToArray(), values[i].ToArray());


                lg_entries[i] = LegendEntry.Line;
                lg_lstyles[i] = LineStyle.Continuous;
                lg_lcolors[i] = i;
                lg_scolors[i] = i;
                lg_snumbers[i] = i;
                lg_lwidths[i] = 2;
                lg_texts[i] = countries[i / 3].name;
                if(i%3 == 0)
                {
                    lg_texts[i] += " confirmed";
                }
                else if(i%3 == 1)
                {
                    lg_texts[i] += " deaths";
                }
                else
                {
                    lg_texts[i] += " recovered";
                }

                lg_scales[i] = 1;
            }

            

            pl.legend(out double width, out double height, Legend.BoundingBox, Position.Left | Position.Top, 0, 0, 0.1, 0, 1, LineStyle.Continuous, 0, 0, lg_entries, lg_toffset, lg_tscale, lg_spacing, 1.0,
                lg_lcolors, lg_texts, lg_lcolors, lg_patterns, lg_scales, lg_lwidths, lg_lcolors, lg_lstyles, lg_lwidths, lg_scolors, lg_scales, lg_snumbers, lg_symbols);

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

                /*case "weather":
                    RespondToCommand("To be done :)", message);
                    break;*/

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
            else
            {
                UpdateCovidData();
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
