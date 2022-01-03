// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Linq;
using System.Threading.Tasks;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Dependencies;
using Otor.MsixHero.Dependencies.Domain;
using Otor.MsixHero.Infrastructure.Processes;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Proxy;

namespace Otor.MsixHero.Cli.Executors.Standard
{
    public class DependenciesVerbExecutor : VerbExecutor<DependenciesVerb>
    {
        public DependenciesVerbExecutor(DependenciesVerb verb, IConsole console) : base(verb, console)
        {
        }

        public override async Task<int> Execute()
        {
            await this.Console.WriteInfo("Reading dependencies...").ConfigureAwait(false);
            var dependencyMapper = new DependencyMapper(new SelfElevationManagerFactory(new Client(new InterProcessCommunicationManager()), new LocalConfigurationService()));

            var graph = await dependencyMapper.GetGraph(this.Verb.Path).ConfigureAwait(false);
            
            foreach (var item in graph.Elements)
            {
                var selectWhereRight = graph.Relations.Where(rel => rel.Right == item).ToArray();
                if (!selectWhereRight.Any())
                {
                    continue;
                }    
                
                await Console.WriteInfo(VertexToString(item)).ConfigureAwait(false);
                foreach (var relation in selectWhereRight)
                {
                    await Console.WriteInfo($" * {VertexToString(relation.Left)} ({relation.RelationDescription})").ConfigureAwait(false);
                }
            }

            return StandardExitCodes.ErrorSuccess;
        }

        private static string VertexToString(GraphElement element)
        {
            if (element is PackageGraphElement package)
            {
                return VertexToString(package);
            }

            if (element is OperatingSystemGraphElement os)
            {
                return VertexToString(os);
            }

            if (element is MissingPackageGraphElement missing)
            {
                return VertexToString(missing);
            }

            return "?";
        }

        private static string VertexToString(PackageGraphElement element)
        {
            return element.Package.DisplayName;
        }

        private static string VertexToString(OperatingSystemGraphElement element)
        {
            return element.MaxRequiredCaption.Replace(Environment.NewLine, " ");
        }

        private static string VertexToString(MissingPackageGraphElement element)
        {
            return element.PackageName;
        }
    }
}