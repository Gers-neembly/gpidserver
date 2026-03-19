using FluentValidation;
using Neembly.GPIDServer.Constants;

namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class FlexibleRegisterDTOValidator : AbstractValidator<FlexibleRegisterDTO>
    {
        #region Constructor
        public FlexibleRegisterDTOValidator()
        {
            RuleFor(x => x.UserName).NotNull().NotEmpty().WithMessage(GlobalConstants.ErrRegUsername).Length(0, 20);

            // Either email OR phone must be provided, not both and not neither
            RuleFor(x => x).Must(x => (!string.IsNullOrEmpty(x.Email) && string.IsNullOrEmpty(x.PhoneNumber)) ||
                                      (string.IsNullOrEmpty(x.Email) && !string.IsNullOrEmpty(x.PhoneNumber)))
                           .WithMessage("Either Email or PhoneNumber must be provided, but not both.");

            // Email validation when provided
            When(x => !string.IsNullOrEmpty(x.Email), () => {
                RuleFor(x => x.Email).EmailAddress().WithMessage(GlobalConstants.ErrEmailFormat);
            });

            // Phone validation when provided
            When(x => !string.IsNullOrEmpty(x.PhoneNumber), () => {
                RuleFor(x => x.PhoneNumber).Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format");
            });

            RuleFor(x => x.Password).NotNull().NotEmpty().WithMessage(GlobalConstants.ErrPasswordValue)
                                              .Length(2, 20).WithMessage(GlobalConstants.ErrPasswordLength)
                                              .Equal(x => x.ConfirmPassword).WithMessage(GlobalConstants.ErrPasswordsMismatch);
            RuleFor(x => x.ConfirmPassword).NotNull().NotEmpty().WithMessage(GlobalConstants.ErrConfirmPasswordValue)
                                              .Length(2, 20).WithMessage(GlobalConstants.ErrConfirmPasswordLength);
            RuleFor(x => x.OperatorId).NotNull().NotEmpty().GreaterThan(0);
        }
        #endregion
    }
}