using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Flubu;
using Flubu.Builds;
using Flubu.Builds.VSSolutionBrowsing;
using Flubu.Targeting;
using Flubu.Tasks.FileSystem;

//css_ref Flubu.dll;
//css_ref Flubu.Contrib.dll;
//css_ref log4net.dll;
//css_ref ICSharpCode.SharpZipLib.dll;

namespace BuildScripts
{
    public class BuildScript
    {
        public static int Main(string[] args)
        {
            TargetTree targetTree = new TargetTree ();
            BuildTargets.FillBuildTargets (targetTree);

            targetTree.GetTarget ("fetch.build.version")
                .Do (TargetFetchBuildVersion);

            targetTree.AddTarget ("rebuild")
                .SetDescription ("Compiles the code and runs tests.")
                .SetAsDefault ().DependsOn ("compile", "fxcop", "unit.tests", "package");

            targetTree.AddTarget ("tests")
                .SetDescription ("Runs tests on the project")
                .Do (r =>
                {
                    int testCounter = 0;
                    BuildTargets.TargetRunTests(r, "GroundTruth.Tests", null, ref testCounter);
                }).DependsOn ("load.solution");

            targetTree.AddTarget ("package")
                .SetDescription ("Packages all the build products into ZIP files.")
                .Do(TargetPackage)
                .SetAsDefault ().DependsOn ("load.solution");

            //runner.AddTarget ("package.source")
            //    .SetDescription ("Packages the souce code into ZIP file")
            //    .Do (TargetPackageSource).DependsOn ("load.solution");

            using (TaskSession session = new TaskSession (new SimpleTaskContextProperties (), args, targetTree))
            {
                BuildTargets.FillDefaultProperties (session);
                session.Start (BuildTargets.OnBuildFinished);

                session.AddLogger (new MulticoloredConsoleLogger (Console.Out));

                session.Properties.Set (BuildProps.CompanyName, "igorbrejc.net");
                session.Properties.Set (BuildProps.CompanyCopyright, "Copyright (C) 2008-2012 Igor Brejc.");
                session.Properties.Set (BuildProps.ProductId, "GroundTruth");
                session.Properties.Set (BuildProps.ProductName, "GroundTruth");
                session.Properties.Set (BuildProps.SolutionFileName, "GroundTruth.sln");
                session.Properties.Set (BuildProps.VersionControlSystem, VersionControlSystem.Mercurial);

                try
                {
                    // actual run
                    if (args.Length == 0)
                        targetTree.RunTarget (session, targetTree.DefaultTarget.TargetName);
                    else
                    {
                        string targetName = args[0];
                        if (false == targetTree.HasTarget (targetName))
                        {
                            session.WriteError ("ERROR: The target '{0}' does not exist", targetName);
                            targetTree.RunTarget (session, "help");
                            return 2;
                        }

                        targetTree.RunTarget (session, args[0]);
                    }

                    session.Complete ();

                    return 0;
                }
                catch (TaskExecutionException)
                {
                    return 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine (ex);
                    return 1;
                }
            }
        }

        private static void TargetFetchBuildVersion (ITaskContext context)
        {
            Version version = BuildTargets.FetchBuildVersionFromFile (context);
            version = new Version (version.Major, version.Minor, BuildTargets.FetchBuildNumberFromFile (context));
            context.Properties.Set (BuildProps.BuildVersion, version);
            context.WriteInfo ("The build version will be {0}", version);
        }

        private static void TargetPackage(ITaskContext context)
        {
            throw new NotImplementedException();
            //context
            //    .BuildProducts
            //    .AddProject ("core", "GroundTruth", String.Empty, false)
            //    .AddFile ("core", "ChangeLog.txt", "ChangeLog.txt")
            //    .AddFile ("core", "Copying.txt", "Copying.txt")
            //    .AddFile ("core", "Copying.txt", "licenses/license-GroundTruth.txt")
            //    .AddFile ("core", "Readme.txt", "Readme.txt")
            //    .AddFile ("misc", @"..\lib\Brejc.Geo\Copying.txt", @"licenses\license-Brejc.Geo.txt")
            //    .AddFile ("misc", @"..\lib\Castle\ASL - Apache Software Foundation License.txt", @"licenses\license-Castle.txt")
            //    .AddFile ("misc", @"..\lib\ICSharpCode.SharpZipLib.2.0\COPYING.txt", @"licenses\license-SharpLibZip.txt")
            //    .AddFile ("misc", @"..\lib\log4net-1.2.10\log4net.LICENSE.txt", @"licenses\license-log4net.txt")
            //    .AddFile ("misc", @"..\lib\PowerCollections\Binaries\License.txt", @"licenses\license-PowerCollections.txt")
            //    .AddFile ("misc", @"..\lib\QuickGraph\license.txt", @"licenses\license-QuickGraph.txt")
            //    .AddFile ("linux", @"Misc\GroundTruth.sh", "GroundTruth.sh");

            //runner
            //    .PackageBuildProduct ("GroundTruth-{1}.zip", "GroundTruth-{1}", "core", "misc", "linux")
            //    .CopyBuildProductToCCNet(@"packages\GroundTruth\GroundTruth-latest.zip")
            //    .CopyBuildProductToCCNet(@"packages\GroundTruth\{2}.{3}\{4}");
        }

        //private static void TargetPackageSource (ConcreteBuildRunner runner)
        //{
        //    List<string> files = new List<string>();

        //    string solutionDirectoryPath = Path.GetFullPath (
        //        Path.Combine (
        //            Environment.CurrentDirectory, 
        //            runner.Solution.SolutionDirectoryPath));

        //    files.Add(Path.GetFullPath(runner.Solution.SolutionFileName));

        //    Regex excludeFilter = new Regex(@"\.snk$");

        //    runner
        //        .Solution
        //        .ForEachProject(p => AddProjectFiles(p, solutionDirectoryPath, files, excludeFilter));

        //    ZipFilesTask zipFilesTask = new ZipFilesTask(
        //        Path.Combine(runner.BuildPackagesDir, runner.ProductName + "-sources.zip"), 
        //        solutionDirectoryPath,
        //        files);
        //    zipFilesTask.CompressionLevel = 9;
        //    zipFilesTask.ZipFileHeaderCallback = GetLicenseHeaderForSourceFiles;
        //    runner.RunTask(zipFilesTask);
        //}

        private static void AddProjectFiles (
            VSProjectInfo project, 
            string solutionDirectoryPath,
            ICollection<string> files,
            Regex excludeFilter)
        {
            if (project is VSProjectWithFileInfo)
            {
                VSProjectWithFileInfo projectWithFileInfo = (VSProjectWithFileInfo) project;

                files.Add (projectWithFileInfo.ProjectFileNameFull.ToFullPath().ToString());

                foreach (VSProjectItem item in projectWithFileInfo.Project.Items)
                {
                    if (item.ItemType == VSProjectItem.CompileItem
                        || item.ItemType == VSProjectItem.Content
                        || item.ItemType == VSProjectItem.NoneItem)
                    {
                        if (item.ItemProperties.ContainsKey ("Link"))
                            continue;

                        string itemPath = Path.GetFullPath (
                            Path.Combine (
                                solutionDirectoryPath,
                                Path.Combine (
                                    projectWithFileInfo.ProjectDirectoryPath.ToString(),
                                    item.Item)));

                        if (excludeFilter.IsMatch (itemPath))
                            continue;

                        files.Add (itemPath);
                    }
                }
            }
            else if (project is VSSolutionFilesInfo)
            {
                VSSolutionFilesInfo solutionFilesInfo = (VSSolutionFilesInfo) project;
                foreach (string file in solutionFilesInfo.Files)
                {
                    string fullPath = Path.GetFullPath(Path.Combine(solutionDirectoryPath, file));

                    if (excludeFilter.IsMatch (fullPath))
                        continue;

                    files.Add (fullPath);
                }
            }
        }

        private static string GetLicenseHeaderForSourceFiles (string fileName)
        {
            if (fileName.ToLower().EndsWith(".cs"))
            {
                StringBuilder licenseHeader =  new StringBuilder();
                licenseHeader.AppendFormat(
                    "// Copyright (C) 2008-{0} by Igor Brejc. All rights reserved.",
                    DateTime.Now.Year);
                licenseHeader.AppendLine();
                licenseHeader.AppendFormat(
                    "// Released under the terms of the GNU General Public License version 3 or later.");
                licenseHeader.AppendLine();
                licenseHeader.AppendLine();

                return licenseHeader.ToString();
            }

            return String.Empty;
        }
    }
}