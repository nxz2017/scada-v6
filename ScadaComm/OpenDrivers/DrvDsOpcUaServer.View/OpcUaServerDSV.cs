﻿// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Comm.Config;
using Scada.Comm.DataSources;
using Scada.Comm.Drivers.DrvDsOpcUaServer.View.Forms;
using System.Windows.Forms;

namespace Scada.Comm.Drivers.DrvDsOpcUaServer.View
{
    /// <summary>
    /// Implements the data source user interface.
    /// <para>Реализует пользовательский интерфейс источника данных.</para>
    /// </summary>
    internal class OpcUaServerDSV : DataSourceView
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public OpcUaServerDSV(DriverView parentView, DataSourceConfig dataSourceConfig)
            : base(parentView, dataSourceConfig)
        {
            CanShowProperties = true;
        }

        /// <summary>
        /// Shows a modal dialog box for editing data source properties.
        /// </summary>
        public override bool ShowProperties()
        {
            return new FrmOpcUaServerDSO(BaseDataSet, AppDirs, DataSourceConfig).ShowDialog() == DialogResult.OK;
        }
    }
}
