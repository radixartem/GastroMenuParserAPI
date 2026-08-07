#!/bin/bash
set -e

if [ -z "$1" ]; then
  echo "Usage: $0 <backup-file.sql.gz>"
  exit 1
fi

BACKUP_FILE=$1
gunzip -c $BACKUP_FILE | docker compose -f /opt/gastro-api/docker-compose.yml exec -T postgres psql -U postgres gastro