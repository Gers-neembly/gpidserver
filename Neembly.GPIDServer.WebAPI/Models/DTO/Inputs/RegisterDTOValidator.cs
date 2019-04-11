using FluentValidation;
using Neembly.GPIDServer.Constants;

namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class RegisterDTOValidator : AbstractValidator<RegisterDTO>
    {
        #region Constructor
        public RegisterDTOValidator()
        {
            RuleFor(x => x.UserName).NotNull().NotEmpty().WithMessage(GlobalConstants.ErrRegUsername).Length(0, 20);
            RuleFor(x => x.Email).NotNull().NotEmpty().WithMessage(GlobalConstants.ErrEmailValue)
                                                      .EmailAddress().WithMessage(GlobalConstants.ErrEmailFormat);
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
