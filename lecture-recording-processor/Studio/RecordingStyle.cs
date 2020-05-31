namespace RecordingProcessor.Studio
{
    public class RecordingStyle
    {
        public TalkingHeadConfiguration TalkingHeadConfig = new TalkingHeadConfiguration();
        public StageConfiguration StageConfig;
        public ChromaKeyParameters ChromaKeyParams;
        public Dimension targetDimension;

        public static RecordingStyle TkStudioStyle(Dimension targetDim)
        {
            return new RecordingStyle
            {
                targetDimension = targetDim,
                ChromaKeyParams = new ChromaKeyParameters
                {
                    //color = "0x358F75",
                    //color = "0x49AD68",
                    //color= "0x69AE53", //max cnuvs
                    color = "0x249561",
                    similarity = "0.13",
                    blend = "0.001"
                },
                TalkingHeadConfig = new TalkingHeadConfiguration
                {
                    Crop = new CropDimensions(0.2f, 0.0f, 0.6f, 0.8f, targetDim), //new!
                    //Crop = new CropDimensions(0.07f, 0.0f, 0.5f, 0.67f, targetDim), //old
                    ChromaKeyTalkingHead = true
                },
                StageConfig = new StageConfiguration
                {
                    ChromaKeyTalkingHead = true,
                    slideTransformation = new TranslateScaleAndDeformTransformation(0.008f, 0.014f, 0.75f, 0.03f, targetDim),
                    speakerTransformation = new TranslateScaleAndDeformTransformation(0.47f, 0.31f, 0.70f, 0, targetDim) //new!
                    //speakerTransformation = new TranslateScaleAndDeformTransformation(0.58f, 0.28f, 0.75f, 0, targetDim) //old
                }
            };
        }

        public class TalkingHeadConfiguration
        {
            public CropDimensions Crop;
            public bool ChromaKeyTalkingHead;
        }

        public class StageConfiguration
        {
            public bool ChromaKeyTalkingHead;
            public TransformationParameters slideTransformation;
            public TransformationParameters speakerTransformation;
        }

        public class TranslateScaleAndDeformTransformation : TransformationParameters
        {
            public TranslateScaleAndDeformTransformation(float x, float y, float scale, float deform, Dimension dimension)
            {
                InitFields((int)(x * dimension.width), (int)(y * dimension.height), (int)(dimension.width * scale), (int)(dimension.height * scale), (int)(deform * dimension.height));
            }

            public TranslateScaleAndDeformTransformation(int x, int y, int width, int height, int deform)
            {
                InitFields(x, y, width, height, deform);
            }

            private void InitFields(int x, int y, int width, int height, int deform)
            {
                leftTop = new StrPoint { X = x.ToString(), Y = y.ToString() };
                leftBottom = new StrPoint { X = x.ToString(), Y = (y + height).ToString() };
                rightTop = new StrPoint { X = (x + width).ToString(), Y = (y + deform).ToString() };
                rightBottom = new StrPoint { X = (x + width).ToString(), Y = (y + height - deform).ToString() };
            }

            public override string ToString()
            {
                return base.ToString() + ":sense=1";
            }
        }

        public class TranslateAndScaleTransformation : TransformationParameters
        {
            public TranslateAndScaleTransformation(int x, int y, int widht, int height)
            {
                //0:0:W:0:0:H:W:H
                leftTop = new StrPoint { X = x.ToString(), Y = y.ToString() };
                rightTop = new StrPoint { X = (x + widht).ToString(), Y = y.ToString() };
                leftBottom = new StrPoint { X = x.ToString(), Y = (y + height).ToString() };
                rightBottom = new StrPoint { X = (x + widht).ToString(), Y = (y + height).ToString() };
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

        public class TransformationParameters
        {
            public StrPoint leftTop;
            public StrPoint rightTop;
            public StrPoint leftBottom;
            public StrPoint rightBottom;

            public override string ToString()
            {
                //0:0:W:H/4:0:H:W:3*H/4:0:1:0
                return leftTop.X + ":" + leftTop.Y + ":" +
                       rightTop.X + ":" + rightTop.Y + ":" +
                       leftBottom.X + ":" + leftBottom.Y + ":" +
                       rightBottom.X + ":" + rightBottom.Y + ":interpolation=cubic";
            }

            public class StrPoint
            {
                public string X;
                public string Y;
            }
        }

        public class CropDimensions
        {
            public CropDimensions(float x, float y, float width, float height, Dimension dimension)
            {

                this.x = (int)(x * dimension.width);
                this.y = (int)(y * dimension.height);
                this.width = (int)(width * dimension.width);
                this.height = (int)(height * dimension.height);
            }

            public int x;
            public int y;
            public int width;
            public int height;

        }

        public class ChromaKeyParameters
        {
            public string color;
            public string similarity;
            public string blend;
        }
    }
}
