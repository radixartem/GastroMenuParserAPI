#!/bin/bash
set -e

TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/tmp/backup"
mkdir -p $BACKUP_DIR

# Дамп PostgreSQL
docker compose -f /opt/gastro-api/docker-compose.yml exec -T postgres pg_dump -U postgres gastro > $BACKUP_DIR/db_$TIMESTAMP.sql

# Сжатие
gzip $BACKUP_DIR/db_$TIMESTAMP.sql

# Загрузка в Object Storage (используем s3cmd или aws cli)
export AWS_ACCESS_KEY_ID=$OBJ_ACCESS_KEY
export AWS_SECRET_ACCESS_KEY=$OBJ_SECRET_KEY
aws s3 cp $BACKUP_DIR/db_$TIMESTAMP.sql.gz s3://your-backup-bucket/gastro-api/postgres/ --endpoint-url=https://your-fsn1.your-objectstorage.com

# Очистка локального временного каталога
rm -rf $BACKUP_DIR