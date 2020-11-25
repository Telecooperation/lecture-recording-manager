using System.Text;

namespace RecordingProcessor.Studio.Styles
{
    public class TKSimpleStyle : RecordingStyle
    {
        public TKSimpleStyle(Dimension targetDimension) : base(targetDimension)
        {
            TalkingHeadConfig = new TalkingHeadConfiguration
            {
                Crop = new CropDimensions(0.2f, 0.0f, 0.6f, 0.8f, targetDimension),
                ChromaKeyTalkingHead = false
            };

            StageConfig = new StageConfiguration
            {
                SlideTransformation = new TranslateScaleAndDeformTransformation(0.008f, 0.014f, 0.75f, 0.03f, targetDimension),
            };
        }

        public override string getFFmpegFilterString()
        {
            var sb = new StringBuilder();

            //+ [0:v]scale=1280:-2, crop=1280:720,fps=fps=30,split=2[slides1][slides2];
            //+ [1v]scale=1280:720,fps=fps=30[th];
            //+ [th]format=rgba,split=2[th_ck1][th_ck2];
            //+ [2][th_ck1]overlay=0:0[th_ck_bg];
            //+ [th_ck_bg]crop=768:576:256:0[th_ck_ct];

            //+ [slides2]format = rgba,scale=980:551[slides_perspective];
            // [th_ck2]crop=600:600:400:0,scale=w=iw/2:h=ih/2[th_ck_tr];
            // [2][slides_perspective]overlay=0:50[slides_with_background];
            // [slides_with_background][th_ck_tr]overlay=980:200[stage]

            sb.Append("[0:v]scale=" + this.TargetDimension.Width + ":-2, crop=" + this.TargetDimension.Width + ":" + this.TargetDimension.Height + ",fps=fps=30,split=2[slides1][slides2];");
            sb.Append("[1v]scale=" + this.TargetDimension.Width + ":" + this.TargetDimension.Height + ",fps=fps=30[th];");
            sb.Append("[th]format=rgba,split=2[th_ck1][th_ck2];");
            sb.Append("[2][th_ck1]overlay=0:0[th_ck_bg];");
            sb.Append("[th_ck_bg]crop=" + this.TalkingHeadConfig.Crop.Width + ":" + this.TalkingHeadConfig.Crop.Height + ":" + this.TalkingHeadConfig.Crop.X + ":" + this.TalkingHeadConfig.Crop.Y + "[th_ck_ct];");
            //sb.Append("[slides2]format=rgba,scale=" + (int)(TargetDimension.Width * 0.76f) + ":" + (int)(TargetDimension.Height * 0.76f) + "[slides_perspective];");

            sb.Append("[slides2]format = rgba,pad = iw + 4:ih + 4:2:2:black@0,");
            sb.Append("perspective=" + this.StageConfig.SlideTransformation.ToString() + ",");
            sb.Append("crop=" + this.TargetDimension.Width + ":" + this.TargetDimension.Height + ":2:2" + "[slides_perspective];");

            sb.Append("[th_ck2]crop=" + (int)(TargetDimension.Width * 0.47f) + ":" + (int)(TargetDimension.Width * 0.47f) + ":" + (int)(TargetDimension.Height * 0.55f) + ":0,scale=w=iw/2:h=ih/2[th_ck_tr];");
            sb.Append("[2][slides_perspective]overlay=0:0[slides_with_background];");
            sb.Append("[slides_with_background][th_ck_tr]overlay=" + (int)(TargetDimension.Width * 0.76f) + ":" + (int)(TargetDimension.Height * 0.28f) + "[stage]");

            return sb.ToString();
        }
    }
}
