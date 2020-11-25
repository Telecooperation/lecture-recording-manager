namespace RecordingProcessor.Studio
{
    public class TranslateScaleAndDeformTransformation : TransformationParameters
    {
        public TranslateScaleAndDeformTransformation(float x, float y, float scale, float deform, Dimension dimension)
        {
            InitFields((int)(x * dimension.Width), (int)(y * dimension.Height), (int)(dimension.Width * scale), (int)(dimension.Height * scale), (int)(deform * dimension.Height));
        }

        public TranslateScaleAndDeformTransformation(int x, int y, int width, int height, int deform)
        {
            InitFields(x, y, width, height, deform);
        }

        private void InitFields(int x, int y, int width, int height, int deform)
        {
            LeftTop = new StrPoint { X = x.ToString(), Y = y.ToString() };
            LeftBottom = new StrPoint { X = x.ToString(), Y = (y + height).ToString() };
            RightTop = new StrPoint { X = (x + width).ToString(), Y = (y + deform).ToString() };
            RightBottom = new StrPoint { X = (x + width).ToString(), Y = (y + height - deform).ToString() };
        }

        public override string ToString()
        {
            return base.ToString() + ":sense=1";
        }
    }
}
