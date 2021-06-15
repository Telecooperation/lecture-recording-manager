FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env

# Install NodeJs
RUN apt-get update && \
	apt-get install -y wget && \
	apt-get install -y gnupg2 && \
	wget -qO- https://deb.nodesource.com/setup_14.x | bash - && \
	apt-get install -y build-essential nodejs

WORKDIR /app

# copy files to docker
COPY ./wait-for-it.sh ./wait-for-it.sh
COPY ./lecture-recording-manager/ ./lecture-recording-manager/
COPY ./lecture-recording-processor/ ./lecture-recording-processor/
COPY ./lecture-recording-manager.sln ./lecture-recording-manager.sln

RUN dotnet restore

RUN dotnet publish -c Release -o out

# build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0

# install tesseract-ocr
RUN apt-get update && \
	apt-get install -y tesseract-ocr tesseract-ocr-eng

# create workdir
WORKDIR /app

COPY --from=build-env /app/out .
COPY --from=build-env /app/wait-for-it.sh ./wait-for-it.sh

RUN chmod +x ./wait-for-it.sh

#ENTRYPOINT ["/bin/bash", "-c"]
ENTRYPOINT ["/bin/sh", "-c", "./wait-for-it.sh lecturedatabase:5432 -t 120 -- dotnet lecture-recording-manager.dll"]