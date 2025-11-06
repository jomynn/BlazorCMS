# BlazorCMS n8n Setup Guide

This guide will walk you through setting up n8n automation for BlazorCMS from scratch.

## Table of Contents

1. [Installation](#installation)
2. [First-Time Configuration](#first-time-configuration)
3. [Getting Your JWT Token](#getting-your-jwt-token)
4. [Importing Workflows](#importing-workflows)
5. [Service-Specific Setup](#service-specific-setup)
6. [Testing Your Setup](#testing-your-setup)
7. [Production Deployment](#production-deployment)

---

## Installation

### Option 1: Docker (Recommended)

**Prerequisites:** Docker and Docker Compose installed

1. Create a `docker-compose.yml` file:

```yaml
version: '3.8'

services:
  n8n:
    image: n8nio/n8n:latest
    container_name: n8n
    restart: always
    ports:
      - "5678:5678"
    environment:
      - N8N_BASIC_AUTH_ACTIVE=true
      - N8N_BASIC_AUTH_USER=admin
      - N8N_BASIC_AUTH_PASSWORD=change-this-password
      - N8N_HOST=localhost
      - N8N_PORT=5678
      - N8N_PROTOCOL=http
      - NODE_ENV=production
      - WEBHOOK_URL=http://localhost:5678/
    volumes:
      - n8n_data:/home/node/.n8n
      - ./n8n-examples/workflows:/workflows:ro

volumes:
  n8n_data:
```

2. Start n8n:

```bash
docker-compose up -d
```

3. Access n8n at `http://localhost:5678`

### Option 2: npm Installation

**Prerequisites:** Node.js 18+ installed

```bash
# Install n8n globally
npm install n8n -g

# Start n8n
n8n start

# Or run with custom settings
n8n start --tunnel
```

Access n8n at `http://localhost:5678`

### Option 3: npx (No Installation)

```bash
npx n8n
```

---

## First-Time Configuration

### 1. Initial Setup

1. Open `http://localhost:5678` in your browser
2. Create your n8n account (local instance):
   - Email: your-email@example.com
   - Password: Choose a strong password
3. Complete the welcome tour (optional)

### 2. Configure Environment Variables

**For Docker:** Add to your `docker-compose.yml`:

```yaml
environment:
  # BlazorCMS Configuration
  - BLAZORCMS_API_URL=http://host.docker.internal:7250
  - BLAZORCMS_PUBLIC_URL=http://localhost:5000
  - BLAZORCMS_JWT_TOKEN=
  - BLAZORCMS_FROM_EMAIL=noreply@blazorcms.com

  # Admin Configuration
  - ADMIN_EMAIL=admin@yourdomain.com
```

**For npm/npx:** Create `~/.n8n/.env`:

```env
# BlazorCMS Configuration
BLAZORCMS_API_URL=http://localhost:7250
BLAZORCMS_PUBLIC_URL=http://localhost:5000
BLAZORCMS_JWT_TOKEN=
BLAZORCMS_FROM_EMAIL=noreply@blazorcms.com

# Admin Configuration
ADMIN_EMAIL=admin@yourdomain.com

# Mailchimp (optional)
MAILCHIMP_LIST_ID=

# Google Services (optional)
GOOGLE_SHEETS_ID=
GOOGLE_DRIVE_BACKUP_FOLDER_ID=

# AWS S3 (optional)
AWS_S3_BUCKET_NAME=blazorcms-backups

# Azure Blob Storage (optional)
AZURE_CONTAINER_NAME=blazorcms-backups

# Slack (optional)
SLACK_ADMIN_CHANNEL=
```

---

## Getting Your JWT Token

You need a valid JWT token to authenticate n8n requests to BlazorCMS API.

### Method 1: Using API (Recommended)

```bash
# Login to get JWT token
curl -X POST http://localhost:7250/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@blazorcms.com",
    "password": "your-password"
  }'
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2025-01-15T12:00:00Z"
}
```

Copy the token value and add it to your environment variables.

### Method 2: Create Service Account

For long-term automation, create a dedicated service account:

```bash
# Register service account
curl -X POST http://localhost:7250/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "n8n-service@blazorcms.local",
    "password": "generate-secure-password",
    "fullName": "n8n Service Account"
  }'

# Login with service account
curl -X POST http://localhost:7250/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "n8n-service@blazorcms.local",
    "password": "your-service-account-password"
  }'
```

**Important:** JWT tokens expire after 2 hours. For production, you need to:
1. Extend token expiration in `BlazorCMS.API/appsettings.json`
2. Or implement token refresh workflow

### Extending Token Expiration

Edit `BlazorCMS.API/appsettings.json`:

```json
{
  "Jwt": {
    "Key": "your-secret-key",
    "Issuer": "BlazorCMS",
    "Audience": "BlazorCMS",
    "ExpirationHours": 720
  }
}
```

This extends token validity to 30 days (720 hours).

---

## Importing Workflows

### Import Individual Workflow

1. Click **"Workflows"** in the left sidebar
2. Click **"Add Workflow"** → **"Import from File"**
3. Select workflow JSON file from `n8n-examples/workflows/`
4. Click **"Import"**
5. Click **"Save"** to save the imported workflow

### Import All Workflows (Bash Script)

```bash
#!/bin/bash

# Set your n8n API credentials
N8N_URL="http://localhost:5678"
N8N_USER="admin"
N8N_PASSWORD="your-password"

# Directory containing workflow files
WORKFLOWS_DIR="./n8n-examples/workflows"

# Import each workflow
for file in $WORKFLOWS_DIR/*.json; do
  echo "Importing $(basename $file)..."

  curl -X POST "$N8N_URL/api/v1/workflows/import" \
    -u "$N8N_USER:$N8N_PASSWORD" \
    -H "Content-Type: application/json" \
    -d @"$file"

  echo ""
done

echo "All workflows imported successfully!"
```

Save as `import-workflows.sh` and run:

```bash
chmod +x import-workflows.sh
./import-workflows.sh
```

---

## Service-Specific Setup

### SMTP Email Configuration

1. Click **"Credentials"** → **"New Credential"**
2. Search for **"SMTP"**
3. Enter your SMTP settings:

**Gmail Example:**
```
Host: smtp.gmail.com
Port: 587
User: your-email@gmail.com
Password: app-specific-password
Secure: Yes (TLS)
```

**SendGrid Example:**
```
Host: smtp.sendgrid.net
Port: 587
User: apikey
Password: your-sendgrid-api-key
Secure: Yes (TLS)
```

### Twitter Integration

1. Go to [Twitter Developer Portal](https://developer.twitter.com/)
2. Create an app and generate API keys
3. In n8n, create **"Twitter OAuth1 API"** credential:
   - API Key
   - API Secret Key
   - Access Token
   - Access Token Secret

### LinkedIn Integration

1. Create LinkedIn app at [LinkedIn Developers](https://www.linkedin.com/developers/)
2. In n8n, create **"LinkedIn OAuth2 API"** credential
3. Follow OAuth2 authentication flow

### Slack Integration

**Option 1: Incoming Webhooks (Simple)**
1. Go to your Slack workspace
2. Apps → Incoming Webhooks → Add to Slack
3. Choose channel and copy webhook URL
4. Use "Slack Webhook" credential type in n8n

**Option 2: OAuth (Full API Access)**
1. Create Slack app at [Slack API](https://api.slack.com/apps)
2. Add OAuth scopes: `chat:write`, `channels:read`
3. Install app to workspace
4. Use OAuth token in n8n

### Mailchimp Integration

1. Get API key from [Mailchimp Account](https://mailchimp.com/help/about-api-keys/)
2. In n8n, create **"Mailchimp API"** credential
3. Enter API key
4. Get your List ID from Mailchimp audience settings

### Google Services (Sheets & Drive)

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create new project: "BlazorCMS n8n Automation"
3. Enable APIs:
   - Google Sheets API
   - Google Drive API
4. Create OAuth 2.0 credentials:
   - Application Type: Web Application
   - Authorized redirect URIs: `http://localhost:5678/rest/oauth2-credential/callback`
5. Download JSON credentials
6. In n8n:
   - Click **"Credentials"** → **"Google Sheets OAuth2 API"**
   - Enter Client ID and Client Secret
   - Complete OAuth flow

### AWS S3 Integration

1. Create IAM user in AWS Console
2. Attach policy: `AmazonS3FullAccess` (or create custom policy)
3. Generate access key
4. In n8n, create **"AWS"** credential:
   - Access Key ID
   - Secret Access Key
   - Region (e.g., `us-east-1`)

### Azure Blob Storage

1. Create Storage Account in Azure Portal
2. Get connection string from "Access keys"
3. In n8n, create **"Microsoft Azure Storage"** credential:
   - Connection String

---

## Testing Your Setup

### Test 1: Verify BlazorCMS API Connection

1. Import workflow: `1-blog-publication-social-media.json`
2. Open the workflow
3. Click on **"Get Blog Post Details"** node
4. Update the URL to use a valid blog post ID
5. Click **"Execute Node"**
6. Verify you receive blog post data

**Expected Result:**
```json
{
  "id": 1,
  "title": "Sample Blog Post",
  "content": "...",
  "isPublished": true,
  "author": "Admin"
}
```

### Test 2: Test Webhook Trigger

1. Open workflow with webhook trigger
2. Click on **"Webhook"** node
3. Copy the webhook URL (e.g., `http://localhost:5678/webhook/blazorcms-blog-published`)
4. Test with curl:

```bash
curl -X POST http://localhost:5678/webhook-test/blazorcms-blog-published \
  -H "Content-Type: application/json" \
  -d '{
    "body": {
      "blogId": 1,
      "title": "Test Post"
    }
  }'
```

5. Check workflow execution in n8n

### Test 3: Test Email Sending

1. Open workflow: `2-user-registration-welcome.json`
2. Click on **"Send Welcome Email"** node
3. Update recipient email to your email
4. Click **"Execute Node"**
5. Check your inbox

---

## Production Deployment

### Security Checklist

- [ ] Change default n8n admin password
- [ ] Enable HTTPS for n8n (use reverse proxy)
- [ ] Use strong JWT tokens for BlazorCMS
- [ ] Store credentials in n8n credential manager (never in workflows)
- [ ] Set up firewall rules to restrict n8n access
- [ ] Enable n8n user authentication
- [ ] Rotate API keys and tokens regularly

### Performance Optimization

1. **Use Queue Mode** for high-volume workflows:
   ```yaml
   environment:
     - EXECUTIONS_MODE=queue
     - N8N_QUEUE_BULL_REDIS_HOST=redis
     - N8N_QUEUE_BULL_REDIS_PORT=6379
   ```

2. **Set Execution Limits**:
   ```yaml
   environment:
     - EXECUTIONS_DATA_PRUNE=true
     - EXECUTIONS_DATA_MAX_AGE=168  # 7 days
   ```

3. **Enable Workflow Versioning**:
   - Export workflows regularly
   - Use Git for version control

### Monitoring Setup

1. **Enable Webhook Error Notifications**:
   Create a global error workflow that sends alerts to Slack/Email

2. **Set Up Uptime Monitoring**:
   Use tools like:
   - UptimeRobot
   - Pingdom
   - New Relic

3. **Log Aggregation**:
   Send n8n logs to:
   - Elasticsearch + Kibana
   - Loggly
   - Papertrail

### Docker Production Setup

```yaml
version: '3.8'

services:
  n8n:
    image: n8nio/n8n:latest
    restart: always
    ports:
      - "5678:5678"
    environment:
      - N8N_HOST=n8n.yourdomain.com
      - N8N_PORT=5678
      - N8N_PROTOCOL=https
      - NODE_ENV=production
      - WEBHOOK_URL=https://n8n.yourdomain.com/
      - GENERIC_TIMEZONE=America/New_York
      - N8N_ENCRYPTION_KEY=change-to-random-string

      # Execution settings
      - EXECUTIONS_MODE=queue
      - EXECUTIONS_DATA_PRUNE=true
      - EXECUTIONS_DATA_MAX_AGE=168

      # Redis (for queue mode)
      - QUEUE_BULL_REDIS_HOST=redis
      - QUEUE_BULL_REDIS_PORT=6379

    volumes:
      - n8n_data:/home/node/.n8n
      - ./workflows:/backup/workflows
    depends_on:
      - redis

  redis:
    image: redis:7-alpine
    restart: always
    volumes:
      - redis_data:/data

volumes:
  n8n_data:
  redis_data:
```

### Reverse Proxy with Nginx

```nginx
server {
    listen 80;
    server_name n8n.yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name n8n.yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/n8n.yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/n8n.yourdomain.com/privkey.pem;

    location / {
        proxy_pass http://localhost:5678;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;

        # WebSocket support
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## Next Steps

1. ✅ Import all workflows
2. ✅ Configure credentials for services you want to use
3. ✅ Test each workflow individually
4. ✅ Activate workflows one by one
5. ✅ Monitor executions for any errors
6. ✅ Customize workflows to match your needs
7. ✅ Set up production deployment

## Troubleshooting

### Cannot Access n8n Interface

**Solution:** Check if n8n is running:
```bash
# Docker
docker ps | grep n8n

# npm
ps aux | grep n8n
```

### Webhook Not Working

**Solution:** Ensure BlazorCMS can reach n8n:
```bash
# Test from BlazorCMS server
curl http://n8n-host:5678/webhook-test/blazorcms-blog-published
```

### Authentication Errors

**Solution:** Regenerate JWT token and update environment variables

---

## Support

- **n8n Documentation**: https://docs.n8n.io/
- **n8n Community**: https://community.n8n.io/
- **BlazorCMS Issues**: Open GitHub issue

**Setup complete! Happy automating! 🎉**
