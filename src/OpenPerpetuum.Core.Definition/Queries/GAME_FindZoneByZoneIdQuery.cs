using OpenPerpetuum.Core.Definition.DataModel;
using OpenPerpetuum.Core.Foundation.Processing;

namespace OpenPerpetuum.Core.Definition.Queries
{
    public class GAME_FindZoneByZoneIdQuery : IQuery<ZoneDataModel>
    {
        public int ZoneId { get; set; }
    }
}
