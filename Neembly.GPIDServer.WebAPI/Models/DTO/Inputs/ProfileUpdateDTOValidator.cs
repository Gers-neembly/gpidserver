using FluentValidation;
using Neembly.GPIDServer.Constants;

namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class ProfileUpdateDTOValidator : AbstractValidator<ProfileUpdateDTO>
    {
        #region Constructor
        public ProfileUpdateDTOValidator()
        {
            RuleFor(x => x.PlayerId).NotNull().NotEmpty().WithMessage(GlobalConstants.ErrUserAccountNotExisting)
                                              .Length(0, 25);
            RuleFor(x => x.PlayerInfo).NotNull();
        }
        #endregion
    }
}
