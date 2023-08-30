using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FracturedState.Game;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Network;

namespace FracturedState.UI
{
    public static class InterfaceManager
    {
		private static readonly int[] ElementTriangles = { 0, 1, 2, 2, 1, 3 };

        public static Mesh MakeFullCameraQuad(Camera camera)
        {
            var verts = new Vector3[4];
            verts[0] = camera.ViewportToWorldPoint(new Vector3(0, 1, camera.farClipPlane));
            verts[1] = camera.ViewportToWorldPoint(new Vector3(1, 1, camera.farClipPlane));
            verts[2] = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.farClipPlane));
            verts[3] = camera.ViewportToWorldPoint(new Vector3(1, 0, camera.farClipPlane));

            var uvs = new Vector2[4];
            uvs[0] = new Vector2(0, 1);
            uvs[1] = new Vector2(1, 1);
            uvs[2] = new Vector2(0, 0);
            uvs[3] = new Vector2(1, 0);

	        var mesh = new Mesh
	        {
		        vertices = verts,
		        triangles = ElementTriangles,
		        uv = uvs
	        };

	        return mesh;
        }

        public static Mesh MakeCameraQuad(Camera camera, float left, float bottom, float width, float height, float depth)
        {
            var vLeft = left / camera.pixelHeight;
            var vRight = (left + width) / camera.pixelHeight;
            var vBottom = bottom / camera.pixelWidth;
            var vTop = (bottom + height) / camera.pixelWidth;

            var verts = new Vector3[4];
            verts[0] = camera.ViewportToWorldPoint(new Vector3(vBottom, vRight, camera.farClipPlane - depth));
            verts[1] = camera.ViewportToWorldPoint(new Vector3(vTop, vRight, camera.farClipPlane - depth));
            verts[2] = camera.ViewportToWorldPoint(new Vector3(vBottom, vLeft, camera.farClipPlane - depth));
            verts[3] = camera.ViewportToWorldPoint(new Vector3(vTop, vLeft, camera.farClipPlane - depth));

            var uvs = new Vector2[4];
            uvs[0] = new Vector2(0, 1);
            uvs[1] = new Vector2(1, 1);
            uvs[2] = new Vector2(0, 0);
            uvs[3] = new Vector2(1, 0);

	        var mesh = new Mesh
	        {
		        vertices = verts,
		        triangles = ElementTriangles,
		        uv = uvs
	        };

	        return mesh;
        }
    }
}