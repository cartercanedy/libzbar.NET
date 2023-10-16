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
  public static class ZBar
  {
    /// <value>
    /// Get version of the backing libzbar library distribution
    /// </value>
    public static string Version
    {
      get
      {
        uint lMajor = 0;
        uint lMinor = 0;

        int lErrNo;

        if ((lErrNo = zbar_version(ref lMajor, ref lMinor)) != 0)
        {
          unsafe
          {
            errinfo_t lErrInfo = new() { error = (zbar_error_e)lErrNo };
            throw new Exception(Marshal.PtrToStringAnsi(_zbar_error_string(&lErrInfo, 10)));
          }
        }

        return $"{lMajor}.{lMinor}";
      }
    }

    internal enum errmodule_e
    {
      ZBAR_MOD_PROCESSOR,
      ZBAR_MOD_VIDEO,
      ZBAR_MOD_WINDOW,
      ZBAR_MOD_IMAGE_SCANNER,
      ZBAR_MOD_UNKNOWN
    }

    internal enum errsev_e
    {
      SEV_FATAL   =  -2,
      SEV_ERROR   =  -1,
      SEV_OK      =   0,
      SEV_WARNING =   1,
      SEV_NOTE    =   2
    }

    internal enum zbar_error_e
    {
      ZBAR_OK = 0,
      ZBAR_ERR_NOMEM,
      ZBAR_ERR_INTERNAL,
      ZBAR_ERR_UNSUPPORTED,
      ZBAR_ERR_INVALID,
      ZBAR_ERR_SYSTEM,
      ZBAR_ERR_LOCKING,
      ZBAR_ERR_BUSY,
      ZBAR_ERR_XDISPLAY,
      ZBAR_ERR_XPROTO,
      ZBAR_ERR_CLOSED,
      ZBAR_ERR_WINAPI,
      ZBAR_ERR_NUM
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct errinfo_t
    {
      public uint magic;
      public errmodule_e module;
      public IntPtr buf;
      public int errnum;
      public errsev_e severity;
      public zbar_error_e error;
      public IntPtr function;
      public IntPtr detail;
      public IntPtr arg_str;
      public int arg_int;
    }

    #region p/invoke declarations
    [DllImport("libzbar")]
    private static extern int zbar_version(ref uint major, ref uint minor);

    [DllImport("libzbar")]
    private static unsafe extern IntPtr _zbar_error_string(errinfo_t *errorInfo, int verbosity);
    #endregion
  }
}