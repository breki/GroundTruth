using System;
using System.Globalization;
using Brejc.OsmLibrary;
using GroundTruth.Engine.Tasks;

namespace GroundTruth.Engine.LabelBuilding
{
    public class ElevationLabelBuildingFunction : ILabelBuildingFunction
    {
        public string Calculate (MapMakerSettings mapMakerSettings, OsmObjectBase osmObjectBase, Tag osmTag, OsmRelation parentRelation)
        {
            double elevation = double.Parse(osmTag.Value, CultureInfo.InvariantCulture);

            if (mapMakerSettings.ElevationUnits == 'f')
                elevation /= 0.30480061;

            return Math.Round(elevation).ToString("F0", CultureInfo.InvariantCulture);
        }
    }
}