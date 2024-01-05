/*------------------------------------------------------------------------
 *  Copyright 2023 (c) Carter Canedy <cartercanedy42@gmail.com>
 *  Copyright 2009 (c) Jonas Finnemann Jensen <jopsen@gmail.com>
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
  /// <summary>
  /// Representation of a decoded symbol. <br/><br/>
  /// This symbol does not hold any references to unmanaged resources.
  /// </summary>
  public class Symbol
  {
    /// <summary>
    /// Initialize a symbol from pointer to a symbol
    /// </summary>
    /// <param name="symbol">
    /// Pointer to a symbol_t.
    /// </param>
    internal Symbol(IntPtr symbol)
    {
      if (symbol == IntPtr.Zero)
        throw new NullReferenceException("Can't initialize symbol from null pointer.");

      IntPtr pData = zbar_symbol_get_data(symbol);
      int length = (int)zbar_symbol_get_data_length(symbol);
      Data = Marshal.PtrToStringAnsi(pData, length);
      Type = (SymbolType)zbar_symbol_get_type(symbol);
      Quality = zbar_symbol_get_quality(symbol);
      Count = zbar_symbol_get_count(symbol);
    }

    public override string ToString() => $"{ Type } { Data }";

    #region Public properties

    /// <summary>
    /// Retrieve current cache count.<br/>
    /// When the cache is enabled for the image_scanner, this provides inter-frame reliability and redundancy information for video streams.
    /// </summary>
    /// <value>
    /// &lt; 0 if symbol is still uncertain<br/>
    ///   == 0 if symbol is newly verified<br/>
    /// &gt; 0 for duplicate symbols
    /// </value>
    public int Count { get; private set; }

    /// <summary>
    /// Data decoded from symbol.
    /// </summary>
    public string Data { get; private set; }

    /// <summary>
    /// Get a symbol confidence metric. 
    /// </summary>
    /// <value>
    /// An unscaled, relative quantity: larger values are better than smaller values, where "large" and "small" are application dependent.
    /// </value>
    public int Quality { get; private set; }

    /// <summary>
    /// Type of decoded symbol
    /// </summary>
    public SymbolType Type { get; private set; }
    #endregion
  }
}
