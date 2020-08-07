using System;
using System.Collections.Generic;
using System.Text;

namespace RecordingProcessor.Studio.Styles
{
    public class TKStudioStyle : RecordingStyle
    {
        public TKStudioStyle(Dimension targetDim) : base(targetDim)
        {
            ChromaKeyParams = new ChromaKeyParameters
            {
                Color = "0x249561",
                Similarity = "0.13",
                Blend = "0.001"
            };

            TalkingHeadConfig = new TalkingHeadConfiguration
            {
                Crop = new CropDimensions(0.2f, 0.0f, 0.6f, 0.8f, targetDim),
                ChromaKeyTalkingHead = true
            };

            StageConfig = new StageConfiguration
            {
                ChromaKeyTalkingHead = true,
                SlideTransformation = new TranslateScaleAndDeformTransformation(0.008f, 0.014f, 0.75f, 0.03f, targetDim),
                SpeakerTransformation = new TranslateScaleAndDeformTransformation(0.47f, 0.31f, 0.70f, 0, targetDim)
            };
        }

        public override string getFFmpegFilterString()
        {
            var sb = new StringBuilder();

            sb.Append("[0:v]scale=" + this.TargetDimension.Width + ":-2, crop=" + this.TargetDimension.Width + ":" + this.TargetDimension.Height + ",fps=fps=30,split=2[slides1][slides2];");
            sb.Append("[1v]scale=" + this.TargetDimension.Width + ":" + this.TargetDimension.Height + ",fps=fps=30[th];");
            sb.Append("[th]format=rgba" + (this.ChromaKeyParams != null ? ",chromakey=" + this.ChromaKeyParams.Color + ":" + this.ChromaKeyParams.Similarity + ":" + this.ChromaKeyParams.Blend : "") + ",split=2[th_ck1][th_ck2];");
            sb.Append("[2][th_ck1]overlay=0:0[th_ck_bg];");
            sb.Append("[th_ck_bg]crop=" + this.TalkingHeadConfig.Crop.Width + ":" +
                    this.TalkingHeadConfig.Crop.Height + ":" +
                    this.TalkingHeadConfig.Crop.X + ":" +
                    this.TalkingHeadConfig.Crop.Y + "[th_ck_ct];"));
            sb.Append("[slides2]format = rgba,pad = iw + 4:ih + 4:2:2:black@0,");
            sb.Append("perspective=" + this.StageConfig.SlideTransformation.ToString() + ",");
            sb.Append("crop=" + this.TargetDimension.Width + ":" + this.TargetDimension.Height + ":2:2" + "[slides_perspective];");
            sb.Append("[th_ck2]pad = iw + 4:ih + 4:2:2:black@0," + "perspective=" + this.StageConfig.SpeakerTransformation.ToString() + "[th_ck_tr];");
            sb.Append("[2][slides_perspective]overlay=0:0[slides_with_background];");
            sb.Append("[slides_with_background][th_ck_tr]overlay=0:0[stage]");

            return sb.ToString();
        }
    }
}
