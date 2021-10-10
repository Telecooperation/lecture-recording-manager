# TK Lecture Recording Manager

This repository contains the source code of the TK Lecture Manager for managing video recordings. It contains the backend (the processing engine for the green screen, zoom meeting, and regular recordings) and the front end (manage publishing dates, titles, descriptions, etc.).

## Features
* Backend engine based on FFMpeg to transcode green screen recordings
* Job management for transcoding
* User interface to manage publishing, title, description
* User accounts

## Requirements
* PostgreSQL Database for storing the recording meta data
* dotnet 5.0 SDK (https://dotnet.microsoft.com/) for compiling
* (only on unix) ```tesseract-ocr``` package

## Build project
```
dotnet restore
dotnet build --configuration Release --no-restore
```

## Configuration and Setup
Modify the ```appsettings.json``` and adjust database connection, ```UploadVideoPath``` (directory for storing the uploaded videos), and ```PublishVideoPath``` (directory for storing the published videos).

Don't forget to set correct folder paths for each lecture (source, publish, convert).

## Use Docker
Instead of running the application directly, there is also a docker-compose file to run the entire platform using:
```console
docker-compose up
```

The compose file will generate corresponding folders under `/docker` and will also persist the database and all other files there.

To create the first user, you'll need to call the user registration API manually:
```console
curl --header "Content-Type: application/json" \
--request POST --data '
{
	"username": "testuser",
	"email": "test@testurl.com",
	"password": "test12345!"
}' \
http://localhost:8080/api/user/register
```
The generated user is locked out by default, so it must be activated manually via the corresponding database entry.

First, connect to yout postgres installation:
`psql -U postgres -W`
If you are using docker, the easiest way to get there is opening a shell within the container:
`docker exec -ti lecturedatabase /bin/bash`

Select the database:
`\connect lecture_manager`

And unlock your account:
`UPDATE "AspNetUsers" SET "LockoutEnabled" = false WHERE "UserName" = 'testuser';`

Now you should be able to login.

Finally, you can also adjust the folders used in the docker container by adjusting the docker-compose file.
