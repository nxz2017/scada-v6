﻿// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Config;

namespace Scada.Server.Modules.ModArcPostgreSql
{
    /// <summary>
    /// The class provides helper methods for the module.
    /// <para>Класс, предоставляющий вспомогательные методы для модуля.</para>
    /// </summary>
    public static class ModuleUtils
    {
        /// <summary>
        /// The module code.
        /// </summary>
        public const string ModuleCode = "ModArcPostgreSql";

        /// <summary>
        /// The default queue size.
        /// </summary>
        public const int DefaultQueueSize = 1000;
    }
}
