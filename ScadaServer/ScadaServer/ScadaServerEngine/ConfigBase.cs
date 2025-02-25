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
 * Module   : ScadaServerEngine
 * Summary  : Represents a configuration database
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2021
 * Modified : 2021
 */

using Scada.Data.Models;
using Scada.Data.Tables;
using System;
using System.Collections.Generic;

namespace Scada.Server.Engine
{
    /// <summary>
    /// Represents a configuration database.
    /// <para>Представляет базу конфигурации.</para>
    /// </summary>
    internal class ConfigBase : BaseDataSet
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ConfigBase()
            : base()
        {
            BaseTimestamp = DateTime.UtcNow;
            TableMap = new Dictionary<string, IBaseTable>();
            MapBaseTables();
        }


        /// <summary>
        /// Gets the timestamp of the configuration database.
        /// </summary>
        public DateTime BaseTimestamp { get; }

        /// <summary>
        /// Gets the tables accessed by DAT file name.
        /// </summary>
        public Dictionary<string, IBaseTable> TableMap { get; }


        /// <summary>
        /// Fills the table dictonary.
        /// </summary>
        private void MapBaseTables()
        {
            foreach (IBaseTable baseTable in AllTables)
            {
                TableMap[baseTable.FileNameDat] = baseTable;
            }
        }
    }
}
