#!/bin/sh
set -eu

mkdir -p /data
chown -R app:app /data

exec gosu app dotnet LifeSwap.Api.dll