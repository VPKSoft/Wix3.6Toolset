//-------------------------------------------------------------------------------------------------
// <copyright file="MsiException.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
//    
//    The use and distribution terms for this software are covered by the
//    Common Public License 1.0 (http://opensource.org/licenses/cpl1.0.php)
//    which can be found in the file CPL.TXT at the root of this distribution.
//    By using this software in any fashion, you are agreeing to be bound by
//    the terms of this license.
//    
//    You must not remove this notice, or any other, from this software.
// </copyright>
// 
// <summary>
// Exception that wraps MsiGetLastError().
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Msi
{
    using System;
    using System.ComponentModel;
    using Microsoft.Tools.WindowsInstallerXml.Msi.Interop;

    /// <summary>
    /// Exception that wraps MsiGetLastError().
    /// </summary>
    [Serializable]
    public class MsiException : Win32Exception
    {
        /// <summary>
        /// Instantiate a new MsiException with a given error.
        /// </summary>
        /// <param name="error">The error code from the MsiXxx() function call.</param>
        public MsiException(int error) : base(error)
        {
            uint handle = MsiInterop.MsiGetLastErrorRecord();
            if (0 != handle)
            {
                using (Record record = new Record(handle))
                {
                    this.MsiError = record.GetInteger(1);

                    int errorInfoCount = record.GetFieldCount() - 1;
                    this.ErrorInfo = new string[errorInfoCount];
                    for (int i = 0; i < errorInfoCount; ++i)
                    {
                        this.ErrorInfo[i] = record.GetString(i + 2);
                    }
                }
            }
            else
            {
                this.MsiError = 0;
                this.ErrorInfo = new string[0];
            }

            this.Error = error;
        }

        /// <summary>
        /// Gets the error number.
        /// </summary>
        public int Error { get; private set; }

        /// <summary>
        /// Gets the internal MSI error number.
        /// </summary>
        public int MsiError { get; private set; }

        /// <summary>
        /// Gets any additional the error information.
        /// </summary>
        public string[] ErrorInfo { get; private set; }

        /// <summary>
        /// Overrides Message property to return useful error message.
        /// </summary>
        public override string Message
        {
            get
            {
                if (0 == this.MsiError)
                {
                    return base.Message;
                }
                else
                {
                    return String.Format("Internal MSI failure. Win32 error: {0}, MSI error: {1}, detail: {2}", this.Error, this.MsiError, String.Join(", ", this.ErrorInfo));
                }
            }
        }
    }
}
