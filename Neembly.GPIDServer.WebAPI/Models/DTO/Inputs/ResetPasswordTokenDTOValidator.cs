using FluentValidation;

namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class ResetPasswordTokenDTOValidator : AbstractValidator<ResetPasswordTokenDTO>
    {
        #region Constructor
        public ResetPasswordTokenDTOValidator()
        {
            RuleFor(x => x.UserName).NotNull().NotEmpty();
            RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress();
            RuleFor(x => x.OperatorId).NotNull().NotEmpty().GreaterThan(0);
        }
        #endregion
    }
}
