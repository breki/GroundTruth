using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;

namespace GroundTruth.Engine.Tasks
{
    public class GenerateMapTypesPolishFileTask : ITask
    {
        public bool SkipOnNonWindows
        {
            get { return false; }
        }

        public string TaskDescription
        {
            get { return string.Format (CultureInfo.InvariantCulture, "Generate polish TYP file"); }
        }

        public void Execute (ITaskRunner taskRunner)
        {
            string polishTypeFileName = Path.GetFullPath (
                Path.Combine (
                    taskRunner.MapMakerSettings.TempDir,
                    String.Format (CultureInfo.InvariantCulture, "{0}.TYP.txt", taskRunner.MapMakerSettings.ProductCode)));

            using (Stream mapOutputStream = File.Open (polishTypeFileName, FileMode.Create))
            {
                using (CGpsMapperGeneralFileWriter file = new CGpsMapperGeneralFileWriter (
                    mapOutputStream, 
                    true,
                    "types file"))
                {
                    file.AddSection ("_id");
                    file.Add ("ProductCode", taskRunner.MapMakerSettings.ProductCode);
                    file.Add ("FID", taskRunner.MapMakerSettings.FamilyCode);
                    file.Add ("CodePage", taskRunner.MapMakerSettings.AdditionalMapParameters["CodePage"]);

                    file.AddSection ("_DRAWORDER");

                    int areaTypesCount = taskRunner.MapMakerSettings.TypesRegistry.AreaTypeRegistrations.Count;

                    int drawOrder = 1;
                    foreach (AreaTypeRegistration registration in taskRunner.MapMakerSettings.TypesRegistry.AreaTypeRegistrationsByPriority)
                    {
                        file
                            .AddFormat (";rule={0}", registration.RuleName)
                            .AddFormat ("Type=0x{0:x},{1}", registration.TypeId, drawOrder);

                        drawOrder = Math.Min(drawOrder + 1, 8);
                    }

                    foreach (AreaTypeRegistration registration in taskRunner.MapMakerSettings.TypesRegistry.AreaTypeRegistrationsByPriority)
                    {
                        file.AddSection ("_polygon")
                            .AddFormat (";rule={0}", registration.RuleName)
                            .AddFormat("Type=0x{0:x}", registration.TypeId)
                            ;

                        WritePolygonTypeDefinition (taskRunner.MapMakerSettings, file, registration);
                    }

                    foreach (PointTypeRegistration registration in taskRunner.MapMakerSettings.TypesRegistry.PointTypeRegistrations.Values)
                    {
                        if (registration.Pattern.Height > 0)
                        {
                            file.AddSection("_point")
                                .AddFormat (";rule={0}", registration.RuleName)
                                .AddFormat ("Type=0x{0:x}", registration.TypeId)
                                ;

                            WritePointTypeDefinition(taskRunner.MapMakerSettings, file, registration);
                        }
                    }

                    foreach (LineTypeRegistration registration in 
                        taskRunner.MapMakerSettings.TypesRegistry.LineTypeRegistrations.Values)
                    {
                        WritePolylineTypeDefinition (taskRunner.MapMakerSettings, file, registration);
                    }

                    file.FinishMap ();
                }
            }

            taskRunner.RegisterProductFile (new ProductFile (PolishTypeFile, polishTypeFileName, true));
        }

        [SuppressMessage ("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "registration")]
        [SuppressMessage ("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "file")]
        [SuppressMessage ("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "mapMakerSettings")]
        private static void WritePointTypeDefinition (
            MapMakerSettings mapMakerSettings, 
            CGpsMapperGeneralFileWriter file, 
            PointTypeRegistration registration)
        {
            // constant labels are placed on type definitions instead of each individual map element
            if (registration.TypeName != null)
                file.AddFormat ("string1=4,{0}", registration.TypeName);
            else if (registration.Label != null && registration.Label.IsConstant)
                file.AddFormat ("string1=4,{0}", registration.Label.BuildLabel (mapMakerSettings, null, null));

            file.AddFormat (
                "xpm=\"{0} {1} {2} {3}\"", 
                registration.Pattern.Width, 
                registration.Pattern.Height, 
                registration.Pattern.ColorsCount + 1, // first color is transparent
                registration.Pattern.ColorsCount >= 128 ? 2 : 1);

            file.AddFormat ("\"0 c none\"");

            foreach (KeyValuePair<string, string> pair in registration.Pattern.EnumerateColorDictionary())
                file.AddFormat ("\"{0} c #{1}\"", pair.Key, pair.Value);

            for (int y = 0; y < registration.Pattern.Height; y++)
            {
                StringBuilder patternLine = new StringBuilder ();

                for (int x = 0; x < registration.Pattern.Width; x++)
                {
                    char patternChar = registration.Pattern.PatternLines[y][x];
                    patternLine.Append (patternChar);
                }

                file.AddFormat ("\"{0}\"", patternLine);
            }
        }

        private static void WritePolygonTypeDefinition (
            MapMakerSettings mapMakerSettings, 
            CGpsMapperGeneralFileWriter file, 
            AreaTypeRegistration registration)
        {
            // constant labels are placed on type definitions instead of each individual map element
            if (registration.Label != null && registration.Label.IsConstant)
                file.AddFormat ("string1=4,{0}", registration.Label.BuildLabel (mapMakerSettings, null, null));

            bool bitmapUsed = registration.Pattern.Height > 0;

            file.AddFormat ("xpm=\"{0} {1} {2} {3}\"", 32, 32, 4, 1);

            List<string> realColors = new List<string>();
            foreach (string color in registration.Pattern.EnumerateColorList())
                realColors.Add(color);

            // add a transparent color if only one color was specified
            if (realColors.Count == 1)
                realColors.Insert(0, null);

            // if only the 2 colors were specified, reuse them for the nighttime scheme
            if (realColors.Count == 2)
            {
                realColors.Add(realColors[0]);
                realColors.Add(realColors[1]);
            }

            for (int i = 0; i < realColors.Count; i++)
            {
                if (realColors[i] != null)
                    file.AddFormat ("\"{0} c #{1}\"", i, realColors[i]);
                else
                    file.AddFormat("\"{0} c none\"", i);
            }

            // if the pattern is not specified, we should still fill the 32x32 box with the main color
            // otherwise the pattern will look like garbage (this is a cgpsmapper bug?)
            if (false == bitmapUsed)
            {
                for (int i = 0; i < 32; i++)
                    file.AddFormat("\"{0}\"", new string('1', 32));
            }
            else
            {
                for (int i = 0; i < 32; i++)
                {
                    StringBuilder normalizedPatternLine = new StringBuilder ();

                    for (int j = 0; j < 32; j++)
                    {
                        char patternChar = registration.Pattern.PatternLines[i % registration.Pattern.Height][j % registration.Pattern.Width];
                        normalizedPatternLine.Append(patternChar);
                    }

                    file.AddFormat ("\"{0}\"", normalizedPatternLine);
                }
            }
        }

        private static void WritePolylineTypeDefinition (
            MapMakerSettings mapMakerSettings, 
            CGpsMapperGeneralFileWriter file, 
            LineTypeRegistration registration)
        {
            int colorsCount = registration.Pattern.ColorsCount;
            if (colorsCount == 0)
                return;

            bool bitmapUsed = registration.Pattern.Height > 0;

            file.AddSection ("_line")
                .AddFormat (";rule={0}", registration.RuleName)
                .AddFormat ("Type=0x{0:x}", registration.TypeId)
                ;

            if (false == bitmapUsed)
            {
                file.Add ("LineWidth", registration.Width);
                file.Add ("BorderWidth", registration.BorderWidth);
            }

            // constant labels are placed on type definitions instead of each individual map element
            if (registration.Label != null && registration.Label.IsConstant)
                file.AddFormat ("string1=4,{0}", registration.Label.BuildLabel (mapMakerSettings, null, null));

            int patternWidth = 0;
            int patternHeight = 0;
            if (bitmapUsed)
            {
                patternWidth = 32;
                patternHeight = registration.Pattern.Height;
            }

            file.AddFormat (
                "xpm=\"{0} {1} {2} {3}\"", 
                patternWidth, 
                patternHeight, 
                Math.Max (2, registration.Pattern.ColorsCount), 1);

            if (registration.Pattern.ColorsCount == 1)
            {
                file.AddFormat ("\"1 c #{0}\"", registration.Pattern.GetColorByIndex(0))
                    .AddFormat ("\"0 c none\"");
            }
            else
            {
                for (int i = 0; i < registration.Pattern.ColorsCount; i++)
                    file.AddFormat ("\"{0} c #{1}\"", i, registration.Pattern.GetColorByIndex(i));
            }

            for (int y = 0; y < registration.Pattern.Height; y++)
            {
                StringBuilder normalizedPatternLine = new StringBuilder ();

                for (int x = 0; x < 32; x++)
                {
                    char patternChar =
                        registration.Pattern.PatternLines[y][x % registration.Pattern.Width];
                    normalizedPatternLine.Append(patternChar);
                }

                file.AddFormat ("\"{0}\"", normalizedPatternLine);
            }
        }

        public const string PolishTypeFile = "PolishTypeFile";
    }
}