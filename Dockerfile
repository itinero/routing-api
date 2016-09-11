FROM microsoft/dotnet:latest

ARG routerdb

RUN wget $routerdb

COPY . /app

WORKDIR /app

RUN ["dotnet", "restore"]

RUN cd ./src/Itinero.API/ ; dotnet build

EXPOSE 5000/tcp

ENTRYPOINT ["dotnet", "run", "--server.urls", "http://0.0.0.0:5000"]
