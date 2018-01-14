using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    static class RectangleExtensions
    {        /// <summary>
             /// Return a list of rectangles to make the outline.
             /// </summary>
             /// <param name="rectangle"></param>
             /// <param name="borderWidth"></param>
             /// <returns></returns>

        public static List<Rect> GetRectOutlines(this Rect rectangle, int borderWidth, Transform mesh)
        {
            List<Rect> rectangles = null;
            float fixedPlaneWidth = 0.04700003F;
            float fixedPlaneHeight = 0.03750001F;
            int webcamHeight = DisplayWebcam.tex.height;
            int webcamWidth = DisplayWebcam.tex.width;
            int screenWidth = Screen.width;
            int screenHeight = Screen.height;

            fixedPlaneWidth = 0.04700003F * screenHeight;
            fixedPlaneHeight = 0.03750001F * screenWidth;
            var position = new Vector3(mesh.position.x, mesh.position.y, mesh.localPosition.z);

            double horizontalRatio = 0.0;
            double verticalRatio = 0.0;

            if (Constants.DEBUG)
            {
                horizontalRatio = screenWidth / webcamWidth;
                verticalRatio = screenHeight / webcamHeight;
            }
            else
            {
                horizontalRatio = fixedPlaneWidth / webcamWidth;
                verticalRatio = fixedPlaneHeight / webcamHeight;
            }


            float rectangleX = (float)(rectangle.x * horizontalRatio);
            float rectangleY = (float)(rectangle.y * verticalRatio);
            float rectangleWidth = (float)(rectangle.width * horizontalRatio);
            float rectangleHeight = (float)(rectangle.height * verticalRatio);
            float borderWidthHeight = (float)(borderWidth * verticalRatio);
            float borderWidthWidth = (float)(borderWidth * horizontalRatio);

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
                    //new Rect(rectangle.x, rectangle.y, rectangle.width, borderWidth),
                    //new Rect(rectangle.x, rectangle.y + rectangle.height - borderWidth, rectangle.width, borderWidth),
                    //new Rect(rectangle.x + rectangle.width - borderWidth, rectangle.y + borderWidth,borderWidth, rectangle.height - (borderWidth * 2)),
                    //new Rect(rectangle.x, rectangle.y + borderWidth,borderWidth, rectangle.height - (borderWidth * 2))

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
