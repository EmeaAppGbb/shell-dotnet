#!/bin/bash
set -e

# Write Vite backend URL for production builds so the SPA calls the real backend.
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
FRONTEND_DIR="$ROOT_DIR/src/frontend"
ENV_FILE="$FRONTEND_DIR/.env.production"

# Load azd environment values (brings BACKEND_URL into scope).
eval "$(azd env get-values)"

if [ -z "$BACKEND_URL" ]; then
  echo "BACKEND_URL is not set in the azd environment; cannot configure frontend API base" >&2
  exit 1
fi

echo "VITE_BACKEND_URL=$BACKEND_URL" > "$ENV_FILE"
echo "Wrote backend URL to $ENV_FILE"
