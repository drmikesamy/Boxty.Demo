namespace Boxty.SharedApp.DTOs.Settings;

/// <summary>Tests the supplied SMTP settings without saving them.</summary>
public sealed record TestSmtpEmailRequest(SmtpConfigDto Settings);
