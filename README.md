# TK Lecture Recording Manager

This repository contains the source code of the TK Lecture Manager for managing video recordings. It contains the backend (the processing engine for the green screen, zoom meeting, and regular recordings) and the front end (manage publishing dates, titles, descriptions, etc.).

The project is build with .NET core and can run cross-platform on Windows and Linux.
Additionally, the system runs within a docker container for easier maintainance (see below).

## Features

* Backend API for uploading and managing lecture recordings of different types
* Backend engine based on FFMpeg to transcode green screen recordings, screenshare recordings, talking head recordings
* Job management for automatic transcoding of recordings
* Automatic pickup of recordings from local directories
* User interface to manage publishing, title, description
* User account management for authentication
* Semester management
* Scheduled publishing by date and time

## Requirements

* PostgreSQL Database for storing the recording meta data
* dotnet 6.0 SDK (https://dotnet.microsoft.com/) for compiling
* (only on unix) ```tesseract-ocr``` package

## Development

### Setup

1. Install the .NET 6.0 SDK.

2. Clone the git repository:

```console
git clone git@github.com:Telecooperation/lecture-recording-manager.git
```

3. Build the project by running the following commands:

```console
dotnet restore
dotnet build --configuration Release --no-restore
```

4. Adjust the directory configurations and database connections in ```appsettings.json```.

```json
...
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=lecture_manager;Username=postgres;Password=test123"
  },
  "UploadVideoPath": "...\\videos",
  "PublishVideoPath": "...\\videos\\target",
...
```

5. Run the lecture-manager via:

```console
dotnet run --project .\lecture-recording-manager\lecture-recording-manager.csproj
```

## Docker

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
