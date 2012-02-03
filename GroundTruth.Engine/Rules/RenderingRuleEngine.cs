using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Brejc.OsmLibrary;
using GroundTruth.Engine.MapDataSources;
using GroundTruth.Engine.PolishMaps;
using GroundTruth.Engine.Tasks;
using Wintellect.PowerCollections;
using log4net;

namespace GroundTruth.Engine.Rules
{
    public class RenderingRuleEngine
    {
        public RenderingRuleEngine(MapDataAnalysis analysis)
        {
            this.analysis = analysis;
        }

        [SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "mapMaker")]
        public void GenerateMapContent (
            OsmDataSource osmDataSource,
            CGpsMapperMapWriter mapWriter,
            MapMakerSettings mapMakerSettings,
            bool justDetectLevelsAndTypesUsed)
        {
            this.osmDataSource = osmDataSource;
            this.mapMakerSettings = mapMakerSettings;
            this.mapWriter = mapWriter;
            this.justDetectLevelsAndTypesUsed = justDetectLevelsAndTypesUsed;

            multipolygonRelationsProcessor = new MultipolygonRelationsProcessor (osmDataSource.OsmDatabase);

            GenerateMapContentPrivate();
        }

        protected MultipolygonRelationsProcessor MultipolygonRelationsProcessor
        {
            get { return multipolygonRelationsProcessor; }
        }

        [SuppressMessage ("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        static private int GetOsmObjectLayerNumber (OsmObjectBase osmObject)
        {
            if (osmObject.HasTag ("layer"))
            {
                string layerString = osmObject.GetTagValue ("layer");

                try
                {
                    int layer = int.Parse (layerString, System.Globalization.CultureInfo.InvariantCulture);
                    return layer;
                }
                    // ignore invalid layer values
                catch (Exception) { }
            }

            // the default layer is 0
            return 0;
        }

        private void GenerateMapContentPrivate()
        {
            if (mapMakerSettings.RenderingRules == null)
                throw new InvalidOperationException ("You have not set the rendering rules.");

            // first search for multipolygon relations
            FindMultipolygonRelations();

            // find the layers used in the OSM database
            OrderedSet<int> layers = FindLayersUsed(osmDataSource.OsmDatabase);
            ProcessLayers(layers);
            ProcessNodes();
        }

        private void ProcessNodes()
        {
            foreach (RenderingRule rule in mapMakerSettings.RenderingRules.EnumerateRules ())
            {
                currentRule = rule;

                // a set of OSM object which are already consumed by a relation-targeted rule
                // so we should not consume them again for the same rule
                consumedNodes = new HashSet<long> ();

                if (currentRule.TargetsRelation)
                    ProcessRuleForRelationNodes();

                if ((currentRule.Targets & RenderingRuleTargets.Nodes) != 0)
                    ProcessNodeRule();
            }
        }

        private void ProcessNodeRule()
        {
            foreach (OsmNode node in osmDataSource.OsmDatabase.Nodes)
            {
                if (consumedNodes.Contains (node.ObjectId))
                    continue;

                if (ProcessNode (node, null))
                    break;
            }
        }

        private void ProcessRuleForRelationNodes()
        {
            foreach (OsmRelation relation in osmDataSource.OsmDatabase.Relations)
            {
                foreach (OsmRelationMember member in relation.EnumerateMembers ())
                {
                    if (member.MemberReference.ReferenceType == OsmReferenceType.Node
                        && ((currentRule.Targets & RenderingRuleTargets.Nodes) != 0))
                    {
                        if (osmDataSource.OsmDatabase.HasNode (member.MemberReference.ReferenceId))
                        {
                            OsmNode node =
                                osmDataSource.OsmDatabase.GetNode (member.MemberReference.ReferenceId);

                            if (consumedNodes.Contains (node.ObjectId))
                                continue;

                            consumedNodes.Add (node.ObjectId);

                            if (true == ProcessNode (node, relation))
                                break;
                        }
                    }
                }
            }
        }

        private void ProcessLayers(OrderedSet<int> layers)
        {
            foreach (int layerToCover in layers)
                ProcessLayer(layerToCover);
        }

        private void ProcessLayer(int layerToCover)
        {
            currentLayer = layerToCover;

            foreach (RenderingRule rule in mapMakerSettings.RenderingRules.EnumerateRules())
            {
                // a set of OSM object which are already consumed by a relation-targeted rule
                // so we should not consume them again for the same rule
                ProcessRule(rule);
            }
        }

        private void ProcessRule(RenderingRule rule)
        {
            consumedWays = new HashSet<long>();
            consumedAreas = new HashSet<long>();
            currentRule = rule;

            if (rule.TargetsRelation)
                ProcessRelationRule();

            if ((rule.Targets & RenderingRuleTargets.Areas) != 0)
                ProcessAreaRule();

            if ((rule.Targets & RenderingRuleTargets.Ways) != 0)
                ProcessWayRule();
        }

        private void ProcessRelationRule()
        {
            foreach (OsmRelation relation in osmDataSource.OsmDatabase.Relations)
            {
                foreach (OsmRelationMember member in relation.EnumerateMembers())
                {
                    if (member.MemberReference.ReferenceType == OsmReferenceType.Way)
                    {
                        if (osmDataSource.OsmDatabase.HasWay(member.MemberReference.ReferenceId))
                        {
                            OsmWay way = osmDataSource.OsmDatabase.GetWay(member.MemberReference.ReferenceId);
                            if ((currentRule.Targets & RenderingRuleTargets.Areas) != 0)
                            {
                                if (consumedAreas.Contains(way.ObjectId))
                                    continue;

                                int layer = GetOsmObjectLayerNumber (way);
                                if (layer != currentLayer)
                                    continue;

                                if (true == ProcessArea(way, relation, currentRule, justDetectLevelsAndTypesUsed, consumedAreas))
                                    break;
                            }

                            if ((currentRule.Targets & RenderingRuleTargets.Ways) != 0)
                            {
                                if (consumedWays.Contains(way.ObjectId))
                                    continue;

                                int layer = GetOsmObjectLayerNumber (way);
                                if (layer != currentLayer)
                                    continue;

                                if (true == ProcessWay(way, relation))
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void ProcessAreaRule()
        {
            foreach (OsmWay way in osmDataSource.OsmDatabase.Ways)
            {
                if (consumedAreas.Contains(way.ObjectId))
                    continue;

                int layer = GetOsmObjectLayerNumber (way);
                if (layer != currentLayer)
                    continue;

                if (true == ProcessArea(way, null, currentRule, justDetectLevelsAndTypesUsed, consumedAreas))
                    break;
            }
        }

        private void ProcessWayRule()
        {
            foreach (OsmWay way in osmDataSource.OsmDatabase.Ways)
            {
                if (consumedWays.Contains(way.ObjectId))
                    continue;

                int layer = GetOsmObjectLayerNumber (way);
                if (layer != currentLayer)
                    continue;

                if (true == ProcessWay(way, null))
                    break;
            }
        }

        private void FindMultipolygonRelations()
        {
            foreach (OsmRelation relation in osmDataSource.OsmDatabase.Relations)
            {
                if (relation.HasTag ("type", "multipolygon"))
                {
                    // remember it for later processing
                    MultipolygonRelationsProcessor.RememberMultipolygonRelation (relation);
                }
            }
        }

        [SuppressMessage ("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "elementLayer")]
        private void AddRenderElementCommandForWay (
            OsmWay way,
            OsmRelation parentRelation,
            int elementLayer,
            RenderingRule renderingRule,
            bool isRelationsWay)
        {
            // ignore ways which are inner ways of a multipolygon relation
            if (false == multipolygonRelationsProcessor.IsWayUsedAsInnerPolygon(way.ObjectId))
            {
                // check if this way represents an outer way of multipolygon
                if (multipolygonRelationsProcessor.IsWayUsedAsOuterPolygon(way.ObjectId))
                {
                    // find all areas with holes object and apply the templates to them
                    foreach (OsmAreaWithHoles areaWithHoles in multipolygonRelationsProcessor.ListAreasWithHolesForSpecificWay(way.ObjectId))
                        ApplyTemplate (isRelationsWay, renderingRule, areaWithHoles, parentRelation);
                }
                    // otherwise just apply the templates to the way
                else
                    ApplyTemplate (isRelationsWay, renderingRule, way, parentRelation);

                mapMakerSettings.MapContentStatistics.IncrementFeaturesCount (renderingRule.RuleName);
            }
        }

        [SuppressMessage ("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "elementLayer")]
        private void AddRenderElementCommandForNode (
            OsmNode node,
            OsmRelation parentRelation,
            int elementLayer,
            RenderingRule renderingRule)
        {
            renderingRule.Template.RenderOsmObject (mapMakerSettings, analysis, osmDataSource.OsmDatabase, node, parentRelation, mapWriter);
            mapMakerSettings.MapContentStatistics.IncrementFeaturesCount(renderingRule.RuleName);
        }

        [SuppressMessage ("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "isRelationsWay")]
        private void ApplyTemplate (
            bool isRelationsWay, 
            RenderingRule renderingRule, 
            OsmObjectBase osmObject,
            OsmRelation parentRelation)
        {
            renderingRule.Template.RenderOsmObject (mapMakerSettings, analysis, osmDataSource.OsmDatabase, osmObject, parentRelation, mapWriter);
        }

        private static OrderedSet<int> FindLayersUsed (InMemoryOsmDatabase osmDatabase)
        {
            OrderedSet<int> layersUsed = new OrderedSet<int>();

            OsmDataQueryParameters queryParameters = new OsmDataQueryParameters ();
            queryParameters.FetchTags = true;
            queryParameters.SkipUntagged = true;

            foreach (OsmWay way in osmDatabase.Ways)
            {
                int layer = GetOsmObjectLayerNumber (way);
                if (false == layersUsed.Contains (layer))
                    layersUsed.Add (layer);
            }

            return layersUsed;
        }

        private bool ProcessArea(
            OsmWay area, 
            OsmRelation relation, 
            RenderingRule rule, 
            bool justDetectLevelsAndTypesUsed,
            HashSet<long> consumedAreas)
        {
            // ignore untagged ways (but not if they are part of a relation)
            if (false == area.IsTagged && relation == null)
                return false;

            // ignore ways with less than 3 nodes
            if (area.NodesCount < 3)
                return false;

            // ignore ways if they are neither designated as areas nor closed
            if (false == area.IsClosed && false == areaSelector.IsMatch(null, area))
                return false;

            int layer = GetOsmObjectLayerNumber(area);

            if (rule.IsMatch(relation, area))
            {
                consumedAreas.Add(area.ObjectId);

                // if we are just running the first run on the rule engine
                // to detect levels and types that are used in the mapWriter
                if (justDetectLevelsAndTypesUsed)
                {
                    rule.MarkHardwareLevelsUsed(analysis);
                    rule.Template.RegisterType(rule.RuleName, mapMakerSettings.TypesRegistry, false);
                    return true;
                }

                AddRenderElementCommandForWay(area, relation, layer, rule, false);
            }

            return false;
        }

        private bool ProcessNode(OsmNode node, OsmRelation relation)
        {
            // ignore untagged nodes (but not if they are part of a relation)
            if (false == node.IsTagged && relation == null)
                return false;

            int layer = GetOsmObjectLayerNumber(node);

            if (currentRule.IsMatch(relation, node))
            {
                // if we are just running the first run on the rule engine
                // to detect levels and types that are used in the mapWriter
                if (justDetectLevelsAndTypesUsed)
                {
                    currentRule.MarkHardwareLevelsUsed (analysis);
                    currentRule.Template.RegisterType (currentRule.RuleName, mapMakerSettings.TypesRegistry, false);
                    return true;
                }

                AddRenderElementCommandForNode(node, relation, layer, currentRule);
            }

            return false;
        }

        private bool ProcessWay(
            OsmWay way, 
            OsmRelation relation)
        {
            // ignore untagged ways (but not if they are part of a relation)
            if (false == way.IsTagged && relation == null)
                return false;

            // ignore ways with less than 2 nodes
            if (way.NodesCount < 2)
                return false;

            // ignore ways if they are designated as areas
            if (areaSelector.IsMatch(null, way))
                return false;

            // ignore ways which are part of the multipolygon relation
            if (multipolygonRelationsProcessor.IsWayUsedAsOuterPolygon(way.ObjectId)
                || multipolygonRelationsProcessor.IsWayUsedAsInnerPolygon(way.ObjectId))
                return false;

            int layer = GetOsmObjectLayerNumber(way);

            if (currentRule.IsMatch(relation, way))
            {
                consumedWays.Add(way.ObjectId);

                // if we are just running the first run on the rule engine
                // to detect levels and types that are used in the mapWriter
                if (justDetectLevelsAndTypesUsed)
                {
                    currentRule.MarkHardwareLevelsUsed (analysis);
                    currentRule.Template.RegisterType (currentRule.RuleName, mapMakerSettings.TypesRegistry, false);
                    return true;
                }

                AddRenderElementCommandForWay(way, relation, layer, currentRule, false);
            }

            return false;
        }

        private readonly MapDataAnalysis analysis;
        private static readonly KeyValueSelector areaSelector = new KeyValueSelector ("area", "yes");
        [SuppressMessage ("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "mapMaker")]
        private MapMakerSettings mapMakerSettings;
        private CGpsMapperMapWriter mapWriter;

        private HashSet<long> consumedAreas;
        private HashSet<long> consumedNodes;
        private HashSet<long> consumedWays;
        private int currentLayer;
        private RenderingRule currentRule;
        private bool justDetectLevelsAndTypesUsed;
        private MultipolygonRelationsProcessor multipolygonRelationsProcessor;
        private OsmDataSource osmDataSource;

        [SuppressMessage ("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        static readonly private ILog log = LogManager.GetLogger (typeof (RenderingRuleEngine));
    }
}