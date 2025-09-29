using Microsoft.EntityFrameworkCore;
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

        public async Task<int> GeneratePlayerId(string userName, string contactInfo, int operatorId)
        {
            int resultPlayerId = 0;

            // Query by contact info and username - validation should have happened before this method
            AppUser playerProfile = null;
            if (contactInfo.Contains("@"))
            {
                // Email-based lookup
                playerProfile = _appDBContext.Users.Where(r => r.Email.ToLower() == contactInfo.ToLower()
                                                 && r.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }
            else
            {
                // Phone-based lookup
                playerProfile = _appDBContext.Users.Where(r => r.PhoneNumber == contactInfo
                                                 && r.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }
            if (playerProfile != null)
                resultPlayerId = playerProfile.PlayerId;
            else
            {
                long tagId = GlobalConstants.PlayerIdTagStarts;
                int opSize = _appDBContext.OperatorData.Count();
                var operatorRecord = _appDBContext.OperatorData.Where(p => p.OperatorId == operatorId).FirstOrDefault();
                if (operatorRecord == null)
                {
                    if (opSize == 0) tagId = GlobalConstants.PlayerIdSegmentSize;
                    else tagId = GlobalConstants.PlayerIdSegmentSize * opSize;
                    tagId += 1;
                    _appDBContext.OperatorData.Add(new OperatorData { OperatorId = operatorId, TagId = tagId});

                }
                else
                {
                    tagId = operatorRecord.TagId + 1;
                    var operatorDataList = _appDBContext.OperatorData.ToList().OrderBy(p => p.TagId);
                    foreach (var operatorItem in operatorDataList)
                    {
                        if (operatorItem.OperatorId == operatorId) continue;
                        else if (tagId < operatorItem.TagId) break;
                        else if (operatorItem.TagId == tagId) tagId = tagId + GlobalConstants.PlayerIdOverFlows;
                    }
                    operatorRecord.TagId = tagId;
                    _appDBContext.Update(operatorRecord);
                }

                resultPlayerId = (int)tagId;
                await _appDBContext.SaveChangesAsync();
            }
            return (resultPlayerId);
        }

        public async Task<AppUser> GetAppUserOnOperator(string email, int operatorId)
        {
            return await _appDBContext.Users.Where(r => r.Email.ToLower() == email.ToLower()
                                             && r.OperatorId == operatorId).FirstOrDefaultAsync();
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

        public async Task<AppUser> GetAppUser(string username)
        {
            return await _appDBContext.Users
                .Where(u => u.UserName.ToLower() == username.ToLower())
                .FirstOrDefaultAsync();
        }

        public IEnumerable<int> GetPlayersOperators(string netUserId)
        {
            return _appDBContext.Users.Where(r => r.Id.Equals(netUserId, StringComparison.InvariantCultureIgnoreCase)).Select(r => r.OperatorId).ToList();
        }


        public bool UserExists(string email, string username, int operatorId)
        {
            var appUser = _appDBContext.Users.Where(r => (r.UserName.ToLower() == username.ToLower() || r.Email.ToLower() == email.ToLower()) && r.OperatorId == operatorId ).FirstOrDefault();
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

        #region Check if Valid Email
        public bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        #endregion


        #region Private Methods
        private AppUser CheckOperatorAssignment(string netUserId, int operatorId)
        {
            return _appDBContext.Users.Where(r => r.Id.Equals(netUserId, StringComparison.InvariantCultureIgnoreCase)
                                                    && r.OperatorId == operatorId).FirstOrDefault();
        }
        #endregion


        #region Get User by OperatorId and PlayerId
        public async Task<AppUser> GetUserByOperatorIdAndPlayerIdAsync(int operatorId, int playerId)
        {
            return await _appDBContext.Users
                .Where(o => o.OperatorId == operatorId && o.PlayerId == playerId)
                .FirstOrDefaultAsync();
        }
        #endregion

        #region Phone Number Lookup Methods
        public async Task<AppUser> GetAppUserByPhoneOnOperator(string phoneNumber, int operatorId)
        {
            return await _appDBContext.Users.Where(r => r.PhoneNumber == phoneNumber
                                             && r.OperatorId == operatorId).FirstOrDefaultAsync();
        }

        public bool PhoneUserExists(string phoneNumber, string username, int operatorId)
        {
            var appUser = _appDBContext.Users.Where(r => (r.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase) || r.PhoneNumber == phoneNumber) && r.OperatorId == operatorId).FirstOrDefault();
            return (appUser == null) ? false : true;
        }
        #endregion
    }
}
