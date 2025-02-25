﻿// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Npgsql;
using NpgsqlTypes;
using Scada.Config;
using Scada.Data.Models;
using Scada.Lang;
using Scada.Log;
using Scada.Server.Archives;
using Scada.Server.Config;
using Scada.Server.Lang;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Scada.Server.Modules.ModArcPostgreSql.Logic
{
    /// <summary>
    /// Implements the current data archive logic.
    /// <para>Реализует логику архива текущих данных.</para>
    /// </summary>
    internal class PostgreCAL : CurrentArchiveLogic
    {
        private readonly ModuleConfig moduleConfig;     // the module configuration
        private readonly PostgreCAO archiveOptions;     // the archive options
        private readonly ILog appLog;                   // the application log
        private readonly ILog arcLog;                   // the archive log
        private readonly Stopwatch stopwatch;           // measures the time of operations
        private readonly QueryBuilder queryBuilder;     // builds SQL requests
        private readonly PointQueue pointQueue;         // contains data points for writing

        private bool hasError;            // the archive is in error state
        private NpgsqlConnection conn;    // the database connection
        private Thread thread;            // the thread for writing data
        private volatile bool terminated; // necessary to stop the thread
        private DateTime nextWriteTime;   // the next time to write the current data
        private int[] cnlIndexes;         // the channel mapping indexes


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public PostgreCAL(IArchiveContext archiveContext, ArchiveConfig archiveConfig, int[] cnlNums,
            ModuleConfig moduleConfig) : base(archiveContext, archiveConfig, cnlNums)
        {
            this.moduleConfig = moduleConfig ?? throw new ArgumentNullException(nameof(moduleConfig));
            archiveOptions = new PostgreCAO(archiveConfig.CustomOptions);
            appLog = archiveContext.Log;
            arcLog = archiveOptions.LogEnabled ? CreateLog(ModuleUtils.ModuleCode) : null;
            stopwatch = new Stopwatch();
            queryBuilder = new QueryBuilder(Code);
            pointQueue = new PointQueue(FixQueueSize(), queryBuilder.InsertCurrentDataQuery)
            {
                ArchiveCode = Code,
                AppLog = appLog,
                ArcLog = arcLog
            };

            hasError = false;
            conn = null;
            thread = null;
            terminated = false;
            nextWriteTime = DateTime.MinValue;
            cnlIndexes = null;
        }


        /// <summary>
        /// Gets the current archive status as text.
        /// </summary>
        public override string StatusText
        {
            get
            {
                return DbUtils.GetStatusText(IsReady, hasError || pointQueue.HasError,
                    pointQueue.Count, pointQueue.MaxQueueSize);
            }
        }


        /// <summary>
        /// Checks and corrects the size of the data queue.
        /// </summary>
        private int FixQueueSize()
        {
            int recommendedSize = Math.Max(CnlNums.Length * 2, DbUtils.MinQueueSize);
            return Math.Max(archiveOptions.MaxQueueSize, recommendedSize);
        }

        /// <summary>
        /// Creates database entities if they do not exist.
        /// </summary>
        private void CreateDbEntities()
        {
            try
            {
                conn.Open();

                NpgsqlCommand cmd = new NpgsqlCommand(queryBuilder.CreateSchemaQuery, conn);
                cmd.ExecuteNonQuery();

                cmd = new NpgsqlCommand(queryBuilder.CreateCurrentTableQuery, conn);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Operating cycle running in a separate thread.
        /// </summary>
        private void Execute()
        {
            while (!terminated)
            {
                if (pointQueue.Count > 0)
                    pointQueue.InsertPoints();

                Thread.Sleep(ScadaUtils.ThreadDelay);
            }
        }


        /// <summary>
        /// Makes the archive ready for operating.
        /// </summary>
        public override void MakeReady()
        {
            // prepare database
            DbConnectionOptions connOptions = archiveOptions.UseStorageConn
                ? DbUtils.GetConnectionOptions(ArchiveContext.InstanceConfig)
                : DbUtils.GetConnectionOptions(moduleConfig, archiveOptions.Connection);

            conn = DbUtils.CreateDbConnection(connOptions);
            pointQueue.Connection = conn; // the same connection for the queue
            CreateDbEntities();

            // start thread for writing data
            terminated = false;
            thread = new Thread(Execute);
            thread.Start();
        }

        /// <summary>
        /// Closes the archive.
        /// </summary>
        public override void Close()
        {
            if (thread != null)
            {
                terminated = true;
                thread.Join();
                thread = null;
            }

            if (conn != null)
            {
                pointQueue.FlushPoints();
                conn.Dispose();
                conn = null;
            }
        }

        /// <summary>
        /// Reads the current data.
        /// </summary>
        public override void ReadData(ICurrentData curData, out bool completed)
        {
            NpgsqlTransaction trans = null;

            try
            {
                stopwatch.Restart();
                conn.Open();
                trans = conn.BeginTransaction();

                string sql = "SELECT cnl_num, time_stamp, val, stat FROM " + queryBuilder.CurrentTable;
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn, trans);
                List<int> cnlsToDelete = new List<int>();
                int pointCnt = 0;

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int cnlNum = reader.GetInt32(0);
                        int cnlIndex = curData.GetCnlIndex(cnlNum);

                        if (cnlIndex >= 0)
                        {
                            curData.Timestamps[cnlIndex] = reader.GetDateTimeUtc(1);
                            curData.CnlData[cnlIndex] = new CnlData
                            {
                                Val = reader.GetDouble(2),
                                Stat = reader.GetInt32(3)
                            };
                            pointCnt++;
                        }
                        else
                        {
                            cnlsToDelete.Add(cnlNum);
                        }
                    }
                }

                // delete data of unused channels
                if (cnlsToDelete.Count > 0)
                {
                    sql = $"DELETE FROM {queryBuilder.CurrentTable} WHERE cnl_num = @cnlNum";
                    cmd = new NpgsqlCommand(sql, conn, trans);
                    NpgsqlParameter cnlNumParam = cmd.Parameters.Add("cnlNum", NpgsqlDbType.Integer);

                    foreach (int cnlNum in cnlsToDelete)
                    {
                        cnlNumParam.Value = cnlNum;
                        cmd.ExecuteNonQuery();
                    }
                }

                trans.Commit();
                completed = true;
                hasError = false;
                stopwatch.Stop();
                arcLog?.WriteAction(ServerPhrases.ReadingPointsCompleted, pointCnt, stopwatch.ElapsedMilliseconds);
            }
            catch
            {
                trans?.Rollback();
                completed = false;
                hasError = true;
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Writes the current data.
        /// </summary>
        public override void WriteData(ICurrentData curData)
        {
            stopwatch.Restart();
            InitCnlIndexes(curData, ref cnlIndexes);

            lock (pointQueue.SyncRoot)
            {
                for (int i = 0, cnlCnt = CnlNums.Length; i < cnlCnt; i++)
                {
                    int cnlIndex = cnlIndexes[i];
                    pointQueue.EnqueueWithoutLock(CnlNums[i], curData.Timestamps[cnlIndex], curData.CnlData[cnlIndex]);
                }
            }

            pointQueue.RemoveExcessPoints();
            stopwatch.Stop();
            arcLog?.WriteAction(ServerPhrases.QueueingPointsCompleted, CnlNums.Length, stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Processes new data.
        /// </summary>
        public override bool ProcessData(ICurrentData curData)
        {
            if (nextWriteTime <= curData.Timestamp)
            {
                nextWriteTime = GetNextWriteTime(curData.Timestamp, archiveOptions.WritingPeriod);
                WriteData(curData);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
