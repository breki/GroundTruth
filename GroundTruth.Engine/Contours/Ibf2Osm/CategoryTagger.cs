using System;
using System.Collections.Generic;
using Brejc.OsmLibrary;

namespace GroundTruth.Engine.Contours.Ibf2Osm
{
    public class CategoryTagger : IContourOsmWayTagger
    {
        public CategoryTagger(int majorElevation, int mediumElevation)
        {
            categories.Add(majorElevation);
            categories.Add(mediumElevation);

            categoriesValueNames.Add("elevation_major");
            categoriesValueNames.Add("elevation_medium");
            categoriesValueNames.Add("elevation_minor");
        }

        public void Tag(OsmWay way, short elevation)
        {
            for (int i = 0; i < categories.Count; i++)
            {
                if ((elevation % categories[i]) == 0)
                {
                    way.SetTag("contour_ext", categoriesValueNames[i]);
                    return;
                }
            }

            way.SetTag ("contour_ext", categoriesValueNames[categoriesValueNames.Count - 1]);
        }

        private readonly List<int> categories = new List<int> ();
        private List<string> categoriesValueNames = new List<string>();
    }
}