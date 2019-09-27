FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

# docker build -f TestOidcIdp/Dockerfile -t yrudakova-oidc . 
# docker run -p 4201:80 --name yrudakova-oidc <image>
# http://localhost:4201/home

# docker tag yrudakova-oidc:latest 729069874454.dkr.ecr.us-east-1.amazonaws.com/yrudakova-oidc:latest
# docker push 729069874454.dkr.ecr.us-east-1.amazonaws.com/yrudakova-oidc:latest

# AKIAJNJMCPEXZBIHDJ3A
# 2RNe/+C+OOpAOvTAPZA1EXYSOI+iPSbZlotOnBO+

FROM microsoft/dotnet:2.1-sdk AS publish
COPY ["TestOidcIdp/", "/src/TestOidcIdp/"]
WORKDIR "/src/TestOidcIdp"

RUN dotnet publish "TestOidcIdp.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app

COPY --from=publish /app .
COPY TestOidcIdp/run.sh run.sh

ENTRYPOINT ["sh", "-e", "run.sh"]