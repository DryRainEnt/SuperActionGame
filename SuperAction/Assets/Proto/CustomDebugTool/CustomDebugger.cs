using System;
using System.Collections.Generic;
using UnityEngine;
using Proto.BasicExtensionUtils;
using UnityEngine.UI;

namespace Proto.CustomDebugTool
{
    public enum GizmoType
    {
        Box,
        FilledBox,
        Circle,
        FilledCircle,
        Line
    }

    public static class CustomDebugger
    {
        public static List<Dictionary<string, object>> DrawGizmoData = new List<Dictionary<string, object>>();
        
        public static Material mat;

        public static bool IsGizmoMode = false;

        public static void CreateMaterial()
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things. In this case, we just want to use
            // a blend mode that inverts destination colors.
            var shader = Shader.Find("Hidden/Internal-Colored");
            mat = new Material(shader);
            mat.hideFlags = HideFlags.HideAndDontSave;
            // Set blend mode to invert destination colors.
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusDstColor);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            // Turn off backface culling, depth writes, depth test.
            mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            mat.SetInt("_ZWrite", 0);
            mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        }
        
        #region Adders
        public static void AddDrawBoxGizmo(Vector3 center, Vector3 size, Color? c = null)
        {
            var data = new Dictionary<string, object>();
            data.Add("type", GizmoType.Box);
            data.Add("center", center);
            data.Add("size", size);
            if (c != null) data.Add("color", c);
            DrawGizmoData.Add(data);
        }

        public static void AddDrawBoxGizmo(Bounds bounds, Color? c = null)
        {
            AddDrawBoxGizmo(bounds.center, bounds.size, c);
        }

        public static void AddDrawFilledBoxGizmo(Vector3 center, Vector3 size, Color? c = null)
        {
            var data = new Dictionary<string, object>();
            data.Add("type", GizmoType.FilledBox);
            data.Add("center", center);
            data.Add("size", size);
            if (c != null) data.Add("color", c);
            DrawGizmoData.Add(data);
        }

        public static void AddDrawFilledBoxGizmo(Bounds bounds, Color? c = null)
        {
            AddDrawFilledBoxGizmo(bounds.center, bounds.size, c);
        }

        public static void AddDrawCircleGizmo(Vector3 center, float radius, Color? c = null)
        {
            var data = new Dictionary<string, object>();
            data.Add("type", GizmoType.Circle);
            data.Add("center", center);
            data.Add("radius", radius);
            if (c != null) data.Add("color", c);
            DrawGizmoData.Add(data);
        }


        public static void AddDrawFilledCircleGizmo(Vector3 center, float radius, Color? c = null)
        {
            var data = new Dictionary<string, object>();
            data.Add("type", GizmoType.FilledCircle);
            data.Add("center", center);
            data.Add("radius", radius);
            if (c != null) data.Add("color", c);
            DrawGizmoData.Add(data);
        }

        public static void AddDrawLineGizmo(Vector3 from, Vector3 to, Color? c = null)
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

        private static void DrawLineGizmo(Vector3 from, Vector3 to)
        {
            if (IsGizmoMode)
                Gizmos.DrawLine(from, to);
            
            //Change Gizmos feature into GL
            GL.Begin(GL.LINES);
            GL.Color(Gizmos.color);
            GL.Vertex(from);
            GL.Vertex(to);
            GL.End();
        }

        private static void DrawCircleGizmo(Vector3 center, float radius)
        {
            if (IsGizmoMode)
                Gizmos.DrawWireSphere(center, radius);
            
            //Change Gizmos feature into GL
            GL.Begin(GL.LINES);
            GL.Color(Gizmos.color);
            for (int i = 0; i <= 360; i += 12)
            {
                var rad = i * Mathf.Deg2Rad;
                var x = Mathf.Cos(rad) * radius;
                var y = Mathf.Sin(rad) * radius;
                GL.Vertex(center + new Vector3(x, y, 0));
            }
            GL.End();
        }

        private static void DrawFilledCircleGizmo(Vector3 center, float radius)
        {
            if (IsGizmoMode)
                Gizmos.DrawSphere(center, radius);
            
            //Change Gizmos feature into GL
            GL.Begin(GL.TRIANGLES);
            GL.Color(Gizmos.color);
            for (int i = 0; i <= 360; i += 12)
            {
                var rad = i * Mathf.Deg2Rad;
                var x = Mathf.Cos(rad) * radius;
                var y = Mathf.Sin(rad) * radius;
                GL.Vertex(center + new Vector3(x, y, 0));
            }
            GL.End();
        }

        private static void DrawBoxGizmo(Vector3 center, Vector3 size)
        {
            if (IsGizmoMode)
                Gizmos.DrawWireCube(center, size);
            
            //Change Gizmos feature into GL
            GL.Begin(GL.LINES);
            GL.Color(Gizmos.color);
            GL.Vertex(center + new Vector3(-size.x / 2, -size.y / 2, 0));
            GL.Vertex(center + new Vector3(size.x / 2, -size.y / 2, 0));
            GL.Vertex(center + new Vector3(size.x / 2, -size.y / 2, 0));
            GL.Vertex(center + new Vector3(size.x / 2, size.y / 2, 0));
            GL.Vertex(center + new Vector3(size.x / 2, size.y / 2, 0));
            GL.Vertex(center + new Vector3(-size.x / 2, size.y / 2, 0));
            GL.Vertex(center + new Vector3(-size.x / 2, size.y / 2, 0));
            GL.Vertex(center + new Vector3(-size.x / 2, -size.y / 2, 0));
            GL.End();
        }

        private static void DrawBoxGizmo(Bounds bounds)
        {
            DrawBoxGizmo(bounds.center, bounds.size);
        }

        private static void DrawFilledBoxGizmo(Vector3 center, Vector3 size)
        {
            if (IsGizmoMode)
                Gizmos.DrawCube(center, size);
            
            //Change Gizmos feature into GL
            GL.Begin(GL.TRIANGLES);
            GL.Color(Gizmos.color);
            GL.Vertex(center + new Vector3(-size.x / 2, -size.y / 2, 0));
            GL.Vertex(center + new Vector3(size.x / 2, -size.y / 2, 0));
            GL.Vertex(center + new Vector3(size.x / 2, -size.y / 2, 0));
            GL.Vertex(center + new Vector3(size.x / 2, size.y / 2, 0));
            GL.Vertex(center + new Vector3(size.x / 2, size.y / 2, 0));
            GL.Vertex(center + new Vector3(-size.x / 2, size.y / 2, 0));
            GL.Vertex(center + new Vector3(-size.x / 2, size.y / 2, 0));
            GL.Vertex(center + new Vector3(-size.x / 2, -size.y / 2, 0));
            GL.End();
        }

        private static void DrawFilledBoxGizmo(Bounds bounds)
        {
            DrawFilledBoxGizmo(bounds.center, bounds.size);
        }

        #endregion
        
        public static void DrawGizmos()
        {
            if (!mat)
                CreateMaterial();
            
            // 요청이 들어온 Gizmo Call Data들을 순회한 뒤 Flush
            while (DrawGizmoData?.Count > 0)
            {
                GL.PushMatrix();
                GL.LoadOrtho();
                
                var data = DrawGizmoData[0];
                if (data["type"] is GizmoType type)
                {
                    Gizmos.color = (Color) data.GetDefault("color", Color.red);
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

                DrawGizmoData.RemoveAt(0);
                GL.PopMatrix();
            }
        }
    }

}