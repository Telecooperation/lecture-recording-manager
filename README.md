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