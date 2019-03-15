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

        public async Task<int> CreatePlayerById(string userId, int operatorId, PlayerInfo playerInfo = null)
        {
            int resultPlayerId = 0;

            var playerProfile = _appDBContext.Players.Where(r => r.NetUserId.Equals(userId, StringComparison.InvariantCultureIgnoreCase)
                                                            && r.OperatorId == operatorId).FirstOrDefault();
            if (playerProfile != null)
                resultPlayerId = playerProfile.PlayerId;
            else
            {
                long tagId = GlobalConstants.PlayerIdTagStarts;
                var operatorRecord = _appDBContext.OperatorData.Where(r => r.OperatorId == operatorId).FirstOrDefault();
                if (operatorRecord == null)
                    _appDBContext.OperatorData.Add(new OperatorData { OperatorId = operatorId, TagId = 1 });
                else
                {
                    tagId = operatorRecord.TagId + 1;
                    operatorRecord.TagId = tagId;
                    _appDBContext.Update(operatorRecord);
                }

                _appDBContext.Players.Add(new Player
                {
                    OperatorId = operatorId,
                    NetUserId = userId,
                    FirstName = playerInfo == null ? string.Empty : playerInfo.FirstName,
                    LastName = playerInfo == null ? string.Empty : playerInfo.LastName,
                    MobilePrefix = playerInfo == null ? string.Empty : playerInfo.MobilePrefix,
                    MobileNo = playerInfo == null ? string.Empty : playerInfo.MobileNo,
                    PlayerId = (int)tagId
                });

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

        public IEnumerable<int> GetPlayersOperators(string netUserId)
        {
            return _appDBContext.Players.Where(r => r.NetUserId.Equals(netUserId, StringComparison.InvariantCultureIgnoreCase)).Select(r => r.OperatorId).ToList();
        }


        public bool UserOperatorExists(string email, string username, int operatorId)
        {
            var appUser = _appDBContext.Users.Where(r => r.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase)
                                                          || r.Email.ToLower() == email.ToLower()).FirstOrDefault();
            return (appUser == null) ? false : CheckOperatorAssignment(appUser.Id, operatorId) != null;
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

        public async Task<bool> ProfileRequestChange(int playerId, int operatorId, PlayerInfo playerInfo)
        {
            var playerRecord = _appDBContext.Players.Where(r => r.PlayerId == playerId && r.OperatorId == operatorId).FirstOrDefault();
            if (playerRecord == null)
                return false;
            playerRecord.FirstName = playerInfo.FirstName;
            playerRecord.LastName = playerInfo.LastName;
            playerRecord.MobilePrefix = playerInfo.MobilePrefix;
            playerRecord.MobileNo = playerInfo.MobileNo;
            return (await _appDBContext.SaveChangesAsync() > 0);
        }

        public PlayerInfo ProfileRequestGet(int playerId, int operatorId)
        {
            var playerRecord = _appDBContext.Players.Where(r => r.PlayerId == playerId && r.OperatorId == operatorId).FirstOrDefault();
            return (new PlayerInfo
            {
                FirstName = playerRecord.FirstName,
                LastName = playerRecord.LastName,
                MobileNo = playerRecord.MobileNo,
                MobilePrefix = playerRecord.MobilePrefix
            });
        }

        #region Private Methods
        private Player CheckOperatorAssignment(string netUserId, int operatorId)
        {
            return _appDBContext.Players.Where(r => r.NetUserId.Equals(netUserId, StringComparison.InvariantCultureIgnoreCase)
                                                    && r.OperatorId == operatorId).FirstOrDefault();
        }
        #endregion

    }
}
