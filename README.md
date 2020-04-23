# CovidBot
CovidBot lets you check how many there are confirmed cases, deaths and recovered cases in the world or in specific country

Configuring:
- Enter your bot token into config.ini after "token = " or in Program.cs 

Usage:
- m!<command> - type m!help for info
- Covid-19 data: m!covid [country] [-date (date)] [-chart] [-info]
    -date: Use this parameter to check cases on specific date
    -chart: Creates chart on specific country, won't display info if -info is not specified
    -info: If specified always displays info ( even when -chart is applied )
