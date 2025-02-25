﻿// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Config;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Scada.Server.Modules.ModArcPostgreSql
{
    /// <summary>
    /// Represents a module configuration.
    /// <para>Представляет конфигурацию модуля.</para>
    /// </summary>
    public class ModuleConfig : BaseConfig
    {
        /// <summary>
        /// The default configuration file name.
        /// </summary>
        public const string DefaultFileName = "ModArcPostgreSql.xml";


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ModuleConfig()
        {
            SetToDefault();
        }


        /// <summary>
        /// Gets the connection options accessed by connection name.
        /// </summary>
        public SortedList<string, DbConnectionOptions> Connections { get; private set; }


        /// <summary>
        /// Sets the default values.
        /// </summary>
        protected override void SetToDefault()
        {
            Connections = new SortedList<string, DbConnectionOptions>();
        }

        /// <summary>
        /// Loads the configuration from the specified reader.
        /// </summary>
        protected override void Load(TextReader reader)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(reader);

            if (xmlDoc.DocumentElement.SelectSingleNode("Connections") is XmlNode connectionsNode)
            {
                foreach (XmlNode connectionNode in connectionsNode.SelectNodes("Connection"))
                {
                    DbConnectionOptions connectionOptions = new DbConnectionOptions();
                    connectionOptions.LoadFromXml(connectionNode);
                    Connections[connectionOptions.Name] = connectionOptions;
                }
            }
        }

        /// <summary>
        /// Saves the configuration to the specified writer.
        /// </summary>
        protected override void Save(TextWriter writer)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlDeclaration xmlDecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDoc.AppendChild(xmlDecl);

            XmlElement rootElem = xmlDoc.CreateElement("ModArcPostgreSql");
            xmlDoc.AppendChild(rootElem);

            XmlElement connectionsElem = rootElem.AppendElem("Connections");
            foreach (DbConnectionOptions connectionOptions in Connections.Values)
            {
                connectionOptions.SaveToXml(connectionsElem.AppendElem("Connection"));
            }

            xmlDoc.Save(writer);
        }
    }
}
