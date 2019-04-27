using OpenPerpetuum.Core.Foundation.Types;

namespace OpenPerpetuum.Api.Models
{
    public class BasicCharacterViewModel
    {
        public int CharacterId { get; set; }
        public string Name { get; set; }
        public string Corporation { get; set; }
        public BasicRobotViewModel Robot { get; set; }
        public Position Location { get; set; }
    }
}
