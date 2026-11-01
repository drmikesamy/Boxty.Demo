using Boxty.SharedApp.DTOs.Settings;
using Boxty.SharedBase.Validation;
using FluentValidation;

namespace Boxty.SharedApp.Validators
{
    public class SmtpConfigValidator : BaseValidator<SmtpConfigDto>
    {
        public SmtpConfigValidator()
        {
            RuleFor(config => config.Name).NotEmpty();
            RuleFor(config => config.SenderAddress).NotEmpty().EmailAddress();
            RuleFor(config => config.SenderName).NotEmpty();
            RuleFor(config => config.SmtpHost).NotEmpty();
            RuleFor(config => config.SmtpPort).InclusiveBetween(1, 65535);
            RuleFor(config => config.SmtpUsername).NotEmpty();
            RuleFor(config => config.SmtpPassword).NotEmpty();
        }
    }
}
