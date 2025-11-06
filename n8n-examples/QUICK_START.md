# Quick Start Guide - BlazorCMS n8n Automation

Get started with n8n automation for BlazorCMS in under 10 minutes!

## Prerequisites

- ✅ BlazorCMS running locally
- ✅ Docker installed (or Node.js 18+)
- ✅ 10 minutes of your time

## Step 1: Start n8n (2 minutes)

### Using Docker (Recommended)

```bash
docker run -it --rm \
  --name n8n \
  -p 5678:5678 \
  -v ~/.n8n:/home/node/.n8n \
  n8nio/n8n
```

### Using npx (No Installation)

```bash
npx n8n
```

Access n8n at: **http://localhost:5678**

## Step 2: Get Your JWT Token (1 minute)

```bash
# Login to BlazorCMS to get JWT token
curl -X POST http://localhost:7250/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@blazorcms.com",
    "password": "your-password"
  }'
```

Copy the `token` from the response.

## Step 3: Set Environment Variables (1 minute)

**For Docker:** Stop the container (Ctrl+C) and restart with environment variables:

```bash
docker run -it --rm \
  --name n8n \
  -p 5678:5678 \
  -e BLAZORCMS_API_URL=http://host.docker.internal:7250 \
  -e BLAZORCMS_PUBLIC_URL=http://localhost:5000 \
  -e BLAZORCMS_JWT_TOKEN="your-jwt-token-here" \
  -e ADMIN_EMAIL="admin@yourdomain.com" \
  -v ~/.n8n:/home/node/.n8n \
  n8nio/n8n
```

**For npx:** Create `~/.n8n/.env`:

```env
BLAZORCMS_API_URL=http://localhost:7250
BLAZORCMS_PUBLIC_URL=http://localhost:5000
BLAZORCMS_JWT_TOKEN=your-jwt-token-here
ADMIN_EMAIL=admin@yourdomain.com
```

Restart n8n.

## Step 4: Import a Workflow (2 minutes)

1. Open **http://localhost:5678** in your browser
2. Complete the welcome setup (create account)
3. Click **"Workflows"** → **"Add Workflow"** → **"Import from File"**
4. Choose `n8n-examples/workflows/1-blog-publication-social-media.json`
5. Click **"Import"**

## Step 5: Test the Workflow (2 minutes)

### Configure Test Data

1. Open the imported workflow
2. Click on **"Get Blog Post Details"** node
3. Change the URL to use an existing blog post ID from your database
4. Click **"Execute Node"**

You should see the blog post data!

### Test Full Workflow

1. Click on **"Webhook - Blog Published"** node
2. Copy the test webhook URL
3. Test it with curl:

```bash
curl -X POST http://localhost:5678/webhook-test/blazorcms-blog-published \
  -H "Content-Type: application/json" \
  -d '{
    "body": {
      "blogId": 1,
      "title": "Test Blog Post"
    }
  }'
```

4. Check the execution log in n8n

## Step 6: (Optional) Enable Auto-Publishing (2 minutes)

Want automatic social media sharing when you publish a blog?

1. Activate the workflow (toggle in top-right)
2. Copy the production webhook URL
3. Add to `BlazorCMS.API/appsettings.json`:

```json
{
  "N8N": {
    "Webhooks": {
      "blog-published": "http://localhost:5678/webhook/blazorcms-blog-published"
    }
  }
}
```

4. Follow the [Webhook Integration Guide](WEBHOOK_INTEGRATION.md) to add webhook calls to your API

## What's Next?

### Try More Workflows

Import and test other workflows:

- ✉️ **User Registration Welcome** - Send welcome emails to new users
- 💾 **Blog Backup** - Auto-backup to cloud storage
- ⏰ **Scheduled Publishing** - Schedule posts for future publication
- 📧 **Email Subscribers** - Notify subscribers of new posts

### Configure Services

For full functionality, set up credentials for:

- **Email**: SMTP or SendGrid
- **Social Media**: Twitter, LinkedIn
- **Cloud Storage**: Google Drive, AWS S3, Azure
- **Communication**: Slack, Discord

See [Setup Guide](SETUP_GUIDE.md) for detailed instructions.

### Customize Workflows

All workflows are fully customizable:

1. Open any workflow in n8n
2. Edit node parameters (change text, URLs, etc.)
3. Add new nodes by clicking the **+** button
4. Connect nodes by dragging between them
5. Save and test!

## Quick Reference

### Useful URLs

- **n8n Interface**: http://localhost:5678
- **BlazorCMS API**: http://localhost:7250
- **BlazorCMS Admin**: http://localhost:5001
- **Swagger Docs**: http://localhost:7250/swagger

### Common Commands

```bash
# Start n8n with Docker
docker run -it --rm --name n8n -p 5678:5678 -v ~/.n8n:/home/node/.n8n n8nio/n8n

# Start n8n with npx
npx n8n

# Get JWT token
curl -X POST http://localhost:7250/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@blazorcms.com","password":"your-password"}'

# Test webhook
curl -X POST http://localhost:5678/webhook-test/blazorcms-blog-published \
  -H "Content-Type: application/json" \
  -d '{"body":{"blogId":1}}'

# View n8n logs (Docker)
docker logs -f n8n
```

### Troubleshooting

| Problem | Solution |
|---------|----------|
| Can't access n8n | Check if running: `docker ps` or `ps aux \| grep n8n` |
| Webhook not working | Verify workflow is activated (toggle green) |
| 401 Unauthorized | JWT token expired - generate new one |
| Can't reach API | Use `host.docker.internal` instead of `localhost` in Docker |

## Get Help

- 📖 [Full Documentation](README.md)
- 🔧 [Setup Guide](SETUP_GUIDE.md)
- 🔗 [Webhook Integration](WEBHOOK_INTEGRATION.md)
- 💬 [n8n Community](https://community.n8n.io/)

---

**🎉 You're all set!** Start automating your BlazorCMS workflows!

**Pro Tip:** Export workflows regularly as backup: Workflows → ⋮ → Download
