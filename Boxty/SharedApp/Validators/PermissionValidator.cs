using FluentValidation;
using Boxty.SharedBase.DTOs.Auth;
using Boxty.SharedBase.Validation;

namespace Boxty.SharedApp.Validators
{
    public class PermissionValidator : BaseValidator<PermissionDto>
    {
        public PermissionValidator()
        {
            RuleFor(permission => permission.Name)
                .NotEmpty().WithMessage("Permission name is required.")
                .Length(2, 200).WithMessage("Permission name must be between 2 and 200 characters.");
        }
    }
}