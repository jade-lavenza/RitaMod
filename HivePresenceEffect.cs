using System;
using UnityEngine;
using XRL.Core;

namespace XRL.World.Effects
{
	[Serializable]
	public class HivePresence : Effect
	{
		public GameObject ourQueen;

		public HivePresence()
		{
			base.DisplayName = "{{cyan|hive-influenced}}";
		}

		public HivePresence(int Duration)
			: this()
		{
			base.Duration = Duration;
		}

		public HivePresence(int Duration, GameObject newQueen)
			: this(Duration)
		{
			this.ourQueen = newQueen;
		}

		public override bool UseStandardDurationCountdown()
		{
			return true;
		}

		public override void Register(GameObject Object)
		{
			Object.RegisterEffectEvent(this, "GetFeeling");
			Object.RegisterEffectEvent(this, "DefenderAfterAttack");
			base.Register(Object);
		}

		public override void Unregister(GameObject Object)
		{
			Object.UnregisterEffectEvent(this, "GetFeeling");
			Object.UnregisterEffectEvent(this, "DefenderAfterAttack");
			base.Unregister(Object);
		}

		public override bool Apply(GameObject Object)
		{
			if (Object.HasEffect<HivePresence>())
			{
				return false;
			}
			ourQueen.StopFighting(Object);
			Object.StopFighting(ourQueen);
			return true;
		}

		public override string GetDetails()
		{
			return "Bound to the will of a hive monarch.";
		}

		public override int GetEffectType()
		{
			return TYPE_MENTAL;
		}

		public override bool FireEvent(Event E)
		{
			if (Duration <= 0 || ourQueen == null)
			{
				return true;
			}
			if (E.ID == "GetFeeling" && E.HasObjectParameter("Who"))
			{
				// We should feel the same way towards an enemy as they feel towards our Queen.
				// If we already feel that way, we feel whichever is stronger, our current feelings
				// or the enemy's feeling toward our Queen.
				int oldFeeling = E.GetIntParameter("Feeling");
				GameObject who = E.GetGameObjectParameter("Who");
				if (who.pBrain == null)
				{
					return true;
				}
				int newFeeling = who.pBrain.GetFeeling(ourQueen);
				if (Math.Sign(oldFeeling) == Math.Sign(newFeeling))
				{
					newFeeling = Math.Sign(newFeeling) * Math.Max(Math.Abs(oldFeeling), Math.Abs(newFeeling));
				}
				E.SetParameter("Feeling", newFeeling);
			}
			else if (E.ID == "DefenderAfterAttack")
			{
				GameObject attacker = E.GetGameObjectParameter("Attacker");
				if (attacker != null && attacker == ourQueen)
				{
					// We've been betrayed by our Queen!
					// End the effect and become hostile.
					Duration = 0;
					GameObject oldQueen = ourQueen;
					ourQueen = null;
					Object.pBrain.WantToKill(oldQueen, "because I was betrayed");
					Object.RemoveEffect(this);
				}
			}
			return base.FireEvent(E);
		}

		public override bool Render(RenderEvent E)
		{
			if (base.Duration > 0)
			{
				int num = XRLCore.CurrentFrame % 60;
				if (num > 5 && num < 10)
				{
					E.Tile = null;
					E.RenderString = "\u0003";
					E.ColorString += "&C";
				}
			}
			return true;
		}
	}
}