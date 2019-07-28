using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordMinikBOT
{
    public static class Help
    {
        public static string message = @" 
**help** - Displays this page
**aliases** - Displays aliases list

**whoami {mention}** - Shows info about You / mentioned user



Bot prefix: **" + Program.prefix + @"**

Bot author: 
    - <@308705467209875456>";

        public static string aliasesMessage = @"
whois {mention} = whoami {mention}
";
    }
}
