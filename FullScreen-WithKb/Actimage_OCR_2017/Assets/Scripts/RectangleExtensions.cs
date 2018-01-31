namespace Assets
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    static class RectangleExtensions
    {
        public static GameObject CreateTextInRectangle(this List<Rect> rectangles, Transform parent, string text)
        {
            GameObject textMesh = new GameObject("Text" + text);
            textMesh.transform.SetParent(parent, false);
            textMesh.transform.localScale = new Vector3(1, -1, 1);
            textMesh.transform.localPosition = new Vector3(rectangles[0].x + (rectangles[0].width / 2), rectangles[1].y - (rectangles[2].height / 2), 0);
            textMesh.AddComponent<MeshRenderer>();
            TextMesh myTextMesh = textMesh.AddComponent<TextMesh>();
            myTextMesh.fontSize = 480;
            myTextMesh.anchor = TextAnchor.MiddleCenter;
            myTextMesh.alignment = TextAlignment.Center;
            myTextMesh.text = text;
            return textMesh;
        }


        public static GameObject CreateRectangleMeshObject(this List<Rect> rectangles, Transform parent, Color32 color, string text)
        {
            GameObject selectionBox = new GameObject(Constants.RESULT_BOX_NAME_PREFIX + text);
            selectionBox.transform.SetParent(parent, false);
            selectionBox.transform.SetParent(parent.Find(Constants.CANVAS_MESH_NAME));
            selectionBox.transform.localScale = new Vector3(1, -1, 1);
            selectionBox.transform.localPosition = new Vector3(0, 0, 0);
            selectionBox.transform.localRotation = new Quaternion(0, 0, 0, 0);
            MeshRenderer rend = selectionBox.AddComponent<MeshRenderer>();
            rend.materials[0] = new Material(Shader.Find("Diffuse"));
            rend.materials[0].color = color;
            Mesh myMesh = selectionBox.AddComponent<MeshFilter>().mesh;
            rectangles.DrawRectangleOutline(color, myMesh);

            if (Constants.DEBUG)
                CreateTextInRectangle(rectangles, selectionBox.transform, text);

            return selectionBox;
        }

        public static void DrawRectangle(Rect rectangle, Color32 color, Mesh mesh)
        {
            using (var vh = new VertexHelper())
            {
                vh.AddVert(new Vector3(rectangle.x, rectangle.y), color, new Vector2(0f, 0f));
                vh.AddVert(new Vector3(rectangle.x, rectangle.y + rectangle.height), color, new Vector2(0f, 1f));
                vh.AddVert(new Vector3(rectangle.x + rectangle.width, rectangle.y + rectangle.height), color, new Vector2(1f, 1f));
                vh.AddVert(new Vector3(rectangle.x + rectangle.width, rectangle.y), color, new Vector2(1f, 0f));
                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
                vh.FillMesh(mesh);
            }
        }

        public static void DrawRectangleOutline(this List<Rect> rectangles, Color32 color, Mesh mesh)
        {
            using (var vh = new VertexHelper())
            {
                int rectangleIndex = 0;
                foreach (var rectangle in rectangles)
                {
                    vh.AddVert(new Vector3(rectangle.x, rectangle.y), color, new Vector2(0f, 0f));
                    vh.AddVert(new Vector3(rectangle.x, rectangle.y + rectangle.height), color, new Vector2(0f, 1f));
                    vh.AddVert(new Vector3(rectangle.x + rectangle.width, rectangle.y + rectangle.height), color, new Vector2(1f, 1f));
                    vh.AddVert(new Vector3(rectangle.x + rectangle.width, rectangle.y), color, new Vector2(1f, 0f));
                    vh.AddTriangle(0 + rectangleIndex, 1 + rectangleIndex, 2 + rectangleIndex);
                    vh.AddTriangle(2 + rectangleIndex, 3 + rectangleIndex, 0 + rectangleIndex);
                    rectangleIndex += 4;
                }
                vh.FillMesh(mesh);
            }
        }

        /// <summary>
        /// Return a list of rectangles to make the outline.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="borderWidth"></param>
        /// <returns></returns>
        public static List<Rect> GetRectOutlines(this Rect rectangle, int borderWidth, Rect canvas)
        {
            int webcamHeight = DisplayWebcam.WebcamHeightResolution;
            int webcamWidth = DisplayWebcam.WebcamWidthResolution;
            float widthOffset = 0;
            float heightOffset = 0;

            float heightRatio = (float)Screen.height / webcamHeight;
            float widthRatio = (float)Screen.width / webcamWidth;
            float smallestRatio = Math.Min(heightRatio, widthRatio);
            if (heightRatio == widthRatio)
            {

            }
            else if (smallestRatio != widthRatio)
            {
                widthOffset = widthRatio * webcamHeight;
                widthOffset = (Screen.width - widthOffset);
            }
            else
            {
                heightOffset = heightRatio * webcamWidth;
                heightOffset = (Screen.height - heightOffset);
            }

            float screenHeight = canvas.height - heightOffset;
            float screenWidth = canvas.width - widthOffset;

            float rectangleX = rectangle.x / webcamWidth * screenWidth - (Screen.width / 2) + (widthOffset / 2);
            float rectangleY = rectangle.y / webcamHeight * screenHeight - (Screen.height / 2) + (heightOffset / 2);
            float rectangleWidth = (rectangle.width / webcamWidth) * screenWidth;
            float rectangleHeight = (rectangle.height / webcamHeight) * screenHeight;

            float borderWidthHeight = borderWidth;
            float borderWidthWidth = borderWidth;

            List<Rect> rectangles = new List<Rect>()
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
