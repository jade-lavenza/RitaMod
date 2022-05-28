using System;

namespace XRL.World.Parts
{
	[Serializable]
	public class InfiniteAmmoLoader : IPart
	{

		public string ProjectileObject;

		public override bool SameAs(IPart p)
		{
			InfiniteAmmoLoader infiniteAmmoLoader = p as InfiniteAmmoLoader;
			if (infiniteAmmoLoader.ProjectileObject != ProjectileObject)
			{
				return false;
			}
			return base.SameAs(p);
		}

		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "LoadAmmo");
			base.Register(Object);
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if(ID == GetProjectileBlueprintEvent.ID)
			{
				return true;
			}
			return base.WantEvent(ID, cascade);
		}

		public override bool HandleEvent(GetProjectileBlueprintEvent E)
		{
			if (!string.IsNullOrEmpty(ProjectileObject))
			{
				E.Blueprint = ProjectileObject;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetMissileWeaponProjectileEvent E)
		{
			if (!string.IsNullOrEmpty(ProjectileObject))
			{
				E.Blueprint = ProjectileObject;
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "LoadAmmo")
			{
				if (ProjectileObject != null)
				{
					E.SetParameter("Ammo", GameObject.create(ProjectileObject));
				}
			}
			return base.FireEvent(E);
		}
	}
}
