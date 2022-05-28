using System;
using UnityEngine;
using XRL.UI;

namespace XRL.World.Parts.Mutation
{
	[Serializable]
	public class RitaTentacles : BaseMutation
	{
		public RitaTentacles()
		{
			DisplayName = "Tentacles";
			Type = "Physical";
		}

		public string AdditionsManagerID => ParentObject.id + "::RitaTentacles::Add";

		public GameObject tentacleLauncher;

		public const int GrabCooldown = 25;

		public const int BaseDamageDie = 2;

		public const int BaseTentacleCount = 2;

		public const int BaseTentacleGrabCapacity = 50;

		public const int BaseRange = 8;
		public const int RangePerLevel = 1;

		public Guid TentacleGrabActivatedAbilityID = Guid.Empty;

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "CommandRitaTentacleGrab");
			Object.RegisterPartEvent(this, "GetTentacleGrabCapacity");
			Object.RegisterPartEvent(this, "OnTentacleGrabTooHeavy");
			Object.RegisterPartEvent(this, "OnTentacleGrabCantMove");
			base.Register(Object);
		}

		public override string GetDescription()
		{
			return "You have tentacles growing from your back.";
		}

		public int GetDamageDie(int Level)
		{
			return (int)(BaseDamageDie + Level / 2);
		}
		public int GetDamageDie()
		{
			return GetDamageDie(Level);
		}

		public int GetTentacleCount(int Level)
		{
			return (int)(BaseTentacleCount + (Level - 1) / 2);
		}
		public int GetTentacleCount()
		{
			return GetTentacleCount(Level);
		}

		public int GetRange(int Level)
		{
			return (int)(BaseRange + (Level - 1) * RangePerLevel);
		}
		public int GetRange()
		{
			return GetRange(Level);
		}

		public double GetTentacleGrabFactor(int Level)
		{
			return (double)(Level / 2.0 + 1.5);
		}
		public double GetTentacleGrabFactor()
		{
			return GetTentacleGrabFactor(Level);
		}

		public override string GetLevelText(int Level)
		{
			string text = $"Gain {GetTentacleCount(Level)} (+1 every odd level) tentacles growing from your back. Tentacles are a long-blade class natural weapon.\n";
			text += $"Damage: 1d{GetDamageDie(Level)} (+1 die size every even level).\n";
			text += "Use your tentacles to grab and pull an enemy to your location.\n";
			return text + $"Range: {GetRange(Level)}\nCooldown: {GrabCooldown} turns\nWeight: {GetTentacleGrabFactor(Level)}x your body weight";
		}

		public override bool ChangeLevel(int NewLevel)
		{
			int numTentaclesToAdd = GetTentacleCount(NewLevel) - (LastLevel > 0 ? GetTentacleCount(LastLevel) : 0);
			if (numTentaclesToAdd > 0)
			{
				for (int i = 0; i < numTentaclesToAdd; i++)
				{
					AddMoreTentacles(ParentObject);
				}
			}
			else if (numTentaclesToAdd < 0)
			{
				for (int i = 0; i < -numTentaclesToAdd; i++)
				{
					BodyPart tentacle = ParentObject.GetBodyPartByManager(AdditionsManagerID, true);
					ParentObject.Body.RemovePart(tentacle);
				}
			}
			foreach (BodyPart tentacle in ParentObject.GetBodyPartsByManager(AdditionsManagerID))
			{
				GameObject tentacleBehavior = tentacle.DefaultBehavior;
				tentacleBehavior.GetPart<MeleeWeapon>().BaseDamage = "1d" +GetDamageDie(NewLevel);
			}
			return base.ChangeLevel(NewLevel);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "GetTentacleGrabCapacity")
			{
				E.SetParameter("Capacity", (int) (ParentObject.GetKineticResistance() * GetTentacleGrabFactor()));
			}
			else if (E.ID == "OnTentacleGrabTooHeavy" || E.ID == "OnTentacleGrabCantMove")
			{
				// If the object is too heavy to be grabbed, or is inherently immovable, we don't give a cooldown.
				TakeMyActivatedAbilityOffCooldown(TentacleGrabActivatedAbilityID);
			}
			else if (E.ID == "CommandRitaTentacleGrab")
			{
				if (!IsMyActivatedAbilityUsable(TentacleGrabActivatedAbilityID))
				{
					return false;
				}
				if (ParentObject.OnWorldMap())
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("You cannot do that on the world map.");
					}
					return false;
				}
				Cell TargetCell = ((ParentObject.Target == null || ParentObject.GetTotalConfusion() > 0) ? ParentObject.CurrentCell : ParentObject.Target.CurrentCell);
				FireType firetype = FireType.Normal;
				MissilePath firePath = MissileWeapon.ShowPicker(TargetCell.X, TargetCell.Y, false, AllowVis.Any, GetRange(), false, null, ref firetype);
				if ((firePath?.Path?.Count ?? 0) <= 1 || firePath.Path.Count > GetRange())
				{

					return false;
				}
				PlayWorldSound("hiss_high", 0.5f, 0f, combat: true);
				UseEnergy(1000);
				TargetCell = firePath.Path[firePath.Path.Count - 1];
				Event event2 = Event.New("CommandFireMissile");
				event2.SetParameter("Owner", ParentObject);
				event2.SetParameter("TargetCell", TargetCell);
				event2.SetParameter("Path", firePath);
				event2.SetParameter("FireType", firetype);
				if(!tentacleLauncher.FireEvent(event2))
				{
					return false;
				}
				CooldownMyActivatedAbility(TentacleGrabActivatedAbilityID, GrabCooldown);
			}
			return base.FireEvent(E);
		}

		public void AddMoreTentacles(GameObject GO)
		{
			BodyPart BodyRoot = GO?.Body?.GetBody();
			if (BodyRoot == null)
			{
				return;
			}
			BodyPart ourBack = BodyRoot.GetFirstAttachedPart("Back");
			if (ourBack == null)
			{
				return;
			}
			BodyPart newTentacle = BodyRoot.AddPartAt(ourBack, "RitaTentacle", Manager: AdditionsManagerID);
		}
		public override bool Mutate(GameObject GO, int Level)
		{
			tentacleLauncher = GameObjectFactory.Factory.CreateObject("RitaTentacleLauncher");
			TentacleGrabActivatedAbilityID = AddMyActivatedAbility("Tentacle Grapple", "CommandRitaTentacleGrab", "Physical Mutation");
			return base.Mutate(GO, Level);
		}

		public override bool Unmutate(GameObject GO)
		{
			tentacleLauncher.Obliterate();
			GO.RemoveBodyPartsByManager(AdditionsManagerID);
			RemoveMyActivatedAbility(ref TentacleGrabActivatedAbilityID);
			return base.Unmutate(GO);
		}
	}
}
