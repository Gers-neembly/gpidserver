using FluentValidation;

namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class ResetPasswordAutoTokenDTOValidator : AbstractValidator<ResetPasswordAutoTokenDTO>
    {
        #region Constructor
        public ResetPasswordAutoTokenDTOValidator()
        {
            RuleFor(x => x.UserName).NotNull().NotEmpty();
            RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress();
            RuleFor(x => x.NewPassword).NotNull().NotEmpty();
            RuleFor(x => x.OperatorId).NotNull().NotEmpty().GreaterThan(0);
        }
        #endregion
    }
}

