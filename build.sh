#!/usr/bin/env bash

set -e

dotnet build
dotnet pack -o ./out
