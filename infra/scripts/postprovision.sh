#!/bin/bash

# Post-provision script to configure apphost.settings.json
# This script reads environment variables from azd and populates the apphost.settings.json file

set -e

echo -e "\033[0;32mConfiguring apphost.settings.json...\033[0m"

# Get the root directory (two levels up from the script)
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
SETTINGS_FILE="$ROOT_DIR/apphost.settings.json"
TEMPLATE_FILE="$ROOT_DIR/apphost.settings.template.json"

# Check if settings file exists, if not, copy from template
if [ ! -f "$SETTINGS_FILE" ]; then
    echo -e "\033[0;33mapphost.settings.json not found. Copying from template...\033[0m"
    if [ -f "$TEMPLATE_FILE" ]; then
        cp "$TEMPLATE_FILE" "$SETTINGS_FILE"
        echo -e "\033[0;32mTemplate copied successfully.\033[0m"
    else
        echo -e "\033[0;31mError: Template file not found at $TEMPLATE_FILE\033[0m"
        exit 1
    fi
fi

# Read environment variables from azd
eval "$(azd env get-values)"

COSMOS_NAME="${AZURE_COSMOS_NAME:-}"
COSMOS_RESOURCE_GROUP="${AZURE_COSMOS_RESOURCE_GROUP:-}"

# Validate required variables

if [ -z "$COSMOS_NAME" ]; then
    echo -e "\033[0;33mWarning: AZURE_COSMOS_NAME environment variable is not set\033[0m"
    COSMOS_NAME=""
fi

if [ -z "$COSMOS_RESOURCE_GROUP" ]; then
    echo -e "\033[0;33mWarning: AZURE_COSMOS_RESOURCE_GROUP environment variable is not set\033[0m"
    COSMOS_RESOURCE_GROUP=""
fi

# Update the settings file using jq
if command -v jq &> /dev/null; then
    # Use jq if available for proper JSON manipulation
    jq --arg --arg cosmosName "$COSMOS_NAME" --arg cosmosRg "$COSMOS_RESOURCE_GROUP" \
        '.Parameters.cosmosName = $cosmosName | .Parameters.cosmosResourceGroup = $cosmosRg' \
        "$SETTINGS_FILE" > "$SETTINGS_FILE.tmp" && mv "$SETTINGS_FILE.tmp" "$SETTINGS_FILE"
else
    # Fallback to sed if jq is not available (less robust but works for simple cases)
    sed -i.bak "s|\"cosmosName\".*:.*\".*\"|\"cosmosName\": \"$COSMOS_NAME\"|g" "$SETTINGS_FILE"
    sed -i.bak "s|\"cosmosResourceGroup\".*:.*\".*\"|\"cosmosResourceGroup\": \"$COSMOS_RESOURCE_GROUP\"|g" "$SETTINGS_FILE"
    rm -f "$SETTINGS_FILE.bak"
fi

echo -e "\033[0;32mapphost.settings.json configured successfully!\033[0m"
echo -e "\033[0;36m  - OpenAI Endpoint: $OPENAI_ENDPOINT\033[0m"
echo -e "\033[0;36m  - OpenAI Deployment: $OPENAI_DEPLOYMENT\033[0m"
echo -e "\033[0;36m  - Image Model Deployment: $IMAGE_MODEL_DEPLOYMENT\033[0m"
echo -e "\033[0;36m  - Cosmos Name: $COSMOS_NAME\033[0m"
echo -e "\033[0;36m  - Cosmos Resource Group: $COSMOS_RESOURCE_GROUP\033[0m"
