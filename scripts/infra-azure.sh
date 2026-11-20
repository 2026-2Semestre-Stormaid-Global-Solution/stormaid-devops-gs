#!/usr/bin/env bash
# Storm Aid - provisionamento idempotente básico via Azure CLI.
# Execute no Azure Cloud Shell (Bash) ou em ambiente com Azure CLI autenticada.

set -euo pipefail

: "${RESOURCE_GROUP:rg-stormaid-gs}"
: "${LOCATION:canadacentral}"
: "${APP_SERVICE_PLAN:plan-stormaid-gs}"
: "${WEB_APP_NAME:bsl-stormaid-gs}"
: "${ORACLE_CONNECTION_STRING:secret}"
: "${JWT_SECRET:secret}"

PLAN_SKU="${PLAN_SKU:-F1}"
RUNTIME="${RUNTIME:-dotnet:10}"

echo "Assinatura Azure selecionada:"
az account show --output table

# Resource Group
if az group show --name "$RESOURCE_GROUP" >/dev/null 2>&1; then
  echo "Resource Group já existe: $RESOURCE_GROUP"
else
  az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output table
fi

# App Service Plan Linux
if az appservice plan show --name "$APP_SERVICE_PLAN" --resource-group "$RESOURCE_GROUP" >/dev/null 2>&1; then
  echo "App Service Plan já existe: $APP_SERVICE_PLAN"
else
  az appservice plan create \
    --name "$APP_SERVICE_PLAN" \
    --resource-group "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --sku "$PLAN_SKU" \
    --is-linux \
    --output table
fi

# Web App
if az webapp show --name "$WEB_APP_NAME" --resource-group "$RESOURCE_GROUP" >/dev/null 2>&1; then
  echo "Web App já existe: $WEB_APP_NAME"
else
  az webapp create \
    --name "$WEB_APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --plan "$APP_SERVICE_PLAN" \
    --runtime "$RUNTIME" \
    --output table
fi

# Configura stack/runtime e App Settings. Segredos são recebidos por ambiente.
az webapp config set \
  --name "$WEB_APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --linux-fx-version "DOTNETCORE|10.0" \
  --output table

az webapp config appsettings set \
  --name "$WEB_APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__Oracle="$ORACLE_CONNECTION_STRING" \
    Jwt__Secret="$JWT_SECRET" \
    OpenApi__Enabled=true \
    RabbitMQ__Enabled=false \
    OutboxPublisher__Enabled=false \
  --output table

echo "Provisionamento/configuração concluídos."
echo "URL: https://${WEB_APP_NAME}.azurewebsites.net"
