FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

# docker build -f TestOidcIdp/Dockerfile -t yrudakova-oidc . 
# docker run -p 4201:80 --name yrudakova-oidc <image>
# http://localhost:4201/home


FROM microsoft/dotnet:2.1-sdk AS publish
COPY ["TestOidcIdp/", "/src/TestOidcIdp/"]
WORKDIR "/src/TestOidcIdp"

RUN dotnet publish "TestOidcIdp.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app

COPY --from=publish /app .
COPY TestOidcIdp/run.sh run.sh

ENTRYPOINT ["sh", "-e", "run.sh"]