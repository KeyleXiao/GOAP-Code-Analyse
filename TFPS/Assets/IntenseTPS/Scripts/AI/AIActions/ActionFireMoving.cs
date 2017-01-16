using Information;
using Sensors;
using StateSystems;
using System.Collections.Generic;
using UnityEngine;

namespace Actions
{
	public class ActionFireMoving : AIAction
	{
		public Vector2 randomFinishTimeBw = new Vector2(2, 6);
		public Vector2 applicapableChanceInBetweenPerc = new Vector2(45, 75); // %
		public Vector2 dirChangeRandTimeMinMax = new Vector2(6f, 9f);
		public Vector2 tryMoveAngleOfTargetHealthMinMax = new Vector2(-25, 25);

		private float _dirChangeTime;
		private float tryMoveAngle;
		private InformationNMCanFirePosition infoMoveSide;
		private float _finishTime;
		private float lastAngle = 0;

		public override void OnStart(AIBrain ai)
		{
			AddNeededStateSystem<AIStateSystemWeapon>();
			AddNeededStateSystem<AIStateSystemAnimator>();
			AddNeededStateSystem<AIStateSystemMove>();
			AddNeededStateSystem<AIStateSystemLookAt>();

			AddNeededSensor<SensorCanFireNMPositionFinder>();

			repeatType = ET.ActionType.Repetitive;

			preConditions.Add(DS.weaponLoaded, true);
			preConditions.Add(DS.weaponArmed, true);
			preConditions.Add(DS.haveTarget, true);
			preConditions.Add(DS.weaponAimed, true);
			preConditions.Add(DS.isNearCurrentTarget, false);

			postEffects.Add(new KeyValuePair<string, object>(DS.killTarget, true));

			correspondingState = ET.StateType.Move;
		}

		public override void Activate(AIBrain ai)
		{
			float sign = Random.value > .5f ? 1 : -1;
			tryMoveAngle = sign;
			tryMoveAngle = 90 * Mathf.Sign(tryMoveAngle) + Mathf.Sign(tryMoveAngle) *
				(tryMoveAngleOfTargetHealthMinMax.x + ai.InfoCurrentTarget.health.Confidence *
				(tryMoveAngleOfTargetHealthMinMax.y - tryMoveAngleOfTargetHealthMinMax.x));

			ai.GetStateSystem<AIStateSystemMove>().SetMoveToPositionNStartMove(ai, ET.MoveType.Walk, ET.MoveToType.ToPosition, infoMoveSide.positionEstimated.Value);

			_dirChangeTime = Random.Range(dirChangeRandTimeMinMax.x, dirChangeRandTimeMinMax.y);
			lastAngle = tryMoveAngle;
			infoMoveSide = GetInfo(ai);

			ai.GetStateSystem<AIStateSystemWeapon>().StartFiring(ai);
			ai.GetStateSystem<AIStateSystemMove>().SetTurnToPosNStartTurn(ai, ET.TurnToType.ToCurrentTarget);
			ai.GetStateSystem<AIStateSystemLookAt>().SetLookAtPosNStartLook(ai, ET.LookAtType.ToCurrentTarget);
			_finishTime = Random.Range(randomFinishTimeBw.x, randomFinishTimeBw.y);
		}

		private InformationNMCanFirePosition GetInfo(AIBrain ai)
		{
			InformationNMCanFirePosition infoRetVal = null;
			List<InformationNMCanFirePosition> infos = ai.GetSensor<SensorCanFireNMPositionFinder>().RequestAllInfo(ai);
			if (infos == null || infos.Count == 0)
				return null;

			float closest = Mathf.Infinity;
			foreach (var info in infos)
				if (Mathf.Abs(tryMoveAngle - info.angle.Value) < closest)
				{
					infoRetVal = info;
					closest = Mathf.Abs(tryMoveAngle - info.angle.Value);
				}

			if (Mathf.Abs(lastAngle - infoRetVal.angle.Value) > 90 || _dirChangeTime < 0)
			{
				tryMoveAngle = -tryMoveAngle;
				lastAngle = tryMoveAngle;
				tryMoveAngle = 90 * Mathf.Sign(tryMoveAngle) + Mathf.Sign(tryMoveAngle) *
				(tryMoveAngleOfTargetHealthMinMax.x + ai.InfoCurrentTarget.health.Confidence * (tryMoveAngleOfTargetHealthMinMax.y - tryMoveAngleOfTargetHealthMinMax.x));
				_dirChangeTime = Random.Range(dirChangeRandTimeMinMax.x, dirChangeRandTimeMinMax.y);
			}

			return infoRetVal;
		}

		public override void OnUpdate(AIBrain ai)
		{
			_finishTime -= Time.deltaTime;
			_dirChangeTime -= Time.deltaTime;
			ai.GetStateSystem<AIStateSystemMove>().SetMoveToPosition(
				ai, ET.MoveType.Walk, ET.MoveToType.ToPosition, infoMoveSide.positionEstimated.Value);
		}

		public override bool IsStillValid(AIBrain ai)
		{
			if (ai.GetStateSystem<AIStateSystemMove>().ReachedDestination(ai))
			{
				infoMoveSide = GetInfo(ai);
				if (infoMoveSide == null)
					return false;
			}

			return true;
		}

		public override bool CanBeAddedToPlan(AIBrain ai)
		{
			if (ai.GetSensor<SensorCanFireNMPositionFinder>().RequestAllInfo(ai) == null)
				return false;

			float chance = 0;
			if (ai.WorldState.CompareKey(DS.takingBulletDamage, true))
				chance = applicapableChanceInBetweenPerc.y;
			else
				chance = applicapableChanceInBetweenPerc.x;
			if (
				Random.Range(0, 100) < chance &&
				ai.HaveCurrentTarget() && ai.HaveCurrentWeapon()
				)
				return true;
			return false;
		}

		public override bool CanActivate(AIBrain ai)
		{
			infoMoveSide = ai.GetSensor<SensorCanFireNMPositionFinder>().RequestInfo(ai);

			if (infoMoveSide != null && ai.HaveCurrentWeapon() && ai.HaveCurrentTarget())
				return true;
			return false;
		}

		public override void DeActivate(AIBrain ai)
		{
			infoMoveSide = null;
			ai.GetStateSystem<AIStateSystemMove>().StopMoving(ai);
			ai.GetStateSystem<AIStateSystemMove>().StopTurning(ai);
			ai.GetStateSystem<AIStateSystemLookAt>().StopLooking(ai);
			ai.GetStateSystem<AIStateSystemWeapon>().StopFiring(ai);
		}

		public override bool IsCompleted(AIBrain ai)
		{
			if (_finishTime < 0)
				return true;
			return false;
		}

		public override void GeneralPostEffects(AIBrain ai)
		{
			infoMoveSide = null;
			ai.GetStateSystem<AIStateSystemMove>().StopMoving(ai);
			ai.GetStateSystem<AIStateSystemMove>().StopTurning(ai);
			ai.GetStateSystem<AIStateSystemLookAt>().StopLooking(ai);
			ai.GetStateSystem<AIStateSystemWeapon>().StopFiring(ai);
		}
	}
}