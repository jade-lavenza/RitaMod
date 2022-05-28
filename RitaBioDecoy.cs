using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation
{
	[Serializable]
	public class RitaBioDecoy : BaseMutation
	{
		public RitaBioDecoy()
		{
			DisplayName = "Bio-Decoy";
			Type = "Physical";
		}

		public Guid DecoyActivatedAbilityID = Guid.Empty;

		[NonSerialized]
		public List<GameObject> Decoys = new List<GameObject>();

		public int TurnsRemaining = 0;

		public const int EffectCooldown = 200;

		public const int BaseDecoys = 1;
		public const int LevelsPerDecoy = 3;
		public int GetMaxDecoys(int Level)
		{
			return (int) BaseDecoys + Level/LevelsPerDecoy;
		}
		public int GetMaxDecoys()
		{
			return GetMaxDecoys(Level);
		}

		public const int BaseRange = 4;
		public const int RangePerLevel = 1;
		public int GetMaxRange(int Level)
		{
			return BaseRange + Level;
		}
		public int GetMaxRange()
		{
			return GetMaxRange(Level);
		}

		public const int BaseDuration = 30;
		public const int DurationPerLevel = 5;
		public int GetMaxDuration(int Level)
		{
			return BaseDuration + Level * DurationPerLevel;
		}
		public int GetMaxDuration()
		{
			return GetMaxDuration(Level);
		}

		public override void SaveData(SerializationWriter Writer)
		{
			base.SaveData(Writer);
			Writer.WriteGameObjectList(Decoys);
		}

		public override void LoadData(SerializationReader Reader)
		{
			base.LoadData(Reader);
			Reader.ReadGameObjectList(Decoys);
		}

		public void PlaceDecoy(Cell C)
		{
			GameObject gameObject = GameObject.create("Bio-Decoy");
			gameObject.pRender.Tile = ParentObject.pRender.Tile;
			gameObject.pRender.RenderString = ParentObject.pRender.RenderString;
			gameObject.pRender.DisplayName = ParentObject.pRender.DisplayName;
			Distraction part = gameObject.GetPart<Distraction>();
			part.DistractionFor = ParentObject;
			part.DistractionGeneratedBy = ParentObject;
			string text = ParentObject.a + ParentObject.DisplayNameOnly;
			gameObject.GetPart<Description>().Short = "A facsimile of " + text + ", made out of malleable flesh.";
			gameObject.SetStringProperty("DecoyOf", text);
			C.AddObject(gameObject);
			Decoys.Add(gameObject);
			BodyPart bodyPart = ParentObject.Body?.GetFirstPart("Back");
			if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage($"A grotesque protuberance swells from you{(bodyPart != null ? "r " + bodyPart.GetOrdinalName() : "")} and forms into a copy of {(ParentObject.IsPlayer() ? "you" : text )}!");
			}
			else if (ParentObject.IsVisible())
			{
				if (bodyPart != null)
				{
					IComponent<GameObject>.AddPlayerMessage("A grotesque protuberance swells from " + Grammar.MakePossessive(ParentObject.the + ParentObject.ShortDisplayName) + " " + bodyPart.GetOrdinalName() + " and forms into a copy of " + text + "!");
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage("A grotesque protuberance swells from " + ParentObject.the + ParentObject.ShortDisplayName + " and forms into a copy of " + text + "!");
				}
			}
		}

		public bool CreateDecoys()
		{
			if (Decoys.Count > 0)
			{
				DestroyDecoys(null);
			}
			if (ParentObject.IsPlayer())
			{
				int maxRange = GetMaxRange();
				int activeDecoys = 0;
				while (activeDecoys < GetMaxDecoys())
				{
					Cell cell = ParentObject.pPhysics.PickDestinationCell(maxRange, AllowVis.OnlyVisible, Locked: false);
					if (cell == null)
					{
						return false;
					}
					if (ParentObject.DistanceTo(cell) > maxRange)
					{
						Popup.Show($"That is out of range ({maxRange} squares).");
						continue;
					}
					PlaceDecoy(cell);
					activeDecoys++;
				}
			}
			else
			{
				List<Cell> list = ParentObject.CurrentCell.GetAdjacentCells(2).ShuffleInPlace();
				for (int i = 0; i < GetMaxDecoys() && i < list.Count; i++)
				{
					if (list[i].IsEmpty())
					{
						PlaceDecoy(list[i]);
					}
				}
			}
			return true;
		}

		public void DestroyDecoys(GameObject Decoy = null)
		{
			string endText = $" is absorbed back into {(ParentObject.IsPlayer() ? ParentObject.the + ParentObject.DisplayNameOnly : "you")}.";
			if (Decoy != null)
			{
				if (!Decoy.IsInvalid())
				{
					MessageQueue.AddPlayerMessage("A copy of " + Decoy.GetStringProperty("DecoyOf") + endText);
					Decoy.Destroy();
				}
				Decoys.Remove(Decoy);
			}
			else
			{
				for (int num = Decoys.Count - 1; num >= 0; num--)
				{
					Decoy = Decoys[num];
					if (!Decoy.IsInvalid())
					{
						MessageQueue.AddPlayerMessage("A copy of " + Decoy.GetStringProperty("DecoyOf") + endText);
						Decoy.Destroy();
					}
					Decoys.RemoveAt(num);
				}
			}
		}

		public override string GetDescription()
		{
			return "You form false copies of yourself to distract enemies.";
		}

		public override string GetLevelText(int Level)
		{
			string levelText = "Form false copies of yourself to distract enemies.\n";
			levelText += $"Max decoys: {GetMaxDecoys(Level)} (+1 per {Grammar.Ordinal(LevelsPerDecoy)} level)\n";
			levelText += $"Max range: {GetMaxRange(Level)} squares (+{RangePerLevel} per level)\n";
			levelText += $"Duration: {GetMaxDuration(Level)} turns (+{DurationPerLevel} per level)\n";
			levelText += $"Cooldown: {EffectCooldown} turns\n";
			return levelText;
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "BeginTakeAction");
			Object.RegisterPartEvent(this, "AfterMoved");
			Object.RegisterPartEvent(this, "CommandBioDecoy");
			base.Register(Object);
		}

        public override bool FireEvent(Event E)
        {
			if (E.ID == "BeginTakeAction" && (Decoys.Count > 0))
			{
				if (TurnsRemaining > 0)
				{
					TurnsRemaining--;
				}
				else
				{
					DestroyDecoys();
				}
			}
            else if (E.ID == "CommandBioDecoy")
            {
				if (!IsMyActivatedAbilityUsable(DecoyActivatedAbilityID))
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
				CreateDecoys();
				UseEnergy(1000, "Physical Mutation");
				CooldownMyActivatedAbility(DecoyActivatedAbilityID, EffectCooldown);
				TurnsRemaining = GetMaxDuration();
            }
			else if (E.ID == "AfterMoved")
				for (int i = 0; i < Decoys.Count; i++)
				{
					GameObject currentDecoy = Decoys[i];
					if (currentDecoy.IsInvalid())
					{
						DestroyDecoys(currentDecoy);
					}
					else
					{
						currentDecoy.Move(Directions.GetOppositeDirection(E.GetStringParameter("Direction")), Forced: true);
					}
				}
            return base.FireEvent(E);
        }

		public override bool Mutate(GameObject GO, int Level)
		{
			DecoyActivatedAbilityID = AddMyActivatedAbility("Create Bio-Decoy", "CommandBioDecoy", "Physical Mutation");
			return base.Mutate(GO, Level);
		}

		public override bool Unmutate(GameObject GO)
		{
			RemoveMyActivatedAbility(ref DecoyActivatedAbilityID);
			return base.Unmutate(GO);
		}
	}
}