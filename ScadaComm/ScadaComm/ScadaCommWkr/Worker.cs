﻿/*
 * Copyright 2022 Rapid Software LLC
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : Communicator Worker
 * Summary  : Implements the Communicator service
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2021
 * Modified : 2021
 */

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scada.Comm.Engine;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace Scada.Comm.Wkr
{
    /// <summary>
    /// Implements the Communicator service.
    /// <para>Реализует службу Коммуникатора.</para>
    /// </summary>
    public class Worker : BackgroundService
    {
        private const int TaskDelay = 1000;
        private readonly ILogger<Worker> logger;
        private readonly Manager manager;

        public Worker(ILogger<Worker> logger)
        {
            this.logger = logger;
            manager = new Manager();

            AssemblyLoadContext.Default.Resolving += (AssemblyLoadContext context, AssemblyName name) =>
            {
                return manager.AppDirs.FindAssembly(name.Name, out string path) ?
                    context.LoadFromAssemblyPath(path) : null;
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() =>
            {
                manager.StopService();
                logger.LogInformation("Communicator is stopped");
            });

            if (manager.StartService())
                logger.LogInformation("Communicator is started successfully");
            else
                logger.LogError("Communicator is started with errors");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TaskDelay, stoppingToken);
            }
        }
    }
}
