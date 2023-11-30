using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Core;
using XRL.Messages;
using XRL.UI;

namespace XRL.World.Parts
{
	[Serializable]
	public class HookOnProjectileHit : IPart
	{
		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "ProjectileHit");
			base.Register(Object);
		}

		public override bool FireEvent(Event E)
		{
			if (E.ID == "ProjectileHit")
			{
				GameObject attacker = E.GetGameObjectParameter("Attacker");
				GameObject defender = E.GetParameter("Defender") as GameObject;
                Cell attackerCell = attacker.CurrentCell;
                Cell defenderCell = defender.CurrentCell;
				if (!Physics.IsMoveable(defender))
                {
                    attacker.FireEvent(Event.New("OnTentacleGrabCantMove"));
                    MessageQueue.AddPlayerMessage($"You can't grab {(defender.UseBareIndicative ? defender.indicativeDistal : null)} {defender.ShortDisplayName}.");
                    return true;
                }
                List<Tuple<Cell, char>> lineTo = attacker.GetLineTo(defender);
				if (lineTo[0].Item1 != attackerCell)
				{
					lineTo.Reverse();
				}
				ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
				string text = "&c";
                double angleToDefender = Math.Atan2((double)defenderCell.Y - attackerCell.Y, (double)defenderCell.X - attackerCell.X);
                string head = "&c";
                switch (angleToDefender / (2 * Math.PI)) // switch head to closest of V<>A
                {
                    case double n when (n <= -0.25):
                        head += "<";
                        break;
                    case double n when (n > -0.25 && n < 0.5):
                        head += "V";
                        break;
                    case double n when (n > 0.5 && n < 0.75):
                        head += ">";
                        break;
                    case double n when (n >= 0.5):
                        head += "A";
                        break;
                }
				text = ((attackerCell.X == defenderCell.X) ? (text + "|") : ((attackerCell.Y == defenderCell.Y) ? (text + "-") : ((attackerCell.Y < defenderCell.Y) ? ((attackerCell.X <= defenderCell.X) ? (text + "\\") : (text + "/")) : ((attackerCell.X <= defenderCell.X) ? (text + "/") : (text + "\\")))));
				int tilesMoved = 0;
                Debug.LogWarning("Firing GetTentacleGrabCapacity event");
                Event eTentacleGrabCapacity = Event.New("GetTentacleGrabCapacity", "Capacity", 0);
                if(!attacker.FireEvent(eTentacleGrabCapacity))
                {
                    Debug.LogError("GetTentacleGrabCapacity event did not fire");
                    return true;
                }
                Debug.LogWarning($"GetTentacleGrabCapacity event fired, result: {eTentacleGrabCapacity.GetParameter("Capacity")}");
                int MaxGrabWeight = eTentacleGrabCapacity.GetParameter<int>("Capacity");
                bool tooHeavy = MaxGrabWeight / Math.Max(defender.GetKineticResistance(), 1) < 1;
				Cell lastCell = defender.CurrentCell;
                for (int currentTile = lineTo.Count - 2; currentTile >= 1 && defender.CurrentCell == lastCell; currentTile--)
                {
					Cell nextCell = attackerCell.ParentZone.GetCell(lineTo[currentTile].Item1.X, lineTo[currentTile].Item1.Y);
					if (nextCell == null || !nextCell.IsAdjacentTo(lastCell))
					{
						break;
					}
					string directionFromCell = lastCell.GetDirectionFromCell(nextCell);
					if (!tooHeavy && !defender.Move(directionFromCell, Forced: true, System: false, IgnoreGravity: true, NoStack: false, AllowDashing: true, DoConfirmations: true, attacker))
					{
						break;
					}
					lastCell = nextCell;
					tilesMoved++;
					bool shouldRedraw = false;
					scrapBuffer.RenderBase();
					for (int j = 1; j < currentTile - 1; j++)
					{
						if (lineTo[j].Item1.IsVisible())
						{
							scrapBuffer.Goto(lineTo[j].Item1.X, lineTo[j].Item1.Y);
                            if (j == lineTo.Count - 2)
                            {
                                scrapBuffer.Write(head);
                            } else {
							    scrapBuffer.Write(text);
                            }
							shouldRedraw = true;
						}
					}
					if (shouldRedraw)
					{
						scrapBuffer.Draw();
						Thread.Sleep(75);
					}
				}
                if (tooHeavy)
                {
                    attacker.FireEvent(Event.New("OnTentacleGrabTooHeavy"));
                    XDidYToZ(attacker, "try", "to grab", defender, $"with {attacker.its} tentacle, but {defender.it} didn't budge!", ColorAsGoodFor: defender, ColorAsBadFor: attacker);
                } else {
                    XDidYToZ(attacker, "pull", defender, $"closer with {attacker.its} tentacle", ColorAsGoodFor: attacker, ColorAsBadFor: defender);
                    defender.Gravitate(); // so they fall if they're over a pit
                }
			}
			return base.FireEvent(E);
		}
	}
}
