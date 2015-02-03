namespace AppmetrS2S.Actions
{
    #region using directives

    using System;
    using System.Runtime.Serialization;

    #endregion

    [DataContract]
    public class Level : AppMetrAction
    {
        private const String ACTION = "trackLevel";

        [DataMember(Name = "level")]
        private int _level;
        
        protected Level()
        {
        }

        public Level(int level) : base(ACTION)
        {
            _level = level;
        }

        public override int CalcApproximateSize()
        {
            return base.CalcApproximateSize() + 4;
        }
    }
}