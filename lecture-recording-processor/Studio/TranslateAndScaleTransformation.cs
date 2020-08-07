namespace RecordingProcessor.Studio
{
    public class TranslateAndScaleTransformation : TransformationParameters
    {
        public TranslateAndScaleTransformation(int x, int y, int widht, int height)
        {
            //0:0:W:0:0:H:W:H
            LeftTop = new StrPoint { X = x.ToString(), Y = y.ToString() };
            RightTop = new StrPoint { X = (x + widht).ToString(), Y = y.ToString() };
            LeftBottom = new StrPoint { X = x.ToString(), Y = (y + height).ToString() };
            RightBottom = new StrPoint { X = (x + widht).ToString(), Y = (y + height).ToString() };
        }

        public override string ToString()
        {
            return base.ToString() + ":sense=1";
        }

        public static TranslateAndScaleTransformation MoveToRightBottomCorner(float scale, int oWidth, int oHeight)
        {
            int width = (int)(oWidth * scale);
            int height = (int)(oHeight * scale);
            int x = oWidth - width;
            int y = oHeight - height;

            return new TranslateAndScaleTransformation(x, y, width, height);
        }
    }
}
