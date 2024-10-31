
namespace XRL.World.Parts
{
	public class PerfectAccuracy : IPart
	{
		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("WeaponMissileWeaponShot");
			base.Register(Object, Registrar);
		}
		public override bool FireEvent(Event E)
		{
			if (E.ID == "WeaponMissileWeaponShot")
			{
				E.SetParameter("AimVariance", 0);
				E.SetParameter("FlatVariance", 0);
				E.SetParameter("WeaponAccuracy", 0);
			}
			return base.FireEvent(E);
		}
	}
}