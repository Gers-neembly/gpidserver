using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.Persistence.Interfaces;
using Neembly.GPIDServer.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        public async Task<string> CreatePlayerById(string userId, string operatorId, PlayerInfo playerInfo = null)
        {
            var player = _appDBContext.Users.Where(r => r.Id.Equals(userId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (player == null)
                return string.Empty;
            long tagId = 1;
            var operatorRecord = _appDBContext.OperatorData.Where(r => r.OperatorId.Equals(operatorId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (operatorRecord == null)
            {
                _appDBContext.OperatorData.Add(new OperatorData { OperatorId = operatorId, TagId = 1 });
            }
            else
            {
                tagId = operatorRecord.TagId + 1;
                operatorRecord.TagId = tagId;
            }

            string tagFormatted = $"{operatorId}-{tagId:D8}";

            _appDBContext.Players.Add(new Player
            { PlayerId = tagFormatted,
              FirstName = playerInfo == null ? string.Empty : playerInfo.FirstName,
              LastName = playerInfo == null ? string.Empty : playerInfo.LastName
            });
            player.PlayerId = tagFormatted;

            await _appDBContext.SaveChangesAsync();
            return tagFormatted;
        }

        public AppUser GetAppUser(string email, string username, string operatorId)
        {
            var playerInfo =  _appDBContext.Users.Where(r => r.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)
                                                            && r.Email.Equals(username, StringComparison.InvariantCultureIgnoreCase)
                                                            && r.OperatorId.Equals(operatorId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            return playerInfo;
        }


    }
}
