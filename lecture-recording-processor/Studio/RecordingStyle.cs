namespace RecordingProcessor.Studio
{
    public abstract class RecordingStyle
    {
        public TalkingHeadConfiguration TalkingHeadConfig { get; set; }

        public StageConfiguration StageConfig { get; set; }

        public ChromaKeyParameters ChromaKeyParams { get; set; }

        public Dimension TargetDimension { get; set; }

        public RecordingStyle(Dimension targetDimension)
        {
            this.TargetDimension = targetDimension;
            this.TalkingHeadConfig = new TalkingHeadConfiguration();
        }

        /// <summary>
        /// Returns the ffmpeg filter string including the outputs [slides1], [th_ck_ct], [stage].
        /// </summary>
        /// <returns></returns>
        public abstract string getFFmpegFilterString();
    }
}
