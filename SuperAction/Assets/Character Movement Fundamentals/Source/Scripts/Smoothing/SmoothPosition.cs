using System;
using UnityEngine;
using System.Collections;
using Proto.EventSystem;
using Resources.Scripts.Events;
using SimpleActionFramework.Core;
using Sirenix.Utilities;

namespace CMF
{
	//This script smoothes the position of a gameobject;
	public class SmoothPosition : MonoBehaviour, IEventListener
	{
		//The target transform, whose position values will be copied and smoothed;
		public Vector3 defaultPosition;
		public Transform target;
		Transform tr;

		private Actor targetActor;

		public Vector3 MinLimit;
		public Vector3 MaxLimit;

		Vector3 currentPosition;
		
		//Speed that controls how fast the current position will be smoothed toward the target position when 'Lerp' is selected as smoothType;
		public float lerpSpeed = 20f;

		//Time that controls how fast the current position will be smoothed toward the target position when 'SmoothDamp' is selected as smoothType;
		public float smoothDampTime = 0.02f;

		//Whether position values will be extrapolated to compensate for delay caused by smoothing;
		public bool extrapolatePosition = false;

		//'UpdateType' controls whether the smoothing function is called in 'Update' or 'LateUpdate';
		public enum UpdateType
		{
			Update,
			LateUpdate
		}
		public UpdateType updateType;

		//Different smoothtypes use different algorithms to smooth out the target's position; 
		public enum SmoothType
		{
			Lerp,
			SmoothDamp, 
		}

		public SmoothType smoothType;

		//Local position offset at the start of the game;
		Vector3 localPositionOffset;

		Vector3 refVelocity;

		private enum ActiveState
		{
			Default,
			Active,
			Inactive,
		}

		ActiveState activeState = ActiveState.Default;
		
		//Awake;
		void Awake () {
			
			//If no target has been selected, choose this transform's parent as the target;
			if(target == null)
				target = this.transform.parent;

			tr = transform;
			currentPosition = transform.position;
			
			if (target.GetComponent<Actor>() is { } actor)
				targetActor = actor;

			localPositionOffset = tr.localPosition;
		}

		//OnEnable;
		void OnEnable()
		{
			//Reset current position when gameobject is re-enabled to prevent unwanted interpolation from last position;
			ResetCurrentPosition();
		}

		private void Start()
		{
			MessageSystem.Subscribe(typeof(OnDeathEvent), this);
			MessageSystem.Subscribe(typeof(OnReviveEvent), this);
		}

		void Update ()
		{
			if (activeState == ActiveState.Inactive)
				return;
			
			if(updateType == UpdateType.LateUpdate)
				return;
			SmoothUpdate();
		}

		void LateUpdate ()
		{
			if (activeState == ActiveState.Inactive)
				return;
			
			if(updateType == UpdateType.Update)
				return;
			SmoothUpdate();
		}

		void SmoothUpdate()
		{
			var targetPosition = activeState switch
			{
				ActiveState.Default => defaultPosition,
				ActiveState.Inactive => currentPosition,
				_ => target.position
			};
			//Smooth current position;
			currentPosition = Smooth (currentPosition, targetPosition, lerpSpeed).Clamp(MinLimit, MaxLimit);

			//Set position;
			tr.position = currentPosition;
		}

		Vector3 Smooth(Vector3 _start, Vector3 _target, float _smoothTime)
		{
			//Convert local position offset to world coordinates;
			Vector3 _offset = tr.localToWorldMatrix * localPositionOffset;

			//If 'extrapolateRotation' is set to 'true', calculate a new target position;
			if (extrapolatePosition) {
				Vector3 difference = _target - (_start - _offset);
				_target += difference;
			}

			//Add local position offset to target;
			_target += _offset;

			//Smooth (based on chosen smoothType) and return position;
			switch (smoothType)
			{
				case SmoothType.Lerp:
					return Vector3.Lerp (_start, _target, Time.deltaTime * _smoothTime);
				case SmoothType.SmoothDamp:
					return Vector3.SmoothDamp (_start, _target, ref refVelocity, smoothDampTime);
				default:
					return Vector3.zero;
			}
		}

		//Reset stored position and move this gameobject directly to the target's position;
		//Call this function if the target has just been moved a larger distance and no interpolation should take place (teleporting);
		public void ResetCurrentPosition()
		{
			//Convert local position offset to world coordinates;
			Vector3 _offset = tr.localToWorldMatrix * localPositionOffset;
			//Add position offset and set current position;
			currentPosition = target.position + _offset;
		}

		public bool OnEvent(IEvent e)
		{
			if (e is OnDeathEvent de)
			{
				if (targetActor && targetActor.ActorIndex == de.ActorIndex)
				{
					activeState = ActiveState.Inactive;
					return true;
				}
			}
			if (e is OnReviveEvent re)
			{
				if (targetActor && targetActor.ActorIndex == re.ActorIndex)
				{
					activeState = ActiveState.Active;
					return true;
				}
			}
			return false;
		}
	}
}