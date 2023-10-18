using System;
using System.Collections.Generic;
using SimpleActionFramework.Core;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DebugDrawer : MonoBehaviour
{
    public List<Dictionary<string, object>> DrawGizmoData = new List<Dictionary<string, object>>();
    
    private Color _color;

	public Material mat;

	private void Awake()
	{
		// Material 생성 및 연결
		if (mat == null)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
			mat = new Material(shader);
		}
	}

    private void OnDestroy()
    {
        Destroy(mat);
        mat = null;
    }

    private void OnPostRender()
	{
		foreach (var mask in MaskManager.HitMaskList)
		{
			var c = mask.Type.GetColor();
			c.a = 0.5f;
			AddDrawFilledBoxGizmo(mask.Bounds, c);
		}

		foreach (var hit in MaskManager.HitDataList)
		{
			AddDrawFilledCircleGizmo((hit.GiverMask.Bounds.center + hit.ReceiverMask.Bounds.center) * 0.5f, 0.3f, Color.magenta);
		}
		
		if (mat != null)
		{
			// mat.SetPass(0);
            
			DrawGizmos();
            
            DrawLineGizmo(Vector3.zero, Vector3.one);
		}
		else
		{
			Debug.LogError("Material for GL drawing is missing!");
		}
	}
    
    #region Adders
    public void AddDrawBoxGizmo(Vector3 center, Vector3 size, Color? c = null)
    {
        var data = new Dictionary<string, object>();
        data.Add("type", GizmoType.Box);
        data.Add("center", center);
        data.Add("size", size);
        if (c != null) data.Add("color", c);
        DrawGizmoData.Add(data);
    }

    public void AddDrawBoxGizmo(Bounds bounds, Color? c = null)
    {
        AddDrawBoxGizmo(bounds.center, bounds.size, c);
    }

    public void AddDrawFilledBoxGizmo(Vector3 center, Vector3 size, Color? c = null)
    {
        var data = new Dictionary<string, object>();
        data.Add("type", GizmoType.FilledBox);
        data.Add("center", center);
        data.Add("size", size);
        if (c != null) data.Add("color", c);
        DrawGizmoData.Add(data);
    }

    public void AddDrawFilledBoxGizmo(Bounds bounds, Color? c = null)
    {
        AddDrawFilledBoxGizmo(bounds.center, bounds.size, c);
    }

    public void AddDrawCircleGizmo(Vector3 center, float radius, Color? c = null)
    {
        var data = new Dictionary<string, object>();
        data.Add("type", GizmoType.Circle);
        data.Add("center", center);
        data.Add("radius", radius);
        if (c != null) data.Add("color", c);
        DrawGizmoData.Add(data);
    }


    public void AddDrawFilledCircleGizmo(Vector3 center, float radius, Color? c = null)
    {
        var data = new Dictionary<string, object>();
        data.Add("type", GizmoType.FilledCircle);
        data.Add("center", center);
        data.Add("radius", radius);
        if (c != null) data.Add("color", c);
        DrawGizmoData.Add(data);
    }

    public void AddDrawLineGizmo(Vector3 from, Vector3 to, Color? c = null)
    {
        var data = new Dictionary<string, object>();
        data.Add("type", GizmoType.Circle);
        data.Add("from", from);
        data.Add("to", to);
        if (c != null) data.Add("color", c);
        DrawGizmoData.Add(data);
    }

    #endregion

    #region Drawers

    private void DrawLineGizmo(Vector3 from, Vector3 to)
    {
        GL.PushMatrix();

        //Change Gizmos feature into GL
        GL.Begin(GL.LINES);
        GL.Color(_color);
        GL.Vertex(from);
        GL.Vertex(to);
        GL.End();
            
        GL.PopMatrix();
    }

    private void DrawCircleGizmo(Vector3 center, float radius)
    {
        GL.PushMatrix();

        //Change Gizmos feature into GL
        GL.Begin(GL.LINES);
        GL.Color(_color);
        for (int i = 0; i <= 360; i += 12)
        {
            var rad = i * Mathf.Deg2Rad;
            var radNext = (i+12) * Mathf.Deg2Rad;
            var x = Mathf.Cos(rad) * radius;
            var y = Mathf.Sin(rad) * radius;
            var xNext = Mathf.Cos(radNext) * radius;
            var yNext = Mathf.Sin(radNext) * radius;
            GL.Vertex(center + new Vector3(x, y, 0));
            GL.Vertex(center + new Vector3(xNext, yNext, 0));
        }
        GL.End();
            
        GL.PopMatrix();
    }

    private void DrawFilledCircleGizmo(Vector3 center, float radius)
    {
        GL.PushMatrix();

        //Change Gizmos feature into GL
        GL.Begin(GL.TRIANGLES);
        GL.Color(_color);
        for (int i = 0; i <= 360; i += 12)
        {
            var rad = i * Mathf.Deg2Rad;
            var radNext = (i+12) * Mathf.Deg2Rad;
            var x = Mathf.Cos(rad) * radius;
            var y = Mathf.Sin(rad) * radius;
            var xNext = Mathf.Cos(radNext) * radius;
            var yNext = Mathf.Sin(radNext) * radius;
            GL.Vertex(center);
            GL.Vertex(center + new Vector3(xNext, yNext, 0));
            GL.Vertex(center + new Vector3(x, y, 0));
        }
        GL.End();
            
        GL.PopMatrix();
    }

    private void DrawBoxGizmo(Vector3 center, Vector3 size)
    {
        GL.PushMatrix();
        
        //Change Gizmos feature into GL
        GL.Begin(GL.LINES);
        GL.Color(_color);
        
        GL.Vertex(center + new Vector3(-size.x / 2, -size.y / 2, 0));
        GL.Vertex(center + new Vector3(size.x / 2, -size.y / 2, 0));
        
        GL.Vertex(center + new Vector3(size.x / 2, -size.y / 2, 0));
        GL.Vertex(center + new Vector3(size.x / 2, size.y / 2, 0));
        
        GL.Vertex(center + new Vector3(size.x / 2, size.y / 2, 0));
        GL.Vertex(center + new Vector3(-size.x / 2, size.y / 2, 0));
        
        GL.Vertex(center + new Vector3(-size.x / 2, size.y / 2, 0));
        GL.Vertex(center + new Vector3(-size.x / 2, -size.y / 2, 0));
        
        GL.End();
            
        GL.PopMatrix();
    }

    private void DrawBoxGizmo(Bounds bounds)
    {
        DrawBoxGizmo(bounds.center, bounds.size);
    }

    private void DrawFilledBoxGizmo(Vector3 center, Vector3 size)
    {
        GL.PushMatrix();
        
        //Change Gizmos feature into GL
        GL.Begin(GL.TRIANGLES);
        GL.Color(_color);
        
        GL.Vertex(center + new Vector3(-size.x / 2, -size.y / 2, 0));
        GL.Vertex(center + new Vector3(size.x / 2, size.y / 2, 0));
        GL.Vertex(center + new Vector3(size.x / 2, -size.y / 2, 0));
        
        GL.Vertex(center + new Vector3(size.x / 2, size.y / 2, 0));
        GL.Vertex(center + new Vector3(-size.x / 2, -size.y / 2, 0));
        GL.Vertex(center + new Vector3(-size.x / 2, size.y / 2, 0));
        
        GL.End();
            
        GL.PopMatrix();
    }

    private void DrawFilledBoxGizmo(Bounds bounds)
    {
        DrawFilledBoxGizmo(bounds.center, bounds.size);
    }

    #endregion
    
    public void DrawGizmos()
    {
        mat.SetPass(0);

        // 요청이 들어온 Gizmo Call Data들을 순회한 뒤 Flush
        foreach (var data in DrawGizmoData)
        {
            if (data["type"] is GizmoType type)
            {
                _color = (Color) data.GetDefault("color", Color.red);
                switch (type)
                {
                    case GizmoType.Box:
                        if (data.GetDefault("center", Vector3.zero) is Vector3 box_center
                            && data.GetDefault("size", Vector3.zero) is Vector3 box_size)
                            DrawBoxGizmo(box_center, box_size);
                        else if (data.GetDefault("bound",
                            new Bounds(Vector3.zero, Vector3.zero)) is Bounds bounds)
                            DrawBoxGizmo(bounds);
                        break;
                    case GizmoType.FilledBox:
                        if (data.GetDefault("center", Vector3.zero) is Vector3 fbox_center
                            && data.GetDefault("size", Vector3.zero) is Vector3 fbox_size)
                            DrawFilledBoxGizmo(fbox_center, fbox_size);
                        else if (data.GetDefault("bound",
                            new Bounds(Vector3.zero, Vector3.zero)) is Bounds bounds)
                            DrawFilledBoxGizmo(bounds);
                        break;
                    case GizmoType.Circle:
                        if (data.GetDefault("center", Vector3.zero) is Vector3 circle_center
                            && data.GetDefault("radius", 0f) is float radius)
                            DrawCircleGizmo(circle_center, radius);
                        break;
                    case GizmoType.FilledCircle:
                        if (data.GetDefault("center", Vector3.zero) is Vector3 fcircle_center
                            && data.GetDefault("radius", 0f) is float fradius)
                            DrawFilledCircleGizmo(fcircle_center, fradius);
                        break;
                    case GizmoType.Line:
                        if (data.GetDefault("from", Vector3.zero) is Vector3 from
                            && data.GetDefault("to", Vector3.zero) is Vector3 to)
                            DrawLineGizmo(from, to);
                        break;
                    default:
                        Debug.LogWarning("This is Not Registered Gizmo Type");
                        break;
                }
            }
        }
        
        DrawGizmoData.Clear();
    }
}

public enum GizmoType
{
    Box,
    FilledBox,
    Circle,
    FilledCircle,
    Line
}

