FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

# docker build -f TestWebApp/Dockerfile -t yrudakova . 
# docker run -p 4200:80 --name test-web-app <image>
# http://localhost:4200/home

FROM microsoft/dotnet:2.1-sdk AS publish
COPY ["TestWebApp/", "/src/TestWebApp/"]
WORKDIR "/src/TestWebApp"

RUN dotnet publish "TestWebApp.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app

COPY --from=publish /app .
COPY TestWebApp/run.sh run.sh

ENTRYPOINT ["sh", "-e", "run.sh"]