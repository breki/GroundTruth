using System;
using System.Collections.Generic;
using Brejc.OsmLibrary;
using GroundTruth.Engine.LabelBuilding;
using MbUnit.Framework;
using GroundTruth.Engine.Tasks;

namespace GroundTruth.Tests
{
    [TestFixture]
    public class LabelBuilderTests
    {
        [Test]
        [Row (@"name ++ ele ""($1f $elevation)""", 2)]
        [Row (@"name ++ ""sdsd"" ++ ele ""($1f $elevation)""", 3)]
        [Row (@"""($1f $elevation)""", 1)]
        [Row (@"   ", 0)]
        [Row (@" ++ ++ ++++ ", 0)]
        public void ParseValidExamples (string expression, int expectedElementsCount)
        {
            LabelExpressionParser parser = new LabelExpressionParser();
            LabelExpression parsedExpression = parser.Parse(expression, 0);

            Assert.AreEqual(expectedElementsCount, parsedExpression.ElementsCount);
        }

        [Test]
        public void ParseElevationSample()
        {
            string expression = @"name ++ ""sdsd"" ++ ele ""($1f $elevation)""";

            LabelExpressionParser parser = new LabelExpressionParser ();
            LabelExpression parsedExpression = parser.Parse (expression, 0);

            int i = 0;
            Assert.IsInstanceOfType (typeof (OsmKeyLabelExpressionElement), parsedExpression.Elements [i]);
            OsmKeyLabelExpressionElement osmKeyLabelExpressionElement = (OsmKeyLabelExpressionElement) parsedExpression.Elements[i++];
            Assert.AreEqual("name", osmKeyLabelExpressionElement.KeyName);
            Assert.IsNull(osmKeyLabelExpressionElement.ConditionalElement);

            Assert.IsInstanceOfType (typeof (FormatLabelExpressionElement), parsedExpression.Elements [i]);
            FormatLabelExpressionElement formatLabelExpressionElement = (FormatLabelExpressionElement)parsedExpression.Elements[i++];
            Assert.AreEqual("sdsd", formatLabelExpressionElement.Format);

            Assert.IsInstanceOfType (typeof (OsmKeyLabelExpressionElement), parsedExpression.Elements [i]);
            osmKeyLabelExpressionElement = (OsmKeyLabelExpressionElement)parsedExpression.Elements[i++];
            Assert.AreEqual ("ele", osmKeyLabelExpressionElement.KeyName);
            Assert.IsInstanceOfType(typeof (FormatLabelExpressionElement), osmKeyLabelExpressionElement.ConditionalElement);
            formatLabelExpressionElement = (FormatLabelExpressionElement)osmKeyLabelExpressionElement.ConditionalElement;
            Assert.AreEqual ("($1f $elevation)", formatLabelExpressionElement.Format);
        }

        [Test]
        public void ParseRelationLabel()
        {
            string expression = @"relation:name";

            LabelExpressionParser parser = new LabelExpressionParser ();
            LabelExpression parsedExpression = parser.Parse (expression, 0);

            int i = 0;
            Assert.IsInstanceOfType (typeof (OsmKeyLabelExpressionElement), parsedExpression.Elements[i]);
            OsmKeyLabelExpressionElement osmKeyLabelExpressionElement = (OsmKeyLabelExpressionElement)parsedExpression.Elements[i++];
            Assert.AreEqual ("relation:name", osmKeyLabelExpressionElement.KeyName);
            Assert.IsNull (osmKeyLabelExpressionElement.ConditionalElement);
        }

        [Test]
        public void BuildElevationSample()
        {
            MapMakerSettings mapMakerSettings = new MapMakerSettings();
            mapMakerSettings.CharactersConversionDictionary.AddConversion('Č', "C");
            mapMakerSettings.CharactersConversionDictionary.AddConversion('č', "c");

            const string expression = @"name ++ "" sdsd"" ++ ele "" ($1f$elevation)""";

            LabelExpressionParser parser = new LabelExpressionParser ();
            LabelExpression parsedExpression = parser.Parse (expression, 0);

            OsmNode osmNode = new OsmNode(1, 10, 10);
            osmNode.SetTag("name", "Veliki vrh Čačka");
            osmNode.SetTag("ele", "1433");

            string label = parsedExpression.BuildLabel(mapMakerSettings, osmNode, null);

            Assert.AreEqual("Veliki vrh Cacka sdsd (~[0x1f]1433)", label);
        }

        /// <summary>
        /// Tests what happens if the "ele" tag is tagged with a non-numeric value. The build
        /// labeling should not fail, it should just ignore the "ele" part of the label.
        /// </summary>
        [Test]
        public void BuildElevationSampleWithError ()
        {
            MapMakerSettings mapMakerSettings = new MapMakerSettings ();
            mapMakerSettings.CharactersConversionDictionary.AddConversion ('Č', "C");
            mapMakerSettings.CharactersConversionDictionary.AddConversion ('č', "c");

            const string expression = @"name ++ "" sdsd"" ++ ele "" ($1f$elevation)""";

            LabelExpressionParser parser = new LabelExpressionParser ();
            LabelExpression parsedExpression = parser.Parse (expression, 0);

            OsmNode osmNode = new OsmNode (1, 10, 10);
            osmNode.SetTag ("name", "Veliki vrh Čačka");
            osmNode.SetTag ("ele", "143sds3");

            string label = parsedExpression.BuildLabel (mapMakerSettings, osmNode, null);

            Assert.AreEqual ("Veliki vrh Cacka sdsd", label);
        }

        [Test]
        public void BuildLabelWithRelationTags()
        {
            const string expression = @"relation:name ++ name";

            LabelExpressionParser parser = new LabelExpressionParser ();
            LabelExpression parsedExpression = parser.Parse (expression, 0);

            OsmObjectMother mother = new OsmObjectMother();
            mother
                .AddRelation ()
                .Tag ("name", "My name is Relation")
                .AddWay (5)
                .Tag("name", "My name is Way")
                .AddToRelation ("friend");

            MapMakerSettings mapMakerSettings = new MapMakerSettings ();
            string label = parsedExpression.BuildLabel (mapMakerSettings, mother.CurrentObject, mother.CurrentRelation);

            Assert.AreEqual ("My name is RelationMy name is Way", label);            
        }

        [Test]
        public void BuildLabelWithRelationTagsAndConditionals ()
        {
            const string expression = @"relation:name ""blabla""";

            LabelExpressionParser parser = new LabelExpressionParser ();
            LabelExpression parsedExpression = parser.Parse (expression, 0);

            OsmObjectMother mother = new OsmObjectMother ();
            mother
                .AddRelation();

            MapMakerSettings mapMakerSettings = new MapMakerSettings ();
            string label = parsedExpression.BuildLabel (mapMakerSettings, mother.CurrentObject, mother.CurrentRelation);

            Assert.AreEqual (String.Empty, label);
        }

        [Test]
        [Row(@"name ""a$value""", "aWay")]
        [Row(@"name ""a$uppercase""", "aWAY")]
        public void BuildVariousLabels (string labelExpression, string expectedLabel)
        {
            LabelExpressionParser parser = new LabelExpressionParser ();
            LabelExpression parsedExpression = parser.Parse (labelExpression, 0);

            OsmObjectMother mother = new OsmObjectMother ();
            mother
                .AddRelation ()
                .Tag ("name", "Relation")
                .AddWay (5)
                .Tag ("name", "Way")
                .AddToRelation ("friend");

            MapMakerSettings mapMakerSettings = new MapMakerSettings ();
            string label = parsedExpression.BuildLabel (mapMakerSettings, mother.CurrentObject, mother.CurrentRelation);

            Assert.AreEqual (expectedLabel, label);                        
        }
    }
}