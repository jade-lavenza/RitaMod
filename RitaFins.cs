using System;
using XRL.Language;

namespace XRL.World.Parts.Mutation
{
	[Serializable]
	public class RitaFins : BaseMutation
	{
		public RitaFins()
		{
			DisplayName = "Fins";
			Type = "Physical";
		}
		
        public int BaseSwimmingBonus = 100;
        public int SwimmingBonusPerLevel = 50;
        public int BaseWadingBonus = 50;
        public int WadingBonusPerLevel = 25;
        public int BaseDodgeBonus = 0;
        public int DodgeBonusLevelsPer = 3;

        public virtual int GetDodgeBonus(int NewLevel)
        {
            return (int)(BaseDodgeBonus + (NewLevel / DodgeBonusLevelsPer));
        }

        public virtual int GetDodgeBonus()
        {
            return GetDodgeBonus(Level);
        }

        public virtual int GetSwimmingBonus(int NewLevel)
        {
            return (int)(BaseSwimmingBonus + (NewLevel * SwimmingBonusPerLevel));
        }

        public virtual int GetSwimmingBonus()
        {
            return GetSwimmingBonus(Level);
        }

        public virtual int GetWadingBonus(int NewLevel)
        {
            return (int)(BaseWadingBonus + (NewLevel * WadingBonusPerLevel));
        }

        public virtual int GetWadingBonus()
        {
            return GetWadingBonus(Level);
        }

        public override string GetDescription()
        {
            return "Your body is streamlined for the water.";
        }

		public override string GetLevelText(int Level)
		{
			return $"Gain +{GetSwimmingBonus(Level)} movespeed while swimming (+{SwimmingBonusPerLevel} swimming movespeed per level), +{GetWadingBonus(Level)} movespeed while wading (+{WadingBonusPerLevel} wading movespeed per level), and +{GetDodgeBonus(Level)} DV (+1 every {Grammar.Ordinal(DodgeBonusLevelsPer)} level). Never slip in liquids.";
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade))
			{
				return ID == GetSwimmingPerformanceEvent.ID || ID == GetWadingPerformanceEvent.ID;
			}
			return true;
		}

		public override bool HandleEvent(GetSwimmingPerformanceEvent E)
		{
			E.MoveSpeedPenalty -= GetSwimmingBonus();
			return base.HandleEvent(E);
		}

        public override bool HandleEvent(GetWadingPerformanceEvent E)
        {
            E.MoveSpeedPenalty -= GetSwimmingBonus();
            return base.HandleEvent(E);
        }

		public override bool Mutate(GameObject GO, int Level)
		{
			GO.Slimewalking = true;
			return base.Mutate(GO, Level);
		}

		public override bool Unmutate(GameObject GO)
		{
			GO.Slimewalking = false;
			return base.Unmutate(GO);
		}
	}
}
