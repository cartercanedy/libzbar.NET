/*------------------------------------------------------------------------
 *  Copyright 2023 (c) Carter Canedy <cartercanedy42@gmail.com>
 * 
 *  This file is part of the libzbar.net .NET Standard 2.1 library.
 *  libzbar.net is not affiliated with the development of libzbar.
 *  This project was derived directly from the zbar-sharp project
 *  (https://github.com/jonasfj/zbar-sharp), first implemented by Jonas
 *  Finneman Jenson <jopsen@gmail.com> et. al. initially released in
 *  2009.
 *
 *  libzbar.net is free software; you can redistribute it
 *  and/or modify it under the terms of the GNU Lesser Public License as
 *  published by the Free Software Foundation; either version 2.1 of
 *  the License, or (at your option) any later version.
 *
 *  libzbar.net is distributed in the hope that it will be
 *  useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 *  of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser Public License
 *  along with any distribution of libzbar.net; if not, write to the
 *  Free Software Foundation, Inc., 51 Franklin St, Fifth Floor,
 *  Boston, MA  02110-1301  USA
 *  
 *------------------------------------------------------------------------*/

using System;
using System.Runtime.InteropServices;

namespace ZBar
{
  /// <summary>
  /// An exception type to wrap runtime ZBar errors
  /// </summary>
  public class ZBarException : Exception
  {
    protected readonly string _message = string.Empty;
    protected readonly ZBarError _errorCode;

    public ZBarException(IntPtr obj) : this(obj, 10) { }

    public ZBarException(IntPtr obj, int verbosity) : this((ZBarError)_zbar_get_error_code(obj))
    {
      _message = Marshal.PtrToStringAnsi(_zbar_error_string(obj, verbosity));
    }

    internal ZBarException(ZBarError errorCode)
    {
      _errorCode = errorCode;
    }

    /// <value>
    /// Error string produced by ZBar
    /// </value>
    public override string Message => _message;

    /// <value>
    /// The ZBar error code associated with this exception.
    /// </value>
    public ZBarError ErrorCode => _errorCode;

    [DllImport("libzbar")]
    private static extern IntPtr _zbar_error_string(IntPtr obj, int verbosity);

    [DllImport("libzbar")]
    private static extern int _zbar_get_error_code(IntPtr obj);
  }

  /// <summary>
  /// Error codes that can be produced by a ZBar runtime error.
  /// </summary>
  /// <remarks>
  /// Self-descriptive, matches the error enumerations in ZBar.
  /// </remarks>
  public enum ZBarError
  {
    Ok = 0, // errno should never be 0, so there might need to be an assertion in the constructor to catch this
    OutOfMemory,
    InternalLibraryError,
    Unsupported,
    InvalidRequest,
    SystemError,
    LockingError,
    AllResourcesBusyError,
    X11DisplayError,
    X11ProtocolError,
    OutputWindowClosed,
    WindowsAPIError
  }
}
