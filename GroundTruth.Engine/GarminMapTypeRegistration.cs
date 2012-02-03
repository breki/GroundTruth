using System;
using GroundTruth.Engine.LabelBuilding;

namespace GroundTruth.Engine
{
    public abstract class GarminMapTypeRegistration
    {
        public LabelExpression Label
        {
            get { return label; }
            set { label = value; }
        }

        /// <summary>
        /// Gets or sets the maximum Garmin hardware zoom level this feature will be visible at.
        /// </summary>
        /// <value>The maximum Garmin hardware zoom level .</value>
        public int MaxLevel
        {
            get { return maxLevel; }
            set { maxLevel = value; }
        }

        /// <summary>
        /// Gets or sets the minimum Garmin hardware zoom level this feature will be visible at.
        /// </summary>
        /// <value>The maximum Garmin hardware zoom level .</value>
        public int MinLevel
        {
            get { return minLevel; }
            set { minLevel = value; }
        }

        public PatternDefinition Pattern
        {
            get { return pattern; }
            set { pattern = value; }
        }

        public string RuleName
        {
            get { return ruleName; }
        }

        public int TypeId
        {
            get { return typeId; }
        }

        public string TypeName
        {
            get { return typeName; }
            set { typeName = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this feature is visible at the specified Garmin hardware zoom level.
        /// </summary>
        /// <param name="hardwareLevel">The Garmin hardware level.</param>
        /// <returns>
        ///     <c>true</c> if the feature is visible at the specified level; otherwise, <c>false</c>.
        /// </returns>
        public bool IsVisibleAtHardwareLevel (int hardwareLevel)
        {
            return minLevel <= hardwareLevel && hardwareLevel <= maxLevel;
        }

        protected GarminMapTypeRegistration(int typeId, string ruleName)
        {
            this.typeId = typeId;
            this.ruleName = ruleName;
        }

        private LabelExpression label;
        private int maxLevel = Int32.MaxValue;
        private int minLevel = Int32.MinValue;
        private PatternDefinition pattern = new PatternDefinition();
        private string ruleName;
        private readonly int typeId;
        private string typeName;
    }
}