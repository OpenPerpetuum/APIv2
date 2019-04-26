using OpenPerpetuum.Core.Authorisation.Models;
using OpenPerpetuum.Core.Foundation.Processing;

namespace OpenPerpetuum.Core.Authorisation.Queries
{
    public class GAME_FindUserByAccountIdQuery : IQuery<UserModel>
    {
        public int AccountId
        {
            get;
            set;
        }
    }
}
