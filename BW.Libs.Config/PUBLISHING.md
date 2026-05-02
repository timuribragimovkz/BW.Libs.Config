# Publishing Guide

This document explains how to publish BW.Libs packages to AWS CodeArtifact.

## Prerequisites

### AWS Profile Setup (One-Time)

You need an AWS profile named `bruceware` configured with CodeArtifact access.

#### Step 1: Create IAM User in AWS Console

1. Log into your **BruceWare AWS account**: https://console.aws.amazon.com/
2. Navigate to **IAM** → **Users** → **Create user**
3. User name: `codeartifact-publisher`
4. **DON'T** check "Provide user access to AWS Management Console"
5. Click **Next**
6. **Attach policies directly**: Select `AWSCodeArtifactAdminAccess`
7. Click **Next** → **Create user**

#### Step 2: Create Access Keys

1. Click on the user → **Security credentials** tab
2. Scroll to **Access keys** → **Create access key**
3. Use case: **Command Line Interface (CLI)**
4. Click **Next** → **Create access key**
5. **Copy** the Access Key ID and Secret Access Key

#### Step 3: Configure AWS Profile

Edit `~/.aws/credentials`:

```bash
nano ~/.aws/credentials
```

Add your BruceWare credentials:

```ini
[bruceware]
aws_access_key_id=YOUR_ACCESS_KEY_HERE
aws_secret_access_key=YOUR_SECRET_KEY_HERE
```

Edit `~/.aws/config`:

```bash
nano ~/.aws/config
```

Add region:

```ini
[profile bruceware]
region = eu-central-1
output = json
```

#### Step 4: Verify

```bash
aws sts get-caller-identity --profile bruceware
```

Should show your BruceWare account ID: `560719246675`

---

## Quick Publish

### PowerShell (Windows/macOS/Linux)
```powershell
./publish.ps1
```

### Bash (macOS/Linux)
```bash
./publish.sh
```

Both scripts automatically:
1. Get AWS CodeArtifact token using `bruceware` profile
2. Update NuGet source credentials
3. Build in Release configuration
4. Push all `.nupkg` files to CodeArtifact

---

## Custom Configuration

### Use Different AWS Profile
```bash
# Bash
./publish.sh Release bruceware-libs my-other-profile

# PowerShell
./publish.ps1 -Configuration Release -SourceName bruceware-libs -AwsProfile my-other-profile
```

### Publish Debug Build
```bash
./publish.sh Debug
```

---

## Manual Workflow

If you prefer manual control:

### 1. Get Token
```bash
export CODEARTIFACT_AUTH_TOKEN=$(aws codeartifact get-authorization-token \
  --profile bruceware \
  --domain bruceware \
  --domain-owner 560719246675 \
  --region eu-central-1 \
  --query authorizationToken \
  --output text)
```

### 2. Update Credentials
```bash
dotnet nuget update source bruceware-libs \
  --username aws \
  --password $CODEARTIFACT_AUTH_TOKEN \
  --store-password-in-clear-text
```

### 3. Build
```bash
dotnet build --configuration Release
```

### 4. Push
```bash
dotnet nuget push "bin/Release/*.nupkg" \
  --source bruceware-libs \
  --api-key aws \
  --skip-duplicate
```

---

## How It Works

The publish scripts read the CodeArtifact URL from `nuget.config`:

```xml
<add key="bruceware-libs" value="https://bruceware-560719246675.d.codeartifact.eu-central-1.amazonaws.com/nuget/bruceware-libs/v3/index.json" />
```

By passing `--source bruceware-libs`, dotnet automatically resolves the URL from config. No hardcoding needed!

The scripts use AWS profiles, so you can separate work and personal credentials:
- Default profile (or named profile): Your work account
- `[bruceware]` profile: Your personal BruceWare account for publishing

---

## Troubleshooting

### "Failed to get CodeArtifact token"
```bash
# Check AWS profile is configured
aws sts get-caller-identity --profile bruceware

# If error, reconfigure credentials
nano ~/.aws/credentials
```

### "ExpiredTokenException"
Your AWS credentials expired. If using temporary credentials (session tokens), refresh them.

For permanent IAM user keys, this shouldn't happen.

### "Package already exists"
Bump version first using Versionize:
```bash
versionize
./publish.sh
```

### "No packages found"
- Ensure `GeneratePackageOnBuild` is `true` in `.csproj`
- Check that build succeeded
- Verify files exist: `ls -la bin/Release/*.nupkg`

### "NuGet.Config is not valid XML"
Your `nuget.config` has syntax errors. Make sure XML comments don't contain `--` (double dash).

---

## Multiple Developers

Each developer needs:
1. Their own IAM user in the BruceWare AWS account
2. Access keys configured in `~/.aws/credentials` under `[bruceware]`
3. To run `./publish.sh` (handles authentication automatically)

**Never share AWS credentials!** Each person gets their own keys.

---

## CI/CD Integration

For GitHub Actions:

```yaml
- name: Configure AWS
  uses: aws-actions/configure-aws-credentials@v2
  with:
    role-to-assume: arn:aws:iam::560719246675:role/GitHubActions
    aws-region: eu-central-1

- name: Publish
  run: ./publish.sh
```

---

## Security Best Practices

✅ **DO:**
- Use AWS profiles to separate work/personal credentials
- Store credentials in `~/.aws/credentials` (not in code)
- Use IAM user keys (not root account)
- Rotate keys periodically

❌ **DON'T:**
- Commit credentials to git
- Hardcode URLs or tokens in scripts
- Share credentials between developers
- Use root account credentials
