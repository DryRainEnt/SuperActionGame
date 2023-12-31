using System.Collections.Generic;
using Proto.BasicExtensionUtils;
using UnityEngine;
using SimpleActionFramework.Core;
using SimpleActionFramework.Utility;
using UnityEditor;

[System.Serializable]
public class SetHitMaskActant : SingleActant
{
	HitMask _mask;
	public HitMask Mask => _mask ??= HitMask.Create(MaskType,
		new Bounds(Position, Size), null, new DamageInfo());
	
	public static readonly Dictionary<CombinedIdKey, HitMask> ActiveActors = new Dictionary<CombinedIdKey, HitMask>();

	public MaskType MaskType;
	public Vector2 Position;
	public Vector2 Size;
	public DamageInfo Info;
	
	public readonly Color AttackMaskColor = new Color(1f, 0f, 0f, 0.5f);
	public readonly Color GuardMaskColor = new Color(0f, 1f, 1f, 0.5f);
	public readonly Color HitMaskColor = new Color(0f, 1f, 0f, 0.5f);
	public readonly Color StaticMaskColor = new Color(1f, 1f, 0f, 0.5f);
	public readonly Color DefaultMaskColor = new Color(1f, 1f, 0f, 0.5f);
	
	public Color GetColor => Mask.Type switch
	{
		MaskType.Attack => AttackMaskColor,
		MaskType.Guard => GuardMaskColor,
		MaskType.Hit => HitMaskColor,
		MaskType.Static => StaticMaskColor,
		_ => DefaultMaskColor
	};
	
	public override void Act(Actor actor, float progress, bool isFirstFrame = false)
	{
	 	base.Act(actor, progress, isFirstFrame);
	 	// Put your code here
	}

	public override void OnStart(Actor actor)
	{
		var key = GetId(actor);
		var mask = HitMask.Create(MaskType,
			new Bounds(Position, Size), actor, Info);
		mask.Id = key;
		actor.ActionStateMachine.RegisterDisposable(this, mask);
	}

	public override void OnFinished(Actor actor)
	{
		ResetMask(actor);
	}

	public void ResetMask(Actor actor)
	{
		actor.ActionStateMachine.DisposeDisposable(this);
	}

	public override void CopyFrom(SingleActant actant)
	{
		if (actant is not SetHitMaskActant hit)
			return;
		
		StartFrame = hit.StartFrame;
		Duration = hit.Duration;
		MaskType = hit.MaskType;
		Position = hit.Position;
		Size = hit.Size;
		Info = hit.Info;
	}

	public override void OnGUI(Rect position, float scale, float progress)
	{
	#if UNITY_EDITOR
		_mask?.Dispose();
		_mask = null;
		
		Vector3 pos = position.size / 2;
		
		GUI.color = GetColor;
		GUI.DrawTexture(
			new Rect(pos + ((Mask.Bounds.center.FlipY() - Mask.Bounds.extents) * (scale * 16f)),
				Mask.Bounds.size * (scale * 16f)), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
		GUI.color = Color.white; // GUI 색상을 기본값으로 돌려놓기
	#endif
	}

	public override string ToString()
	{
		return Utils.BuildString(MaskType.ToString(), " Mask");
	}
}
