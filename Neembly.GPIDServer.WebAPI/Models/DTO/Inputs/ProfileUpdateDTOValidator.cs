using FluentValidation;

namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class ProfileUpdateDTOValidator : AbstractValidator<ProfileUpdateDTO>
    {
        #region Constructor
        public ProfileUpdateDTOValidator()
        {
            RuleFor(x => x.PlayerId).NotNull().NotEmpty().Length(0, 25);
            RuleFor(x => x.PlayerInfo).NotNull();
        }
        #endregion
    }
}
