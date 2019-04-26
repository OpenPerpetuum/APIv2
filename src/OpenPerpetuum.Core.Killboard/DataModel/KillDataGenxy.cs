using System.Collections.Generic;

namespace OpenPerpetuum.Core.Killboard.DataModel
{
	public class KillDataGenxy
	{
		public int ZoneId
		{
			get;
			set;
		}

		public CharacterDataGenxy Victim
		{
			get;
			set;
		}

		public AttackerDataGenxy[] Attackers
		{
			get;
			set;
		}


	}
}
