namespace RecordingProcessor.Studio
{
    public class TransformationParameters
    {
        public StrPoint LeftTop { get; set; }

        public StrPoint RightTop { get; set; }

        public StrPoint LeftBottom { get; set; }

        public StrPoint RightBottom { get; set; }

        public override string ToString()
        {
            //0:0:W:H/4:0:H:W:3*H/4:0:1:0
            return LeftTop.X + ":" + LeftTop.Y + ":" +
                   RightTop.X + ":" + RightTop.Y + ":" +
                   LeftBottom.X + ":" + LeftBottom.Y + ":" +
                   RightBottom.X + ":" + RightBottom.Y + ":interpolation=cubic";
        }

        public class StrPoint
        {
            public string X;
            public string Y;
        }
    }
}
