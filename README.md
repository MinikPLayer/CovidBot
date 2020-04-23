# CovidBot
CovidBot lets you check how many there are confirmed cases, deaths and recovered cases in the world or in specific country

# Configuring:
- Enter your bot token into config.ini after "token = " or in Program.cs 

# Usage:
- m!< command > - type m!help for info
- Covid-19 data: m!covid [country] [+< country > / +< country > +< country2 >] [-date (date)] [-chart] [-info] 

# Covid parameters:
+ ( if country name have 2 or more words, use "< country >" )
- date: Use this parameter to check cases on specific date, formats available: yyyy-mm-dd, yyyy.mm.dd, dd-mm-yyyy, dd.mm.yyyy
- chart: Creates chart on specific country, won't display info if -info is not specified
- info: If specified always displays info ( even when -chart is applied )
+ country or +country1 +country2: Additional countries to the chart


# Covid examples:
m!covid US  - displays info about US ( most up-to-date data )
m!covid Germany -chart  - displays chart with data about Germany 
m!covid Italy +Spain -chart   - displays chart comparing data about Italy and Spain
m!covid China -date 20-03-2020    - displays info about China on 20 march 2020
m!covid "United Kingdom" -date 2020.03.20   - displays info about UK on 20 march 2020
