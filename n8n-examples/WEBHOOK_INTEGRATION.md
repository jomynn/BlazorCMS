# Webhook Integration Guide for BlazorCMS

This guide shows you how to integrate n8n webhook triggers into your BlazorCMS API to enable automatic workflow execution.

## Overview

Webhooks allow BlazorCMS to notify n8n when important events occur (blog published, user registered, etc.). This enables real-time automation without polling or manual triggers.

## Architecture

```
┌─────────────────┐         Webhook          ┌──────────────┐
│   BlazorCMS     │────────────────────────>  │     n8n      │
│   API Event     │   HTTP POST /webhook     │   Workflow   │
└─────────────────┘                           └──────────────┘
                                                     │
                                                     ▼
                                              ┌──────────────┐
                                              │   External   │
                                              │   Services   │
                                              └──────────────┘
```

## Step 1: Create Webhook Service

Create a new service to handle webhook notifications in your BlazorCMS API.

### File: `BlazorCMS.Infrastructure/Webhooks/WebhookService.cs`

```csharp
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlazorCMS.Infrastructure.Webhooks
{
    public interface IWebhookService
    {
        Task NotifyAsync(string eventName, object data);
        Task NotifyBlogPublishedAsync(int blogId, string title, DateTime publishedDate);
        Task NotifyUserRegisteredAsync(string userId, string email, string fullName, DateTime registeredOn);
    }

    public class WebhookService : IWebhookService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WebhookService> _logger;

        public WebhookService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<WebhookService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task NotifyAsync(string eventName, object data)
        {
            try
            {
                var webhookUrl = _configuration[$"N8N:Webhooks:{eventName}"];

                if (string.IsNullOrEmpty(webhookUrl))
                {
                    _logger.LogDebug($"Webhook not configured for event: {eventName}");
                    return;
                }

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5); // Short timeout for webhooks

                var payload = new
                {
                    @event = eventName,
                    timestamp = DateTime.UtcNow,
                    body = data
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                _logger.LogInformation($"Sending webhook notification: {eventName}");

                var response = await client.PostAsync(webhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Webhook notification sent successfully: {eventName}");
                }
                else
                {
                    _logger.LogWarning(
                        $"Webhook notification failed: {eventName}. Status: {response.StatusCode}"
                    );
                }
            }
            catch (Exception ex)
            {
                // Don't throw - webhook failures shouldn't break the main flow
                _logger.LogError(ex, $"Error sending webhook notification: {eventName}");
            }
        }

        public async Task NotifyBlogPublishedAsync(int blogId, string title, DateTime publishedDate)
        {
            await NotifyAsync("blog-published", new
            {
                blogId,
                title,
                publishedDate
            });
        }

        public async Task NotifyUserRegisteredAsync(
            string userId,
            string email,
            string fullName,
            DateTime registeredOn)
        {
            await NotifyAsync("user-registered", new
            {
                userId,
                email,
                fullName,
                registeredOn
            });
        }
    }
}
```

## Step 2: Register Service

Register the webhook service in your dependency injection container.

### File: `BlazorCMS.API/Program.cs`

```csharp
using BlazorCMS.Infrastructure.Webhooks;

// ... existing code ...

// Register HTTP Client Factory
builder.Services.AddHttpClient();

// Register Webhook Service
builder.Services.AddScoped<IWebhookService, WebhookService>();

// ... rest of the code ...
```

## Step 3: Configure Webhook URLs

Add webhook URLs to your appsettings configuration.

### File: `BlazorCMS.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=blazorcms.db"
  },
  "Jwt": {
    "Key": "your-secret-key-here",
    "Issuer": "BlazorCMS",
    "Audience": "BlazorCMS",
    "ExpirationHours": 2
  },
  "N8N": {
    "Enabled": true,
    "Webhooks": {
      "blog-published": "http://localhost:5678/webhook/blazorcms-blog-published",
      "user-registered": "http://localhost:5678/webhook/blazorcms-user-registered",
      "notify-subscribers": "http://localhost:5678/webhook/blazorcms-notify-subscribers"
    }
  }
}
```

### File: `BlazorCMS.API/appsettings.Development.json`

```json
{
  "N8N": {
    "Enabled": true,
    "Webhooks": {
      "blog-published": "http://localhost:5678/webhook/blazorcms-blog-published",
      "user-registered": "http://localhost:5678/webhook/blazorcms-user-registered",
      "notify-subscribers": "http://localhost:5678/webhook/blazorcms-notify-subscribers"
    }
  }
}
```

### File: `BlazorCMS.API/appsettings.Production.json`

```json
{
  "N8N": {
    "Enabled": true,
    "Webhooks": {
      "blog-published": "https://n8n.yourdomain.com/webhook/blazorcms-blog-published",
      "user-registered": "https://n8n.yourdomain.com/webhook/blazorcms-user-registered",
      "notify-subscribers": "https://n8n.yourdomain.com/webhook/blazorcms-notify-subscribers"
    }
  }
}
```

## Step 4: Integrate into Blog Service

Add webhook notifications to your blog service when posts are published.

### File: `BlazorCMS.API/Services/BlogService.cs`

```csharp
using BlazorCMS.Infrastructure.Webhooks;

public class BlogService
{
    private readonly IWebhookService _webhookService;

    public BlogService(
        IBlogRepository blogRepository,
        IWebhookService webhookService,
        ILogger<BlogService> logger)
    {
        _blogRepository = blogRepository;
        _webhookService = webhookService;
        _logger = logger;
    }

    public async Task<BlogPost> CreateBlogPostAsync(BlogPostDTO blogPostDto)
    {
        // ... existing blog creation code ...

        var blogPost = await _blogRepository.CreateAsync(newBlogPost);

        // Notify n8n if blog is published
        if (blogPost.IsPublished)
        {
            await _webhookService.NotifyBlogPublishedAsync(
                blogPost.Id,
                blogPost.Title,
                blogPost.PublishedDate ?? DateTime.UtcNow
            );
        }

        return blogPost;
    }

    public async Task<BlogPost> UpdateBlogPostAsync(int id, BlogPostDTO blogPostDto)
    {
        // ... existing blog update code ...

        var updatedBlog = await _blogRepository.UpdateAsync(existingBlog);

        // Check if blog was just published (transition from unpublished to published)
        if (updatedBlog.IsPublished && !existingBlog.IsPublished)
        {
            await _webhookService.NotifyBlogPublishedAsync(
                updatedBlog.Id,
                updatedBlog.Title,
                updatedBlog.PublishedDate ?? DateTime.UtcNow
            );
        }

        return updatedBlog;
    }
}
```

## Step 5: Integrate into Auth Service

Add webhook notifications when users register.

### File: `BlazorCMS.API/Controllers/AuthController.cs`

```csharp
using BlazorCMS.Infrastructure.Webhooks;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebhookService _webhookService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IWebhookService webhookService)
    {
        _userManager = userManager;
        _webhookService = webhookService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO model)
    {
        // ... existing registration code ...

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Notify n8n about new user registration
            await _webhookService.NotifyUserRegisteredAsync(
                user.Id,
                user.Email,
                user.FullName,
                user.RegisteredOn
            );

            return Ok(new { message = "User registered successfully" });
        }

        return BadRequest(result.Errors);
    }
}
```

## Step 6: Test Your Integration

### Test Blog Publication Webhook

```bash
# Start n8n and import the blog publication workflow
# Activate the workflow

# Create and publish a blog post via API
curl -X POST http://localhost:7250/api/blog \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Blog Post",
    "content": "This is a test blog post to trigger the webhook.",
    "authorId": "your-user-id",
    "isPublished": true
  }'

# Check n8n execution log to verify webhook was received
```

### Test User Registration Webhook

```bash
# Start n8n and import the user registration workflow
# Activate the workflow

# Register a new user via API
curl -X POST http://localhost:7250/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "TestPassword123!",
    "fullName": "Test User"
  }'

# Check n8n execution log and your email for welcome message
```

## Step 7: Advanced Features

### Webhook Retry Logic

Add retry logic for failed webhook notifications:

```csharp
public async Task NotifyWithRetryAsync(string eventName, object data, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            await NotifyAsync(eventName, data);
            return; // Success
        }
        catch (Exception ex)
        {
            if (i == maxRetries - 1)
            {
                _logger.LogError(ex, $"Webhook failed after {maxRetries} retries: {eventName}");
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i))); // Exponential backoff
            }
        }
    }
}
```

### Webhook Queue (Background Processing)

For high-traffic applications, queue webhook notifications:

```csharp
using System.Threading.Channels;

public class WebhookQueue : BackgroundService
{
    private readonly Channel<WebhookNotification> _queue;
    private readonly IServiceProvider _serviceProvider;

    public WebhookQueue(IServiceProvider serviceProvider)
    {
        _queue = Channel.CreateUnbounded<WebhookNotification>();
        _serviceProvider = serviceProvider;
    }

    public void Enqueue(string eventName, object data)
    {
        _queue.Writer.TryWrite(new WebhookNotification
        {
            EventName = eventName,
            Data = data,
            Timestamp = DateTime.UtcNow
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var notification in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = _serviceProvider.CreateScope();
            var webhookService = scope.ServiceProvider.GetRequiredService<IWebhookService>();

            await webhookService.NotifyAsync(notification.EventName, notification.Data);
        }
    }
}
```

### Webhook Signatures (Security)

Add HMAC signatures to verify webhook authenticity:

```csharp
private string GenerateSignature(string payload, string secret)
{
    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
    return Convert.ToBase64String(hash);
}

public async Task NotifySecureAsync(string eventName, object data)
{
    var payload = JsonSerializer.Serialize(data);
    var secret = _configuration["N8N:WebhookSecret"];
    var signature = GenerateSignature(payload, secret);

    var content = new StringContent(payload, Encoding.UTF8, "application/json");
    content.Headers.Add("X-Webhook-Signature", signature);

    // ... send webhook ...
}
```

## Troubleshooting

### Webhook Not Triggering

**Check:**
1. Is n8n workflow activated? (toggle should be green)
2. Is webhook URL correct in appsettings.json?
3. Can BlazorCMS reach n8n? (test with curl)
4. Check BlazorCMS logs for webhook errors
5. Check n8n execution log for incoming requests

### Webhook Timeout

**Solution:** Webhooks should complete quickly. For long-running tasks:
1. Return immediately from webhook
2. Process tasks asynchronously in n8n
3. Use webhook queue for high volume

### Production Considerations

1. **Use HTTPS** for all webhook URLs
2. **Add authentication** (API keys or signatures)
3. **Implement rate limiting** to prevent abuse
4. **Monitor webhook failures** and alert on repeated errors
5. **Use background queue** for high-traffic scenarios
6. **Add webhook logging** for audit trail

## Complete Example

Here's a complete working example combining all concepts:

```csharp
// Startup configuration
builder.Services.AddHttpClient();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddHostedService<WebhookQueue>();

// Blog controller with webhook
[HttpPost]
public async Task<IActionResult> CreateBlog([FromBody] BlogPostDTO blogDto)
{
    var blog = await _blogService.CreateAsync(blogDto);

    if (blog.IsPublished)
    {
        // Fire and forget webhook notification
        _ = _webhookService.NotifyBlogPublishedAsync(blog.Id, blog.Title, DateTime.UtcNow);
    }

    return CreatedAtAction(nameof(GetBlog), new { id = blog.Id }, blog);
}
```

## Next Steps

1. ✅ Implement webhook service in your API
2. ✅ Configure webhook URLs
3. ✅ Integrate into blog and auth services
4. ✅ Test with n8n workflows
5. ✅ Add error handling and logging
6. ✅ Deploy to production with HTTPS

---

**Questions or issues?** Open a GitHub issue with the `webhook` label.
