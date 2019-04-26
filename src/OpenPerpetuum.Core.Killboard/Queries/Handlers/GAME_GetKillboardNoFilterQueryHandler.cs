using Microsoft.Extensions.Options;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.Genxy;
using OpenPerpetuum.Core.Killboard.DataModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenPerpetuum.Core.Killboard.Queries.Handlers
{
    internal class GAME_GetKillboardNoFilterQueryHandler : IQueryHandler<GAME_GetKillboardNoFilterQuery, ReadOnlyCollection<KillDataGenxy>>
    {
        private readonly IGenxyReader genxyReader;
        private readonly TestData testData;

        public GAME_GetKillboardNoFilterQueryHandler(IGenxyReader genxyReader, IOptionsSnapshot<TestData> testData)
        {
            this.genxyReader = genxyReader;
            this.testData = testData.Value;
        }

        public ReadOnlyCollection<KillDataGenxy> Handle(GAME_GetKillboardNoFilterQuery query)
        {
            if (testData != null)
            {
                if (!string.IsNullOrWhiteSpace(testData.TestKillData))
                {
                    var killData = genxyReader.Deserialise<KillDataGenxy>(testData.TestKillData);
                    return new List<KillDataGenxy> { killData }.AsReadOnly();
                }
            }

            return new List<KillDataGenxy>().AsReadOnly();
        }
    }
}
