﻿// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Admin.Extensions.ExtCommConfig.Code;
using Scada.Admin.Project;
using Scada.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Scada.Admin.Extensions.ExtCommConfig.Controls
{
    /// <summary>
    /// Represents a control for selecting an object when creating channels.
    /// <para>Представляет элемент управления для выбора объекта при создании каналов.</para>
    /// </summary>
    public partial class CtrlCnlCreate2 : UserControl
    {
        private ScadaProject project;            // the project under development
        private RecentSelection recentSelection; // the recently selected objects


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public CtrlCnlCreate2()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Gets or sets the selected device name.
        /// </summary>
        public string DeviceName
        {
            get
            {
                return txtDevice.Text;
            }
            set
            {
                txtDevice.Text = value ?? "";
            }
        }
        
        /// <summary>
        /// Gets the selected object number.
        /// </summary>
        public int? ObjNum
        {
            get
            {
                int objNum = (int)cbObj.SelectedValue;
                return objNum > 0 ? objNum : null;
            }
        }


        /// <summary>
        /// Fills the combo box with the device types.
        /// </summary>
        private void FillObjectList()
        {
            List<Obj> objs = new(project.ConfigBase.ObjTable.ItemCount + 1);
            objs.Add(new Obj { ObjNum = 0, Name = " " });
            objs.AddRange(project.ConfigBase.ObjTable.Enumerate().OrderBy(obj => obj.Name));
            cbObj.DataSource = objs;

            try { cbObj.SelectedValue = recentSelection.ObjNum; }
            catch { cbObj.SelectedValue = 0; }
        }

        /// <summary>
        /// Initializes the control.
        /// </summary>
        public void Init(ScadaProject project, RecentSelection recentSelection)
        {
            this.project = project ?? throw new ArgumentNullException(nameof(project));
            this.recentSelection = recentSelection ?? throw new ArgumentNullException(nameof(recentSelection));
            FillObjectList();
        }

        /// <summary>
        /// Sets the input focus.
        /// </summary>
        public void SetFocus()
        {
            cbObj.Select();
        }
    }
}
