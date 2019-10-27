#!/bin/sh

set -e

until aws --region eu-west-1 --endpoint-url=http://localstack:4568 kinesis list-streams; do
  >&2 echo "Localstack Kinesis is unavailable - sleeping"
  sleep 1
done

>&2 echo "Localstack Kinesis is up - executing command"
aws --region eu-west-1 --endpoint-url=http://localstack:4568 kinesis create-stream --shard-count 1 --stream-name demo-stream
