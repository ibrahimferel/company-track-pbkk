#!/usr/bin/env bash

set -euo pipefail

SA_PASSWORD="${SA_PASSWORD:-CompanyTrack#2026}"
APP_PASSWORD="${APP_PASSWORD:-CtApp#2026Pass}"
CONTAINER="${CONTAINER:-companytrack-sql}"
IMAGE="${IMAGE:-mcr.microsoft.com/mssql/server:2022-latest}"
HERE="$(cd "$(dirname "$0")" && pwd)"
SQLCMD="/opt/mssql-tools18/bin/sqlcmd"

PLATFORM_FLAG=""
if [ "$(uname -m)" = "arm64" ] || [ "$(uname -m)" = "aarch64" ]; then
  PLATFORM_FLAG="--platform linux/amd64"
fi

if ! docker ps -a --format '{{.Names}}' | grep -qx "$CONTAINER"; then
  echo ">> Membuat container $CONTAINER"
  docker run $PLATFORM_FLAG -d --name "$CONTAINER" \
    -e ACCEPT_EULA=Y -e "MSSQL_SA_PASSWORD=$SA_PASSWORD" \
    -p 1433:1433 "$IMAGE" >/dev/null
else
  echo ">> Menjalankan container $CONTAINER"
  docker start "$CONTAINER" >/dev/null
fi

ready=0
for _ in $(seq 1 60); do
  if docker exec "$CONTAINER" "$SQLCMD" -S localhost -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" >/dev/null 2>&1; then
    ready=1
    break
  fi
  sleep 2
done
if [ "$ready" -ne 1 ]; then
  echo "SQL Server belum siap. Cek log: docker logs $CONTAINER" >&2
  exit 1
fi

run_sql() {
  docker cp "$HERE/$1" "$CONTAINER:/tmp/$1"
  docker exec "$CONTAINER" "$SQLCMD" -S localhost -U sa -P "$SA_PASSWORD" -C -I -b -i "/tmp/$1" "${@:2}"
}

echo ">> Membuat skema"
run_sql 01_schema.sql
echo ">> Memasukkan data seed dummy"
run_sql 02_seed.sql
echo ">> Membuat akun aplikasi (hak minimum)"
run_sql 03_app_login.sql -v "APP_PASSWORD=$APP_PASSWORD"

cat <<EOF

Selesai. Connection string untuk aplikasi:
  Server=localhost,1433;Database=CompanyTrack_DB;User Id=ct_app;Password=$APP_PASSWORD;TrustServerCertificate=True;Encrypt=True

Tempel ke src/CompanyTrack/appsettings.json, atau set environment variable COMPANYTRACK_CONNECTION.
EOF