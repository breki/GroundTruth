using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using Brejc.Geometry;
using Brejc.OsmLibrary;

namespace GroundTruth.Engine.PolishMaps
{
    [SuppressMessage("Microsoft.Naming", "CA1722:IdentifiersShouldNotHaveIncorrectPrefix")]
    public class CGpsMapperMapWriter : CGpsMapperWriterBase<CGpsMapperMapWriter>
    {
        public CGpsMapperMapWriter (Stream outputStream, string mapFileInfo)
            : base (outputStream, mapFileInfo)
        {
        }

        public CGpsMapperMapWriter AddCoordinates (string parameterName, int level, OsmNode node)
        {
            StringBuilder line = new StringBuilder ();
            line.AppendFormat ("{0}{1}=", parameterName, level);

            line.AppendFormat (CultureInfo.InvariantCulture,
                               "({0},{1})",
                               RenderCoordinateValue (node.Y),
                               RenderCoordinateValue (node.X));

            AppendLine (line.ToString ());

            return this;
        }

        public CGpsMapperMapWriter AddCoordinates(string parameterName, int level, IList<OsmNode> nodes)
        {
            StringBuilder line = new StringBuilder();
            line.AppendFormat("{0}{1}=", parameterName, level);

            for (int i = 0; i < nodes.Count; i++)
            {
                OsmNode node = nodes[i];
                line.AppendFormat(CultureInfo.InvariantCulture,
                                  "({0},{1})",
                                  RenderCoordinateValue (node.Y),
                                  RenderCoordinateValue (node.X));
                if (i < nodes.Count - 1)
                    line.Append(",");
            }

            AppendLine(line.ToString());

            return this;
        }

        [SuppressMessage ("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public CGpsMapperMapWriter AddCoordinates (string parameterName, int level, IPointD2List points)
        {
            StringBuilder line = new StringBuilder ();
            line.AppendFormat ("{0}{1}=", parameterName, level);

            for (int i = 0; i < points.PointsCount; i++)
            {
                IPointD2 point = points.GetPointD2(i);
                line.AppendFormat (CultureInfo.InvariantCulture,
                                  "({0},{1})",
                                  RenderCoordinateValue (point.Y),
                                  RenderCoordinateValue (point.X));
                if (i < points.PointsCount - 1)
                    line.Append (",");
            }

            AppendLine (line.ToString ());

            return this;
        }

        public CGpsMapperMapWriter AddTypeReference (GarminMapTypeRegistration typeRegistration)
        {
            AddHexValue("Type", typeRegistration.TypeId);
            return this;
        }

        public static string RenderCoordinateValue (double coordinateValue)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0:0.##################}", coordinateValue);
        }

        protected override void CloseSection()
        {
            if (CurrentSectionId != null)
            {
                AppendLine("[END-{0}]", CurrentSectionId);
                AppendLine();
            }
        }
    }
}