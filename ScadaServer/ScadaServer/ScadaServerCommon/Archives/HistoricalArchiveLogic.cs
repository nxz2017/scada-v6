﻿/*
 * Copyright 2020 Mikhail Shiryaev
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
 * Module   : ScadaServerCommon
 * Summary  : Represents the base class for historical data archive logic
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2020
 * Modified : 2020
 */

using Scada.Data.Models;
using Scada.Server.Config;
using System;
using System.Collections.Generic;

namespace Scada.Server.Archives
{
    /// <summary>
    /// Represents the base class for historical data archive logic.
    /// <para>Представляет базовый класс логики архива исторических данных.</para>
    /// </summary>
    public abstract class HistoricalArchiveLogic : ArchiveLogic
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public HistoricalArchiveLogic(ArchiveConfig archiveConfig, int[] cnlNums)
            : base(archiveConfig, cnlNums)
        {
        }


        /// <summary>
        /// Gets the input channel data.
        /// </summary>
        public abstract CnlData GetCnlData(int cnlNum, DateTime timestamp);

        /// <summary>
        /// Gets the trends of the specified input channels.
        /// </summary>
        public abstract TrendBundle GetTrends(int[] cnlNums, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets the trend of the specified input channel.
        /// </summary>
        public abstract Trend GetTrend(int cnlNum, DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets the available timestamps.
        /// </summary>
        public abstract List<DateTime> GetTimestamps(DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets the slice of the specified input channels at the timestamp.
        /// </summary>
        public abstract Slice GetSlice(int[] cnlNums, DateTime timestamp);

        /// <summary>
        /// Processes new data.
        /// </summary>
        public abstract void ProcessData(ICurrentData curData);

        /// <summary>
        /// Accepts or rejects data with the specified timestamp.
        /// </summary>
        public abstract bool AcceptData(DateTime timestamp);

        /// <summary>
        /// Maintains performance when data is written one at a time.
        /// </summary>
        public abstract void BeginUpdate(int deviceNum);

        /// <summary>
        /// Completes the update operation.
        /// </summary>
        public abstract void EndUpdate(int deviceNum);

        /// <summary>
        /// Writes the input channel data.
        /// </summary>
        public abstract void WriteCnlData(int cnlNum, DateTime timestamp, CnlData cnlData);
    }
}
