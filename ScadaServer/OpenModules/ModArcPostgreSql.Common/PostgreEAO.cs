﻿// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Config;
using Scada.Server.Archives;

namespace Scada.Server.Modules.ModArcPostgreSql
{
    /// <summary>
    /// Represents options of an event archive.
    /// <para>Представляет параметры архива событий.</para>
    /// </summary>
    public class PostgreEAO : EventArchiveOptions
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public PostgreEAO(OptionList options)
            : base(options)
        {
            UseStorageConn = options.GetValueAsBool("UseStorageConn", true);
            Connection = options.GetValueAsString("Connection");
            MaxQueueSize = options.GetValueAsInt("MaxQueueSize", ModuleUtils.DefaultQueueSize);
            PartitionSize = options.GetValueAsEnum("PartitionSize", PartitionSize.OneMonth);
        }


        /// <summary>
        /// Gets or sets a value indicating whether to use a connection specified in the storage configuration.
        /// </summary>
        public bool UseStorageConn { get; set; }

        /// <summary>
        /// Gets or sets the connection name.
        /// </summary>
        public string Connection { get; set; }

        /// <summary>
        /// Gets or sets the maximum queue size.
        /// </summary>
        public int MaxQueueSize { get; set; }

        /// <summary>
        /// Gets or sets the partition size.
        /// </summary>
        public PartitionSize PartitionSize { get; set; }


        /// <summary>
        /// Adds the options to the list.
        /// </summary>
        public override void AddToOptionList(OptionList options)
        {
            base.AddToOptionList(options);
            options["UseStorageConn"] = UseStorageConn.ToLowerString();
            options["Connection"] = Connection;
            options["MaxQueueSize"] = MaxQueueSize.ToString();
            options["PartitionSize"] = PartitionSize.ToString();
        }
    }
}
