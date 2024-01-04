/*------------------------------------------------------------------------
 *  Copyright 2023 (c) Carter Canedy <cartercanedy42@gmail.com>
 *  Copyright 2009 (c) Jonas Finnemann Jensen <jopsen@gmail.com>
 *  Copyright 2007-2009 (c) Jeff Brown <spadix@users.sourceforge.net>
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
using ZBar.Interop;
using static ZBar.Interop.NativeFunctions;

namespace ZBar
{
  /// <summary>
  /// Representation of a decoded symbol
  /// </summary>
  /// <remarks>This symbol does not hold any references to unmanaged resources.</remarks>
  public class Symbol
  {
    /// <summary>
    /// Initialize a symbol from pointer to a symbol
    /// </summary>
    /// <param name="symbol">
    /// Pointer to a symbol
    /// </param>
    internal Symbol(IntPtr symbol)
    {
      if (symbol == IntPtr.Zero)
        throw new Exception("Can't initialize symbol from null pointer.");

      IntPtr pData = zbar_symbol_get_data(symbol);
      int length = (int)zbar_symbol_get_data_length(symbol);
      data = Marshal.PtrToStringAnsi(pData, length);
      type = (SymbolType)zbar_symbol_get_type(symbol);
      quality = zbar_symbol_get_quality(symbol);
      count = zbar_symbol_get_count(symbol);
    }

    private string data;
    private int quality;
    private int count;
    private SymbolType type;

    public override string ToString()
    {
      return type.ToString() + " " + data;
    }

    #region Public properties

    /// <value>
    /// Retrieve current cache count.
    /// </value>
    /// <remarks>
    /// When the cache is enabled for the image_scanner this provides inter-frame reliability and redundancy information for video streams. 
    /// 	&lt; 0 if symbol is still uncertain.
    /// 	0 if symbol is newly verified.
    /// 	&gt; 0 for duplicate symbols 
    /// </remarks>
    public int Count { get => count; }

    /// <value>
    /// Data decoded from symbol.
    /// </value>
    public string Data { get => data; }

    /// <value>
    /// Get a symbol confidence metric. 
    /// </value>
    /// <remarks>
    /// An unscaled, relative quantity: larger values are better than smaller values, where "large" and "small" are application dependent.
    /// </remarks>
    public int Quality { get => quality; }

    /// <value>
    /// Type of decoded symbol
    /// </value>
    public SymbolType Type { get => type; }

    #endregion
    #region Extern C functions
    #endregion
  }

  /// <summary>
  /// Different symbol types
  /// </summary>
  [Flags]
  public enum SymbolType
  {
    /// <summary>
    /// No symbol decoded
    /// </summary>
    None = 0,

    /// <summary>
    /// Intermediate status
    /// </summary>
    Partial = 1,

    /// <summary>
    /// EAN-8
    /// </summary>
    EAN8 = 8,

    /// <summary>
    /// UPC-E
    /// </summary>
    UPCE = 9,

    /// <summary>
    /// ISBN-10 (from EAN-13)
    /// </summary>
    ISBN10 = 10,

    /// <summary>
    /// UPC-A
    /// </summary>
    UPCA = 12,

    /// <summary>
    /// EAN-13
    /// </summary>
    EAN13 = 13,

    /// <summary>
    /// ISBN-13 (from EAN-13)
    /// </summary>
    ISBN13 = 14,

    /// <summary>
    /// Interleaved 2 of 5.
    /// </summary>
    I25 = 25,

    /// <summary>
    /// Code 39.
    /// </summary>
    CODE39 = 39,

    /// <summary>
    /// PDF417
    /// </summary>
    PDF417 = 57,

    /// <summary>
    /// QR Code
    /// </summary>
    QRCODE = 64,

    /// <summary>
    /// Code 128
    /// </summary>
    CODE128 = 128,

    /// <summary>
    /// mask for base symbol type
    /// </summary>
    Symbole = 0x00ff,

    /// <summary>
    /// 2-digit add-on flag
    /// </summary>
    Addon2 = 0x0200,

    /// <summary>
    /// 5-digit add-on flag
    /// </summary>
    Addon5 = 0x0500,

    /// <summary>
    /// add-on flag mask
    /// </summary>
    Addon = 0x0700
  }
}
