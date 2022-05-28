using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Language;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation
{
	[Serializable]
	public class RitaHivePresence : BaseMutation
	{
		public RitaHivePresence()
		{
			DisplayName = "Hive Presence";
			Type = "Mental";
		}

		public Guid PresenceActivatedAbilityID = Guid.Empty;

		public const int EffectDuration = 100;

		public const int EffectCooldown = 200;

		public const int BaseRange = 4;
		public int GetRange(int Level)
		{
			return (int) BaseRange + Level;
		}
		public int GetRange()
		{
			return GetRange(Level);
		}

		public const int BaseRadius = 2;
		public const int LevelsPerRadius = 3;
		public int GetRadius(int Level)
		{
			return (int) (BaseRadius + Level / LevelsPerRadius);
		}
		public int GetRadius()
		{
			return GetRadius(Level);
		}

		public override string GetDescription()
		{
			return "You command nearby creatures to temporarily fight alongside you.";
		}

		public override string GetLevelText(int Level)
		{
			return "Mental attack versus creatures in radius with minds\n" + "Success roll: {{rules|mutation rank}} or Ego mod (whichever is higher) + character level + 1d8 VS. Defender MA + character level\n" + $"Radius: {GetRadius(Level)} (+1 every {Grammar.Ordinal(LevelsPerRadius)} level)\n" + $"Range: {GetRange(Level)} (+1 every level)\n" + $"Cooldown: {EffectCooldown} rounds\n" + $"Duration: {EffectDuration} rounds";
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "CommandHivePresence");
			base.Register(Object);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "CommandHivePresence")
			{
				if (!IsMyActivatedAbilityUsable(PresenceActivatedAbilityID))
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
				List<Cell> affectedCells = PickCircle(GetRadius(), GetRange(), bLocked: false, AllowVis.OnlyVisible);
				if (affectedCells == null || affectedCells.Count <= 0 || affectedCells[0].DistanceTo(ParentObject) > GetRange())
				{
					return false;
				}
				UseEnergy(1000, "Mental Mutation");
				CooldownMyActivatedAbility(PresenceActivatedAbilityID, EffectCooldown);
				PlayWorldSound("time_dilation", 0.5f, 0f, combat: true);
				XRLCore.ParticleManager.AddRadial("&C@", affectedCells[0].X, affectedCells[0].Y, XRL.Rules.Stat.Random(0, 5), XRL.Rules.Stat.Random(4, 8), 0.01f * (float)XRL.Rules.Stat.Random(3, 5), -0.05f * (float)XRL.Rules.Stat.Random(2, 6));
				int attackModifier = ParentObject.Stat("Level") + Math.Max(ParentObject.StatMod("Ego"), Level);
				int numAttacked = 0;
				foreach (Cell affectedCell in affectedCells)
				{
					foreach (GameObject defender in affectedCell.GetObjectsWithPart("Brain"))
					{
						if (defender == null || defender == ParentObject || !defender.IsValid() || !defender.HasStat("Level"))
						{
							continue;
						}
						numAttacked++;
						Mental.PerformAttack(Presence, ParentObject, defender, Dice: "1d8", Type: 1, DefenseModifier: defender.Stat("Level"));
					}
				}
				if (numAttacked <= 0 && ParentObject.IsPlayer())
				{
					Popup.ShowFail("There are no valid targets in that area!");
				}
			}
			return base.FireEvent(E);
		}

		private bool Presence(MentalAttackEvent E)
		{
			GameObject defender = E.Defender;
			if (E.Penetrations <= 0 || !defender.ApplyEffect(new HivePresence(EffectDuration, E.Attacker)))
			{
				return false;
			}
			return true;
		}

		public override bool Mutate(GameObject GO, int Level)
		{
			PresenceActivatedAbilityID = AddMyActivatedAbility("Hive Presence", "CommandHivePresence", "Mental Mutation");
			return base.Mutate(GO, Level);
		}

		public override bool Unmutate(GameObject GO)
		{
			RemoveMyActivatedAbility(ref PresenceActivatedAbilityID);
			return base.Unmutate(GO);
		}
	}
}