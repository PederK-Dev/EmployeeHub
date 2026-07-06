namespace EmployeeHub.Api.Services;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}

/// <summary>
/// Development email sender: writes the message to the application log instead of sending it.
/// Verification and password-reset links appear in the API console, so the flows can be exercised
/// end-to-end without an SMTP provider. Swap for a real implementation in production.
/// </summary>
public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string body)
    {
        _logger.LogInformation(
            "\n===== DEV EMAIL =====\nTo: {To}\nSubject: {Subject}\n{Body}\n=====================",
            to,
            subject,
            body);

        return Task.CompletedTask;
    }
}
