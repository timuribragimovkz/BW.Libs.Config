#!/bin/bash
# Publish script for BW.Libs packages to AWS CodeArtifact
# Reads source URL from nuget.config automatically

set -e

CONFIGURATION="${1:-Release}"
SOURCE_NAME="${2:-bruceware-libs}"
AWS_PROFILE="${3:-bruceware}"

echo "🔐 Getting CodeArtifact token (using profile: $AWS_PROFILE)..."
TOKEN=$(aws codeartifact get-authorization-token \
    --profile "$AWS_PROFILE" \
    --domain bruceware \
    --domain-owner 560719246675 \
    --region eu-central-1 \
    --query authorizationToken \
    --output text)

if [ -z "$TOKEN" ]; then
    echo "❌ Failed to get CodeArtifact token. Is AWS CLI configured?"
    exit 1
fi

echo "✅ Token retrieved"

echo "🔧 Updating NuGet source credentials..."
dotnet nuget update source "$SOURCE_NAME" \
    --username aws \
    --password "$TOKEN" \
    --store-password-in-clear-text

echo "✅ Credentials updated"

echo "🔨 Building $CONFIGURATION configuration..."
dotnet build --configuration "$CONFIGURATION"

echo "✅ Build succeeded"

echo "📦 Pushing packages to $SOURCE_NAME..."

# Find all .nupkg files in bin/$CONFIGURATION (exclude symbols)
find . -type f -path "*/bin/$CONFIGURATION/*.nupkg" ! -name "*.symbols.nupkg" | while read -r package; do
    echo "  → $(basename "$package")"
    dotnet nuget push "$package" --source "$SOURCE_NAME" --api-key aws --skip-duplicate
done

echo "✅ All packages published successfully!"
