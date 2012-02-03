using System;
using System.Collections.Generic;
using System.IO;
using GroundTruth.Engine;
using GroundTruth.Engine.MapDataSources;
using GroundTruth.Engine.Tasks;
using MbUnit.Framework;
using Rhino.Mocks;

namespace GroundTruth.Tests
{
    [TestFixture]
    public class ContoursMapGenerationTest
    {
        [Test]
        [Row(true)]
        [Row(false)]
        public void Test(bool metric)
        {
            // prepare the temp directory
            MapMaker mapMaker = new MapMaker();
            mapMaker.PrepareTempDirectory();

            if (false == metric)
                mapMaker.MapMakerSettings.ContoursRenderingRulesSource = "Rules/ContoursRulesFeet.txt";

            ContoursDataSource dataSource = new ContoursDataSource(@"..\..\..\..\Data\Samples\IBF\Maribor.ibf");

            IPolishMapFileCreator creator = new DefaultPolishMapFileCreator(mapMaker.MapMakerSettings);

            List<ProductFile> productFiles = new List<ProductFile>();

            ITaskRunner runner = MockRepository.GenerateStub<ITaskRunner> ();
            runner.Expect(r => r.RegisterProductFile(null)).IgnoreArguments()
                .Do((Action<ProductFile>)(productFiles.Add)).Repeat.Any();

            dataSource.AnalyzeData (mapMaker.MapMakerSettings);
            dataSource.GeneratePolishMapFiles (runner, creator);

            int polylinesCount = 0;

            foreach (ProductFile file in productFiles)
            {
                string mapContent = File.ReadAllText(file.ProductFileName);
                polylinesCount += GetCountOf(mapContent, "[POLYLINE]");

                if (metric)
                {
                    Assert.IsTrue(mapContent.Contains("Label=800"));
                    Assert.IsTrue(mapContent.Contains("Label=820"));
                    Assert.IsFalse(mapContent.Contains("Label=790"));
                }
                else
                {
                    Assert.IsTrue (mapContent.Contains ("Label=450"));
                    Assert.IsTrue (mapContent.Contains ("Label=600"));
                    Assert.IsFalse (mapContent.Contains ("Label=1000"));
                }
            }

            if (metric)
                Assert.AreEqual (9602, polylinesCount);
            else
                Assert.AreEqual (1968, polylinesCount);
        }

        private static int GetCountOf (string content, string substring)
        {
            int i = 0;
            int count = 0;
            while (true)
            {
                i = content.IndexOf (substring, i);
                if (i == -1)
                    break;
                count++;
                i++;
            }

            return count;
        }
    }
}