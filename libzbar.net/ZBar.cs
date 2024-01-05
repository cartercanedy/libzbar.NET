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
using ZBar.Native;
using static ZBar.Native.NativeFunctions;

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
  }
}