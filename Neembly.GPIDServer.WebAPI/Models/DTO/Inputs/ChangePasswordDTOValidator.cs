using FluentValidation;

namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class ChangePasswordDTOValidator : AbstractValidator<ChangePasswordDTO>
    {
        #region Constructor
        public ChangePasswordDTOValidator()
        {
            RuleFor(x => x.UserName).NotNull().NotEmpty();
            RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress();
            RuleFor(x => x.CurrentPassword).NotNull().NotEmpty();
            RuleFor(x => x.NewPassword).NotNull().NotEmpty();
            RuleFor(x => x.OperatorId).NotNull().NotEmpty().GreaterThan(0);
        }
        #endregion
    }
}

