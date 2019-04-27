using OpenPerpetuum.Core.Definition.DataModel;
using OpenPerpetuum.Core.Foundation.Processing;

namespace OpenPerpetuum.Core.Definition.Queries
{
    public class GAME_FindRobotEntityDefaultsByDefinitionQuery : IQuery<BasicRobotDataModel>
    {
        public int DefinitionId { get; set; }
    }
}
