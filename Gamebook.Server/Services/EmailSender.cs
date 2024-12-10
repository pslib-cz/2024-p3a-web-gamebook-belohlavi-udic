using Microsoft.AspNetCore.Identity;

internal class EmailSender<T> : IEmailSender<T> where T : class
{
    public Task SendEmailAsync(T user, string subject, string htmlMessage)
    {
        throw new NotImplementedException();
    }

    public Task SendConfirmationLinkAsync(T user, string email, string confirmationLink)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetLinkAsync(T user, string email, string resetLink)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetCodeAsync(T user, string email, string resetCode)
    {
        throw new NotImplementedException();
    }
}