# BlazorCMS n8n Automation Examples

This directory contains example n8n workflows for automating various aspects of your BlazorCMS installation. These workflows demonstrate how to integrate BlazorCMS with external services, automate content distribution, manage backups, and streamline your content management processes.

## 📋 Overview

n8n is a powerful workflow automation tool that allows you to connect BlazorCMS with hundreds of other services and automate repetitive tasks. These examples show you how to leverage n8n to build a complete content automation pipeline.

## 🚀 Available Workflows

### 1. Blog Publication to Social Media
**File:** `workflows/1-blog-publication-social-media.json`

Automatically share your blog posts across multiple social media platforms when published.

**Features:**
- Triggered by webhook when a blog post is published
- Fetches complete blog post details from the API
- Posts to Twitter, LinkedIn, and Slack simultaneously
- Customizable message templates for each platform

**Use Case:** Maximize your content reach by automatically distributing blog posts to your social media channels without manual effort.

---

### 2. User Registration Welcome Flow
**File:** `workflows/2-user-registration-welcome.json`

Create a comprehensive onboarding experience for new users.

**Features:**
- Triggered by webhook on user registration
- Sends personalized welcome email with HTML template
- Adds user to Mailchimp mailing list
- Notifies admin team via Slack
- Logs registration to Google Sheets for analytics

**Use Case:** Provide a professional first impression and automatically organize your user database across multiple platforms.

---

### 3. Blog Backup to Cloud Storage
**File:** `workflows/3-blog-backup-cloud-storage.json`

Automatically backup all your blog content to multiple cloud storage providers.

**Features:**
- Scheduled to run daily (configurable)
- Fetches all blog posts via API
- Creates timestamped JSON backup files
- Uploads to Google Drive, AWS S3, and Azure Blob Storage
- Sends email notification on completion

**Use Case:** Ensure your content is safe with regular automated backups to multiple cloud providers.

---

### 4. Scheduled Blog Publishing
**File:** `workflows/4-scheduled-blog-publishing.json`

Schedule blog posts to be published automatically at a future date/time.

**Features:**
- Checks every 15 minutes for posts scheduled to publish
- Automatically publishes posts when scheduled time arrives
- Triggers social media sharing workflow
- Notifies author via email when post goes live

**Use Case:** Write content in advance and have it published automatically at optimal times for your audience.

---

### 5. Email Subscribers on New Post
**File:** `workflows/5-email-subscribers-notification.json`

Send beautiful email notifications to all subscribers when you publish new content.

**Features:**
- Triggered by webhook on blog publication
- Fetches subscribers from Mailchimp and Google Sheets
- Deduplicates subscriber list
- Sends personalized HTML emails with blog excerpt
- Rate-limited to avoid spam filters
- Notifies admin team on completion

**Use Case:** Keep your audience engaged by automatically notifying them of new content.

---

## 🔧 Prerequisites

Before using these workflows, you need:

1. **n8n Installation**
   - Self-hosted: [n8n Installation Guide](https://docs.n8n.io/hosting/)
   - Cloud: [n8n Cloud](https://n8n.io/cloud/)

2. **BlazorCMS Setup**
   - Running BlazorCMS API instance
   - Valid JWT authentication token
   - Public URL for webhook callbacks (optional but recommended)

3. **Service Accounts** (depending on which workflows you use)
   - Email: SMTP credentials
   - Twitter: API credentials (OAuth)
   - LinkedIn: OAuth credentials
   - Mailchimp: API key
   - Google Sheets/Drive: OAuth credentials
   - AWS S3: Access key and secret
   - Azure Blob Storage: Connection string
   - Slack: Webhook URL or OAuth token

## ⚙️ Setup Instructions

### Step 1: Install n8n

Choose your installation method:

**Docker (Recommended):**
```bash
docker run -it --rm \
  --name n8n \
  -p 5678:5678 \
  -v ~/.n8n:/home/node/.n8n \
  n8nio/n8n
```

**npm:**
```bash
npm install n8n -g
n8n start
```

Access n8n at `http://localhost:5678`

### Step 2: Import Workflows

1. Open n8n in your browser
2. Click **"Workflows"** in the left sidebar
3. Click **"Import from File"**
4. Select one of the workflow JSON files from the `workflows/` directory
5. Click **"Import"**

### Step 3: Configure Environment Variables

Create a `.env` file in your n8n directory with the following variables:

```env
# BlazorCMS Configuration
BLAZORCMS_API_URL=http://localhost:7250
BLAZORCMS_PUBLIC_URL=http://localhost:5000
BLAZORCMS_JWT_TOKEN=your-jwt-token-here
BLAZORCMS_FROM_EMAIL=noreply@blazorcms.com

# Admin Configuration
ADMIN_EMAIL=admin@yourdomain.com

# Mailchimp (for user management and subscribers)
MAILCHIMP_LIST_ID=your-list-id

# Google Services
GOOGLE_SHEETS_ID=your-spreadsheet-id
GOOGLE_DRIVE_BACKUP_FOLDER_ID=your-folder-id

# AWS S3 (for backups)
AWS_S3_BUCKET_NAME=blazorcms-backups

# Azure Blob Storage (for backups)
AZURE_CONTAINER_NAME=blazorcms-backups

# Slack
SLACK_ADMIN_CHANNEL=C01234567

# n8n Webhooks
N8N_WEBHOOK_BLOG_PUBLISHED=http://localhost:5678/webhook/blazorcms-blog-published
```

### Step 4: Set Up Credentials

For each workflow, you'll need to configure credentials:

1. Click **"Credentials"** in the left sidebar
2. Click **"New Credential"**
3. Select the service type (SMTP, Twitter, Google, etc.)
4. Follow the authentication flow
5. Save the credential

### Step 5: Activate Workflows

1. Open each workflow
2. Click **"Active"** toggle in the top-right corner
3. Verify webhook URLs if using webhook triggers

## 🔗 Integrating with BlazorCMS

### Option 1: Webhook Triggers (Recommended)

To trigger workflows automatically, you need to add webhook calls to your BlazorCMS API.

**Example: Trigger on Blog Publication**

Add this code to `BlazorCMS.API/Services/BlogService.cs` after creating/publishing a blog post:

```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class BlogService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public async Task PublishBlogAsync(int blogId)
    {
        // ... existing blog publishing code ...

        // Trigger n8n webhook
        await TriggerWebhookAsync("blog-published", new
        {
            blogId = blog.Id,
            title = blog.Title,
            publishedDate = DateTime.UtcNow
        });
    }

    private async Task TriggerWebhookAsync(string webhookName, object data)
    {
        try
        {
            var webhookUrl = _configuration[$"N8N:Webhooks:{webhookName}"];
            if (string.IsNullOrEmpty(webhookUrl)) return;

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(
                JsonSerializer.Serialize(data),
                Encoding.UTF8,
                "application/json"
            );

            await client.PostAsync(webhookUrl, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to trigger webhook: {webhookName}");
        }
    }
}
```

Add webhook URLs to `appsettings.json`:

```json
{
  "N8N": {
    "Webhooks": {
      "blog-published": "http://localhost:5678/webhook/blazorcms-blog-published",
      "user-registered": "http://localhost:5678/webhook/blazorcms-user-registered",
      "notify-subscribers": "http://localhost:5678/webhook/blazorcms-notify-subscribers"
    }
  }
}
```

### Option 2: Manual Triggers

You can manually trigger workflows using HTTP requests:

```bash
curl -X POST http://localhost:5678/webhook/blazorcms-blog-published \
  -H "Content-Type: application/json" \
  -d '{
    "blogId": 1,
    "title": "My New Blog Post",
    "publishedDate": "2025-01-15T10:00:00Z"
  }'
```

### Option 3: Scheduled Workflows

Some workflows (like backups and scheduled publishing) run on a timer and don't require triggers. Simply activate them and they'll run automatically.

## 🧪 Testing Workflows

### Test Individual Nodes

1. Open a workflow in n8n
2. Click on any node
3. Click **"Execute Node"** to test that specific step
4. Review the output in the right panel

### Test Complete Workflow

1. Click **"Execute Workflow"** in the bottom-right
2. For webhook triggers, use a test HTTP request:

```bash
curl -X POST http://localhost:5678/webhook-test/blazorcms-blog-published \
  -H "Content-Type: application/json" \
  -d '{"body": {"blogId": 1}}'
```

3. Review execution log for any errors

## 📊 Monitoring and Logs

### View Execution History

1. Click **"Executions"** in the left sidebar
2. See all workflow runs with status (success/error)
3. Click any execution to see detailed logs

### Error Handling

All workflows include basic error handling. For production:

1. Add error workflows using the "On Workflow Error" trigger
2. Set up email/Slack notifications for failures
3. Enable workflow execution logging in n8n settings

## 🎨 Customization

### Modify Message Templates

Edit the text/HTML content in the email and social media nodes to match your branding.

### Adjust Schedules

For scheduled workflows:
1. Open the workflow
2. Click the "Schedule Trigger" node
3. Modify the interval (minutes, hours, days)
4. Save the workflow

### Add New Services

You can extend these workflows with additional services:
- Discord notifications
- Telegram messages
- Trello card creation
- Notion page updates
- And 300+ other integrations

## 🔒 Security Best Practices

1. **JWT Tokens**: Rotate your BlazorCMS JWT tokens regularly
2. **Environment Variables**: Never commit `.env` files to version control
3. **HTTPS**: Use HTTPS for all webhook URLs in production
4. **API Keys**: Store all credentials securely in n8n's credential manager
5. **Rate Limiting**: Implement rate limiting on your webhook endpoints
6. **Validation**: Validate all incoming webhook data before processing

## 🐛 Troubleshooting

### Workflow Not Triggering

**Issue**: Webhook workflow doesn't execute when expected

**Solutions**:
- Verify webhook URL is correct and accessible
- Check BlazorCMS is sending webhook requests (check logs)
- Ensure workflow is activated (toggle is green)
- Test webhook URL with curl command

### Authentication Errors

**Issue**: "401 Unauthorized" when calling BlazorCMS API

**Solutions**:
- Generate a new JWT token from BlazorCMS
- Update `BLAZORCMS_JWT_TOKEN` environment variable
- Verify token hasn't expired (default: 2 hours)
- Check Authorization header format: `Bearer <token>`

### Email Not Sending

**Issue**: Emails not being delivered

**Solutions**:
- Verify SMTP credentials are correct
- Check spam folder
- Enable "Less secure app access" if using Gmail
- Use a dedicated email service (SendGrid, Mailgun) for production

### Rate Limiting Issues

**Issue**: Social media posts failing due to rate limits

**Solutions**:
- Add delay nodes between API calls
- Use workflow batching
- Implement queue system for high-volume scenarios

## 📚 Resources

- [n8n Documentation](https://docs.n8n.io/)
- [n8n Community Forum](https://community.n8n.io/)
- [BlazorCMS Documentation](../README.md)
- [n8n Workflow Templates](https://n8n.io/workflows/)

## 🤝 Contributing

Have an idea for a new workflow? Submit a pull request with:
1. The workflow JSON file
2. Documentation in this README
3. Example configuration

## 📝 License

These examples are provided as-is for use with BlazorCMS. Modify and adapt them to your needs.

## 💬 Support

For issues specific to:
- **n8n**: Visit [n8n Community Forum](https://community.n8n.io/)
- **BlazorCMS**: Open an issue in the BlazorCMS repository
- **These Workflows**: Create an issue with the `n8n` label

---

**Happy Automating! 🚀**
