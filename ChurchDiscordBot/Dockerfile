#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ChurchDiscordBot.csproj", "ChurchDiscordBot/"]
WORKDIR "/src/ChurchDiscordBot"
RUN dotnet restore "ChurchDiscordBot.csproj"
COPY . .
WORKDIR "/src/ChurchDiscordBot"
RUN dotnet build "ChurchDiscordBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChurchDiscordBot.csproj" -c Release -o /app/publish

FROM base AS final

#####################
#PUPPETEER RECIPE
#####################
# Install latest chrome dev package and fonts to support major charsets (Chinese, Japanese, Arabic, Hebrew, Thai and a few others)
# Note: this installs the necessary libs to make the bundled version of Chromium that Puppeteer
# installs, work.
RUN apt-get install
RUN apt-get update && apt-get -f install && apt-get -y install wget gnupg2 apt-utils
RUN wget -q -O - https://dl-ssl.google.com/l... | apt-key add - \
&& echo "deb http://dl.google.com/linux/... stable main" >> /etc/apt/sources.list.d/google.list
RUN apt-get update && apt-get -f install -y google-chrome-stable fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst fonts-freefont-ttf

# Add user, so we don't need --no-sandbox.
# same layer as npm install to keep re-chowned files from using up several hundred MBs more space    
RUN groupadd -r pptruser && useradd -r -g pptruser -G audio,video pptruser \
    && mkdir -p /home/pptruser/Downloads \
    && chown -R pptruser:pptruser /home/pptruser

# Run everything after as non-privileged user.
USER pptruser
#####################
#END PUPPETEER RECIPE
#####################

ENV PUPPETEER_EXECUTABLE_PATH "/usr/bin/google-chrome-unstable"
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChurchDiscordBot.dll"]
