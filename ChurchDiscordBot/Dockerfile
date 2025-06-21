FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ChurchDiscordBot.csproj", "."]
RUN dotnet restore "ChurchDiscordBot.csproj"
COPY . .
RUN dotnet build "ChurchDiscordBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChurchDiscordBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final

# Puppeteer recipe
RUN apt-get update && apt-get install gnupg wget -y && \
  wget --quiet --output-document=- https://dl-ssl.google.com/linux/linux_signing_key.pub | gpg --dearmor > /etc/apt/trusted.gpg.d/google-archive.gpg && \
  sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list' && \
  apt-get update && \
  apt-get install google-chrome-stable -y --no-install-recommends && \
  rm -rf /var/lib/apt/lists/*

ENV PUPPETEER_SKIP_CHROMIUM_DOWNLOAD=true
ENV PUPPETEER_EXECUTABLE_PATH="/usr/bin/google-chrome"
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChurchDiscordBot.dll"]