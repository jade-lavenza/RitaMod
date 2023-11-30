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

		public override bool WantEvent(int ID, int cascade)
		{
			if(ID == GetProjectileBlueprintEvent.ID || 
			   ID == GetMissileWeaponProjectileEvent.ID || 
			   ID == LoadAmmoEvent.ID)
			{
				return true;
			}
			return base.WantEvent(ID, cascade);
		}

		public override bool HandleEvent(GetProjectileBlueprintEvent E)
		{
			if (!ProjectileObject.IsNullOrEmpty())
			{
				E.Blueprint = ProjectileObject;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetMissileWeaponProjectileEvent E)
		{
			if (!ProjectileObject.IsNullOrEmpty())
			{
				E.Blueprint = ProjectileObject;
				return false;
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(LoadAmmoEvent E)
		{
			if (!ProjectileObject.IsNullOrEmpty())
			{
				E.Projectile = GameObject.Create(ProjectileObject, Context: "Projectile");
			}
			return base.HandleEvent(E);
		}
	}
}
