using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DiscordMinikBOT
{
    public static class CovidParser
    {
        public class Country
        {
            public string name;
            //private List<Data> _data;
            public List<Data> data = new List<Data>();
            public struct Data
            {
                public string date;
                public long confirmed;
                public long deaths;
                public long recovered;
            }
            
        }

        public static List<Country> Parse(string data)
        {
            List<Country> countries = new List<Country>();

            Country world = new Country();
            world.name = "world";
            countries.Add(world);

            DataSet set = JsonConvert.DeserializeObject<DataSet>(data);
            for(int i = 0;i<set.Tables.Count;i++)
            { 
               
                Country c = new Country();
                
                c.name = set.Tables[i].TableName.ToLower();

                int date = 0;
                foreach (DataRow row in set.Tables[i].Rows)
                {
                    Country.Data dt = new Country.Data();
                    dt.date = row["date"].ToString();
                    dt.confirmed = (long)row["confirmed"];
                    dt.deaths = (long)row["deaths"];
                    dt.recovered = (long)row["recovered"];

                    if (i == 0)
                    {
                        world.data.Add(dt);
                    }
                    else
                    {
                        Country.Data wDt = world.data[date];
                        wDt.confirmed += dt.confirmed;
                        wDt.deaths += dt.deaths;
                        wDt.recovered += dt.recovered;
                        world.data[date] = wDt;
                    }

                    c.data.Add(dt);

                    date++;
                }
                

                countries.Add(c);
            }

           

            return countries;
        }
    }
}
