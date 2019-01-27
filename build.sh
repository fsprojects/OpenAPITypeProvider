#!/bin/sh

export OS=Unix

dotnet restore build.proj
dotnet fake build $@