/*-----------------------------------------------------------------------
Licensed to the Apache Software Foundation (ASF) under one
or more contributor license agreements.  See the NOTICE file
distributed with this work for additional information
regarding copyright ownership.  The ASF licenses this file
to you under the Apache License, Version 2.0 (the
"License"; you may not use this file except in compliance
with the License.  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing,
software distributed under the License is distributed on an
"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, either express or implied.  See the License for the
specific language governing permissions and limitations
under the License.
-----------------------------------------------------------------------*/
using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace fesapiGenerator
{
    /// <summary>
    /// A simple dialog form carrying a progress bar.
    /// </summary>
    public partial class ProgressDialog : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        /// <summary>
        /// Constructor
        /// </summary>
        public ProgressDialog()
        {
            InitializeComponent();
            this.TopMost = true;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

            // tells that the progress bar should continuously progress till the end of the
            // background process
            progressBar1.Style = ProgressBarStyle.Marquee;


            // sets the EA window as owner of the progress bar dialog such that
            // it is not displayed when switching form EA to another application 
            IntPtr mainWindowHandle = Tool.getMainWindowHandle();
            if (mainWindowHandle != IntPtr.Zero)
                SetParent(this.Handle, mainWindowHandle);

        }

        /// <summary>
        /// This method prevents the user from using the EA GUI when the 
        /// progress bar dialog is displayed
        /// </summary>
        /// <param name="e">not used</param>
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            this.Focus();
        }
    }
}
