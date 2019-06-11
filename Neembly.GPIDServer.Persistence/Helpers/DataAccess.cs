using Neembly.GPIDServer.Constants;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.Persistence.Interfaces;
using Neembly.GPIDServer.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.Persistence.Helpers
{
    public class DataAccess : IDataAccess
    {
        private readonly AppDBContext _appDBContext;

        public DataAccess(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        public async Task<int> GeneratePlayerId(string userName, string email, int operatorId)
        {
            int resultPlayerId = 0;

            var playerProfile = _appDBContext.Users.Where(r => r.Email.ToLower() == email.ToLower()
                                             && r.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (playerProfile != null)
                resultPlayerId = playerProfile.PlayerId;
            else
            {
                long tagId = GlobalConstants.PlayerIdTagStarts;
                var operatorRecord = _appDBContext.OperatorData.FirstOrDefault();
                if (operatorRecord == null)
                    _appDBContext.OperatorData.Add(new OperatorData { OperatorId = operatorId, TagId = tagId });
                else
                {
                    tagId = operatorRecord.TagId + 1;
                    operatorRecord.TagId = tagId;
                    _appDBContext.Update(operatorRecord);
                }

                resultPlayerId = (int)tagId;
                await _appDBContext.SaveChangesAsync();
            }
            return (resultPlayerId);
        }

        public AppUser GetAppUser(string email, string username)
        {
            return _appDBContext.Users.Where(r => r.Email.ToLower() == email.ToLower()
                                             && r.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        public AppUser GetAppUser(string username, int playerId)
        {
            return _appDBContext.Users.Where(r => r.PlayerId == playerId
                                             && r.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        public IEnumerable<int> GetPlayersOperators(string netUserId)
        {
            return _appDBContext.Users.Where(r => r.Id.Equals(netUserId, StringComparison.InvariantCultureIgnoreCase)).Select(r => r.OperatorId).ToList();
        }


        public bool UserExists(string email, string username, int operatorId)
        {
            var appUser = _appDBContext.Users.Where(r => (r.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase) || r.Email.ToLower() == email.ToLower()) && r.OperatorId == operatorId ).FirstOrDefault();
            return (appUser == null) ? false : true;
        }

        public bool EmailExists(string email, int operatorId, int playerId)
        {
            var appUser = _appDBContext.Users.Where(r => r.OperatorId == operatorId
                            && r.Email.ToLower() == email.ToLower()).FirstOrDefault();
            bool isExists = true;
            if (appUser == null)
            {
                isExists= false;
            } else
            { if (appUser.PlayerId == playerId) isExists=false;
            }
            return isExists;
            //return (appUser != null || appUser.PlayerId!=playerId) ? false : true;
        }

        public async Task<bool> SetRegistrationStatus(string userId, RegistrationStatusNames registerStatus)
        {
            var playerInfo = await _appDBContext.Users.FindAsync(userId);
            if (playerInfo == null)
                return false;
            string strStatus = Enum.GetName(typeof(RegistrationStatusNames), registerStatus);
            if (playerInfo.RegistrationStatus.Equals(strStatus, StringComparison.InvariantCultureIgnoreCase))
                return true;
            else
            {
                playerInfo.RegistrationStatus = strStatus;
                return (await _appDBContext.SaveChangesAsync() > 0);
            }
        }

        #region Private Methods
        private AppUser CheckOperatorAssignment(string netUserId, int operatorId)
        {
            return _appDBContext.Users.Where(r => r.Id.Equals(netUserId, StringComparison.InvariantCultureIgnoreCase)
                                                    && r.OperatorId == operatorId).FirstOrDefault();
        }
        #endregion

    }
}
