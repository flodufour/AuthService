using AuthService.Interfaces;

namespace AuthService.Services
{
    public class ConsoleEmailService : IEmailService
    {
        private readonly ILogger<ConsoleEmailService> _logger;

        public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            _logger.LogInformation("Password reset token for {Email}: {Token}", toEmail, resetToken);
            return Task.CompletedTask;
        }
    }
}
