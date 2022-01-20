#!/usr/bin/env bash

dotnet pack -p:TargetFrameworks=netstandard2.0 -c:Release AppMetrS2S/AppmetrS2S.csproj
dotnet nuget push AppmetrS2S/bin/Release/AppmetrS2S.*.nupkg -s https://nexus.pixapi.net/repository/nuget-hosted/ -k $NEXUS_NUGET_API_KEY