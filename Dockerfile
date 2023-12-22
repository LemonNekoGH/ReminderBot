FROM mcr.microsoft.com/dotnet/sdk:8.0 as builder

ARG VERSION=0.1.0
ARG PLATFORM=linux-x64

WORKDIR /app

COPY LemonNeko.ReminderBot.Bot/LemonNeko.ReminderBot.Bot.csproj LemonNeko.ReminderBot.Bot.csproj

RUN dotnet restore

COPY LemonNeko.ReminderBot.Bot .

RUN dotnet publish -c Release -r ${PLATFORM}

FROM ubuntu as runner

RUN apt update && apt install libicu-dev ca-certificates -y && rm -rf /var/lib/apt/lists/*

ARG PLATFORM=linux-x64

WORKDIR /app

COPY --from=builder /app/bin/$PLATFORM/Release/net8.0/$PLATFORM/publish/LemonNeko.ReminderBot.Bot /app/LemonNeko.ReminderBot.Bot

ENTRYPOINT [ "/app/LemonNeko.ReminderBot.Bot" ]
