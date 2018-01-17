namespace Assets
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    static class RectangleExtensions
    {       
        public static GameObject CreateRectangleMeshObject(this Rect rectangle, Transform parent, Color32 color)
        {
            using (var vh = new VertexHelper())
            {
                GameObject selectionBox = new GameObject(Constants.RESULT_BOX_NAME_PREFIX);
                selectionBox.AddComponent<MeshFilter>();
                selectionBox.transform.SetParent(parent, false);
                selectionBox.transform.SetParent(parent.Find(Constants.CANVAS_MESH_NAME));
                selectionBox.transform.localScale = new Vector3(1, 1, 1);
                selectionBox.transform.localPosition = new Vector3(-398, 399, 0);
                selectionBox.transform.localRotation = new Quaternion(1, 0, 0, 0);
                MeshRenderer rend = selectionBox.AddComponent<MeshRenderer>();
                rend.materials[0] = Resources.Load(Constants.LAMBERT_MATERIAL_LOCATION, typeof(Material)) as Material;
                rend.materials[0].color = color;
                rend.materials[0].shader = Shader.Find(Constants.RESULT_BOX_SHADER_NAME);
                Mesh myMesh = selectionBox.GetComponent<MeshFilter>().mesh;
                rectangle.DrawRectangle(vh, color, myMesh);
                return selectionBox;
            }
        }

        public static void DrawRectangle(this Rect rectangle, VertexHelper vh, Color32 color, Mesh mesh)
        {
            vh.AddVert(new Vector3(rectangle.x, rectangle.y), color, new Vector2(0f, 0f));
            vh.AddVert(new Vector3(rectangle.x, rectangle.y + rectangle.height), color, new Vector2(0f, 1f));
            vh.AddVert(new Vector3(rectangle.x + rectangle.width, rectangle.y + rectangle.height), color, new Vector2(1f, 1f));
            vh.AddVert(new Vector3(rectangle.x + rectangle.width, rectangle.y), color, new Vector2(1f, 0f));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
            vh.FillMesh(mesh);
        }
        /// <summary>
        /// Return a list of rectangles to make the outline.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="borderWidth"></param>
        /// <returns></returns>
        public static List<Rect> GetRectOutlines(this Rect rectangle, int borderWidth, bool scaling = false, Transform mesh = null)
        {
            List<Rect> rectangles = null;
            int webcamHeight = DisplayWebcam.WebcamHeightResolution;
            int webcamWidth = DisplayWebcam.WebcamWidthResolution;
            int screenWidth = Screen.width;
            int screenHeight = Screen.height;
            float fixedPlaneWidth = Constants.DEBUG_PLANE_WIDTH * screenHeight;
            float fixedPlaneHeight = Constants.DEBUG_PLANE_HEIGHT * screenWidth;
            float horizontalRatio = 0.0F;
            float verticalRatio = 0.0F;

            if (!Constants.DEBUG)
            {
                horizontalRatio = screenWidth / (float) webcamWidth;
                verticalRatio = screenHeight / (float) webcamHeight;
            }
            else
            {
                horizontalRatio = fixedPlaneWidth / webcamWidth;
                verticalRatio = fixedPlaneHeight / webcamHeight;
            }

            if (!scaling)
                horizontalRatio = verticalRatio = 1;

            float rectangleX = rectangle.x * horizontalRatio;
            float rectangleY = rectangle.y * verticalRatio;
            float rectangleWidth = rectangle.width * horizontalRatio;
            float rectangleHeight = rectangle.height * verticalRatio;
            float borderWidthHeight = borderWidth * verticalRatio;
            float borderWidthWidth = borderWidth * horizontalRatio;

            if (!Constants.DEBUG)
                rectangles = new List<Rect>()
                {
                    // Top Rectangle
                    new Rect(rectangleX, rectangleY, rectangleWidth, borderWidthHeight),
                    // Bottom Rectangle
                    new Rect(rectangleX, rectangleY + rectangleHeight - borderWidthHeight, rectangleWidth, borderWidthHeight),
                    // Right Rectangle
                    new Rect(rectangleX + rectangleWidth - borderWidthWidth, rectangleY + borderWidthHeight, borderWidthWidth, rectangleHeight - (borderWidthHeight * 2)),
                    // Left Rectangle
                    new Rect(rectangleX, rectangleY + borderWidthHeight, borderWidthWidth, rectangleHeight - (borderWidthHeight * 2))
                };
            else
                rectangles = new List<Rect>()
                {
                    // Top Rectangle
                    new Rect(rectangleX, rectangleY, rectangleWidth, borderWidthHeight),
                    // Bottom Rectangle
                    new Rect(rectangleX, rectangleY + rectangleHeight - borderWidthHeight, rectangleWidth, borderWidthHeight),
                    // Right Rectangle
                    new Rect(rectangleX + rectangleWidth - borderWidthWidth, rectangleY + borderWidthHeight, borderWidthWidth, rectangleHeight - (borderWidthHeight * 2)),
                    // Left Rectangle
                    new Rect(rectangleX, rectangleY + borderWidthHeight, borderWidthWidth, rectangleHeight - (borderWidthHeight * 2))
                };

            return rectangles;
        }

    }
}
