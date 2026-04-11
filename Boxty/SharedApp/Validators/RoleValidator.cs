using FluentValidation;
using Boxty.SharedBase.DTOs.Auth;
using Boxty.SharedBase.Validation;

namespace Boxty.SharedApp.Validators
{
    public class RoleValidator : BaseValidator<RoleDto>
    {
        public RoleValidator()
        {
            RuleFor(role => role.Name)
                .NotEmpty().WithMessage("Role name is required.")
                .Length(2, 100).WithMessage("Role name must be between 2 and 100 characters.");
        }
    }
}