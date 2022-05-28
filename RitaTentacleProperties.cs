using System;
using System.Text;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts
{
	[Serializable]
	public class RitaTentacleProperties : IPart
	{
        private RitaTentacles _ourMutation;
		public RitaTentacles ourMutation
        {
            get
            {
                if (_ourMutation == null)
                {
                    _ourMutation = ParentObject?.Equipped?.GetPart<Mutations>()?.GetMutation("RitaTentacles") as RitaTentacles;
                }
                return _ourMutation;
            }
        }

		public override bool SameAs(IPart p)
		{
			if ((p as RitaTentacleProperties).ourMutation != ourMutation)
			{
				return false;
			}
			return base.SameAs(p);
		}

		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "QueryWeaponSecondaryAttackChance");
			base.Register(Object);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "QueryWeaponSecondaryAttackChance")
			{
				E.SetParameter("Chance", 20);
			}
			return base.FireEvent(E);
		}
	}
}
