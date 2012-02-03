using System.IO;
using System.Text;
using Brejc.Geometry;
using GroundTruth.Engine;
using GroundTruth.Engine.PolishMaps;
using GroundTruth.Engine.Rules;
using MbUnit.Framework;
using System.Text.RegularExpressions;

namespace GroundTruth.Tests
{
    [TestFixture]
    [TestsOn(typeof(WikiRulesParser))]
    public class WikiRulesParserTests : GroundTruthFixtureBase
    {
        [Test]
        public void ParseRules()
        {
            RenderingRuleSet rules;
            TypesRegistry typesRegistry = ParseRules(@"..\..\..\GroundTruth\Rules\HikingMapRules.txt", out rules);

            Assert.AreEqual(51, rules.RulesCount);

            RenderingRule rule;
            int i = 0;

            rule = rules[i++];
            Assert.AreEqual ("Land", rule.RuleName);
            Assert.AreEqual (RenderingRuleTargets.Areas, rule.Targets);

            rule = rules[i++];
            Assert.AreEqual ("Marsh", rule.RuleName);
            Assert.AreEqual (RenderingRuleTargets.Areas, rule.Targets);

            rule = rules[i++];
            Assert.AreEqual ("Forest", rule.RuleName);
            Assert.AreEqual (RenderingRuleTargets.Areas, rule.Targets);

            rule = rules[i++];
            Assert.AreEqual ("Park", rule.RuleName);
            Assert.AreEqual (RenderingRuleTargets.Areas, rule.Targets);

            rule = rules[i++];
            Assert.AreEqual ("Farm", rule.RuleName);
            Assert.AreEqual (RenderingRuleTargets.Areas, rule.Targets);

            rule = rules[i++];
            Assert.AreEqual ("Farmyard", rule.RuleName);
            Assert.AreEqual (RenderingRuleTargets.Areas, rule.Targets);

            rule = rules[i++];
            Assert.AreEqual ("Fell", rule.RuleName);
            Assert.AreEqual (RenderingRuleTargets.Areas, rule.Targets);

            rule = rules[i++];
            Assert.AreEqual ("Residential", rule.RuleName);
            Assert.AreEqual (RenderingRuleTargets.Areas, rule.Targets);

            rule = rules[i++];
            Assert.AreEqual ("Water", rule.RuleName);
            Assert.AreEqual (RenderingRuleTargets.Areas, rule.Targets);

            // check point patterns
            Assert.AreEqual(11, typesRegistry.Patterns.Count);
            PatternDefinition patternDefinition = typesRegistry.Patterns["IconPeak"];
            Assert.IsNotNull(patternDefinition);
        }

        [Test]
        public void MakeSureOptionsAreParsed()
        {
            RenderingRuleSet rules;
            TypesRegistry typesRegistry = ParseRules (@"..\..\..\GroundTruth\Rules\HikingMapRules.txt", out rules);

            Assert.AreEqual (new GisColor (unchecked ((int)0xffFFEFBF)), rules.Options.LandBackgroundColor);
            Assert.AreEqual("1.6", rules.Options.RulesVersion);
        }

        [Test]
        public void ParseRules2 ()
        {
            RenderingRuleSet rules;
            TypesRegistry typesRegistry = ParseRules (@"..\..\..\GroundTruth\Rules\DefaultRules.txt", out rules);
        }

        [Test]
        public void ParseCharConversionTable ()
        {
            CharactersConversionDictionary charactersConversionDictionary = new CharactersConversionDictionary ();

            using (Stream stream = File.OpenRead (@"..\..\..\GroundTruth\Rules\CharacterConversionTable.txt"))
            {
                WikiRulesParser parser = new WikiRulesParser (
                    stream,
                    null,
                    charactersConversionDictionary,
                    SerializersRegistry);
                parser.Parse ();
            }

            // check character conversion
            Assert.AreEqual ("Cevapcici", charactersConversionDictionary.Convert ("Čevapčići"));
        }

        [Test]
        public void ParseGarminStandardTypes()
        {
            RenderingRuleSet rules;
            TypesRegistry typesRegistry = ParseRules (@"..\..\..\GroundTruth\Rules\StandardGarminTypes.txt", out rules);

            Assert.IsNotNull (typesRegistry.GarminPointTypesDictionary);
            Assert.AreEqual (79, typesRegistry.GarminPointTypesDictionary.TypesCount);
            Assert.AreEqual (1, typesRegistry.GarminPointTypesDictionary.GetType ("City (Capital)"));
            Assert.AreEqual (20480, typesRegistry.GarminPointTypesDictionary.GetType ("Drinking Water"));

            Assert.IsNotNull (typesRegistry.GarminLineTypesDictionary);
            Assert.AreEqual (41, typesRegistry.GarminLineTypesDictionary.TypesCount);
            Assert.AreEqual (5, typesRegistry.GarminLineTypesDictionary.GetType ("Arterial Road-thick"));

            Assert.IsNotNull (typesRegistry.GarminAreaTypesDictionary);
            Assert.AreEqual (28, typesRegistry.GarminAreaTypesDictionary.TypesCount);
            Assert.AreEqual (1, typesRegistry.GarminAreaTypesDictionary.GetType ("City"));
        }

        [Test]
        public void PatternRegexTest()
        {
            Regex regexLinePattern = new Regex (@"^((?<line>[01]{32})\s*)+$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            string patternText = @"11111000111110001111100011111000
11111000111110001111100011111011
11111000111110001111100011111100";
            MatchCollection matches = regexLinePattern.Matches (patternText);

            Assert.AreEqual(1, matches.Count);
            Match match = matches[0];
            Group group = match.Groups["line"];
            Assert.AreEqual(3, group.Captures.Count);
            Assert.AreEqual ("11111000111110001111100011111000", group.Captures[0].Value);
            Assert.AreEqual ("11111000111110001111100011111011", group.Captures[1].Value);
            Assert.AreEqual ("11111000111110001111100011111100", group.Captures[2].Value);
        }

        [Test]
        public void ParseContourRules ()
        {
            TypesRegistry typesRegistry = new TypesRegistry ();

            ContoursElevationRuleMap rules;

            using (Stream stream = File.OpenRead (@"..\..\..\GroundTruth\Rules\ContoursRulesMetric.txt"))
            {
                ContoursRulesParser parser = new ContoursRulesParser();
                rules = parser.Parse (stream);
            }

            Assert.IsNotNull(rules.FindMatchingRule(1100));
            Assert.IsNotNull(rules.FindMatchingRule(20));
            Assert.IsNotNull(rules.FindMatchingRule(50));
            Assert.IsNull(rules.FindMatchingRule(5));
        }

        [Test, ExpectedArgumentException ("Incompatibile rendering rules version.")]
        public void ExceptionIfRulesVersionIsIncompatible()
        {
            TypesRegistry typesRegistry = new TypesRegistry ();

            string rulesText = @"== Rendering Rules ==

=== Options ===
{| class='wikitable' border='1' cellspacing='0' cellpadding='2' 
|- style='background-color:#F8F4C2'
! Option
! Value
! Comment 
|-
| RulesVersion || 1.7 || Minimal version of [[GroundTruth]] needed to use these rules
|-
| LandBackgroundColor || #218CFF || Color of the map background. If not set, the default unit color will be used
|-
|}
";

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(rulesText)))
            {
                WikiRulesParser parser = new WikiRulesParser (
                    stream,
                    typesRegistry,
                    null,
                    SerializersRegistry);
                parser.Parse ();
                RenderingRuleSet rules = parser.Rules;
            }
        }

        [FixtureSetUp]
        public void TestFixtureSetup ()
        {
            log4net.Config.XmlConfigurator.Configure ();
        }

        private TypesRegistry ParseRules(string rulesFile, out RenderingRuleSet rules)
        {
            TypesRegistry typesRegistry = new TypesRegistry();

            using (Stream stream = File.OpenRead (rulesFile))
            {
                WikiRulesParser parser = new WikiRulesParser(
                    stream, 
                    typesRegistry,
                    null,
                    SerializersRegistry);
                parser.Parse();
                rules = parser.Rules;
            }

            return typesRegistry;
        }
    }
}