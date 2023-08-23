using Proto.BasicExtensionUtils;
using UnityEngine;
using SimpleActionFramework.Core;
using UnityEditor;

[System.Serializable]
public class SetHitMaskActant : SingleActant
{
	HitMask _mask;
	public HitMask Mask => _mask ??= HitMask.Create(MaskType,
		new Bounds(Position, Size), null);

	public MaskType MaskType;
	public Vector2 Position;
	public Vector2 Size;
	
	public readonly Color AttackMaskColor = new Color(1f, 0f, 0f, 0.5f);
	public readonly Color GuardMaskColor = new Color(0f, 1f, 1f, 0.5f);
	public readonly Color HitMaskColor = new Color(0f, 1f, 0f, 0.5f);
	
	public Color GetColor => Mask.Type switch
	{
		MaskType.Attack => AttackMaskColor,
		MaskType.Guard => GuardMaskColor,
		MaskType.Hit => HitMaskColor,
		_ => Color.white
	};
	
	public override void Act(Actor actor, float progress, bool isFirstFrame = false)
	{
	 	base.Act(actor, progress, isFirstFrame);
	 	// Put your code here
	}

	public override void OnStart()
	{
		_mask?.Dispose();
		_mask = HitMask.Create(MaskType,
			new Bounds(Position, Size), Machine.Actor);
	}

	public override void OnFinished()
	{
		_mask?.Dispose();
		_mask = null;
	}

	public void ResetMask()
	{
		_mask?.Dispose();
		_mask = null;
	}
	
	public override void OnGUI(Rect position, float scale, float progress)
	{
		ResetMask();
		
		Vector3 pos = position.size / 2;
		
		GUI.color = GetColor;
		GUI.DrawTexture(
			new Rect(pos + ((Mask.Bounds.center.FlipY() - Mask.Bounds.extents) * (scale * 16f)),
				Mask.Bounds.size * (scale * 16f)), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
		GUI.color = Color.white; // GUI 색상을 기본값으로 돌려놓기
	}

	public override void OnDrawGizmos()
	{
		if (_mask is null)
			return;
		
		Gizmos.color = GetColor;
		var center = Mask.Bounds.center;
		Gizmos.DrawCube(center, Mask.Bounds.size);
	}
}
