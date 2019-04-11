using System.Threading.Tasks;
using Neembly.GPIDServer.Persistence;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Interfaces;

namespace Neembly.GPIDServer.SharedServices.Helpers
{
    public class DbEmailQueueService : IEmailQueueService
    {
        #region Variables
        private readonly UtilsDBContext _utilscontext;
        #endregion

        #region Constructor
        public DbEmailQueueService(UtilsDBContext utilscontext)
        {
            _utilscontext = utilscontext;
        }
        #endregion

        #region Actions
        public async Task<bool> Send(EmailMessage emailMessage)
        {
            EmailQueue emailToAdd = new EmailQueue
            {
                IsHtml = emailMessage.IsHtml,
                Message = emailMessage.Message,
                OperatorId = emailMessage.OperatorId,
                CarbonCopies = emailMessage.CarbonCopies,
                Receipients = emailMessage.Receipients,
                Sender = emailMessage.Sender,
                Subject = emailMessage.Subject,
                BlindCarbonCopies = emailMessage.BlindCarbonCopies,
                Status = emailMessage.Status
            };
            var resultSave = _utilscontext.EmailQueues.Add(emailToAdd);
            if (resultSave == null)
                return false;
            return (await _utilscontext.SaveChangesAsync() > 0); 
        }
        #endregion
    }
}
