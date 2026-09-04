FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY HackerNews.BestStories.sln ./
COPY src/HackerNews.BestStories.Api/HackerNews.BestStories.Api.csproj src/HackerNews.BestStories.Api/
COPY tests/HackerNews.BestStories.Tests/HackerNews.BestStories.Tests.csproj tests/HackerNews.BestStories.Tests/
RUN dotnet restore HackerNews.BestStories.sln

COPY . .
RUN dotnet publish src/HackerNews.BestStories.Api/HackerNews.BestStories.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

USER $APP_UID
ENTRYPOINT ["dotnet", "HackerNews.BestStories.Api.dll"]
