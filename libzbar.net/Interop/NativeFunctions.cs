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

using System.Runtime.InteropServices;
using System;

namespace ZBar.Native
{
  internal static class NativeFunctions
  {
    [DllImport("libzbar")]
    public static extern int zbar_version(ref uint major, ref uint minor);

    [DllImport("libzbar")]
    public static unsafe extern IntPtr _zbar_error_string(errinfo_t* errorInfo, int verbosity);

    /// <summary>
    /// Constructs a zbar image.
    /// </summary>
    /// <returns>
    /// An unitialized and unmanaged reference to a zbar image.<br/>
    /// </returns>
    /// <remarks>
    /// WARNING: UNMANAGED RESOURCE CREATION<br/>
    /// Use <see cref="zbar_image_destroy(IntPtr)"/> to ensure resources are properly released.
    /// </remarks>
    [DllImport("libzbar")]
    public static extern IntPtr zbar_image_create();

    /// <summary>
    /// Marks the referenced image instance for release and invalidates all remaining references to the specified image and it's resources.<br/>
    /// All images should be destroyed using this function.<br/>
    /// When an image is destroyed, the associated data cleanup handler will be invoked, if available.
    /// </summary>
    /// <remarks>
    /// Make no assumptions about the image or the data buffer.<br/>
    /// They may not be destroyed/cleaned immediately if the library is still using them.<br/>
    /// If necessary, use the cleanup handler hook to keep track of image data buffers
    /// </remarks>
    [DllImport("libzbar")]
    public static extern void zbar_image_destroy(IntPtr image);

    /// <summary>
    /// Used for direct reference count manipulation.
    /// Increment the reference count when you store a new reference to the
    /// image. Decrement when the reference is no longer used. Do not
    /// refer to the image any longer once the count is decremented.
    /// </summary>
    /// <remarks>
    /// Calling this function with <paramref name="refs"/> set to -1 has the same effect as calling <see cref="zbar_image_destroy(IntPtr)"/>.
    /// </remarks>
    [DllImport("libzbar")]
    public static extern void zbar_image_ref(IntPtr image, int refs);

    /// <summary>
    /// Converts image data format into specified format. Refer to documentation for supported image formats
    /// </summary>
    /// <returns>
    /// A new image with data from the provided image converted into the requested format. 
    /// </returns>
    /// <remarks>
    /// The converted image size may be rounded (up) due to format constraints.<br/>
    /// The original image data is unaffected.
    /// </remarks>
    [DllImport("libzbar")]
    public static extern IntPtr zbar_image_convert(IntPtr image, uint format);

    /// <summary>
    /// Image format conversion with crop/pad.<br/><br/>
    /// If the requested size is larger than the image, the last row/column are duplicated to cover the difference.<br/>
    /// If the requested size is smaller than the image, the extra rows/columns are dropped from the right/bottom.<br/>
    /// </summary>
    /// <returns>
    /// A new image with the sample data from the original image converted to the requested format and size.
    /// </returns>
    /// <remarks>
    /// The image is not scaled
    /// </remarks>
    [DllImport("libzbar")]
    public static extern IntPtr zbar_image_convert_resize(IntPtr image, uint format, uint width, uint height);

    /// <summary>
    /// Retrieves the format of the image data.
    /// </summary>
    /// <returns>
    /// The FourCC describing the format of the image sample data.
    /// </returns>
    [DllImport("libzbar")]
    public static extern uint zbar_image_get_format(IntPtr image);

    /// <summary>
    /// Retrieve a "sequence" (page/frame) number associated with this image.
    /// </summary>
    [DllImport("libzbar")]
    public static extern uint zbar_image_get_sequence(IntPtr image);

    /// <summary>
    /// Retrieve the width of the image.
    /// </summary>
    /// <returns>
    /// The width in sample columns.
    /// </returns>
    [DllImport("libzbar")]
    public static extern uint zbar_image_get_width(IntPtr image);

    /// <summary>
    /// Retrieve the height of the image.
    /// </summary>
    /// <returns>
    /// The height in sample rows
    /// </returns>
    [DllImport("libzbar")]
    public static extern uint zbar_image_get_height(IntPtr image);

    /// <summary>
    /// Gets a pointer the image data buffer.<br/>
    /// </summary>
    /// <remarks>
    /// The lifetime of the buffer is tied to the image it came from. See <see cref="zbar_image_destroy(IntPtr)"/>.
    /// </remarks>
    [DllImport("libzbar")]
    public static extern IntPtr zbar_image_get_data(IntPtr image);

    /// <summary>
    /// Gets the size of the image data buffer held by a zbar image_t.
    /// </summary>
    /// <returns>
    /// The size of the data buffer in bytes
    /// </returns>
    [DllImport("libzbar")]
    public static extern uint zbar_image_get_data_length(IntPtr img);

    /// <summary>
    /// Image_scanner decode result iterator.
    /// </summary>
    /// <returns>
    /// The first decoded symbol result for an image, or <see cref="IntPtr.Zero"/> if no results are available.
    /// </returns>
    [DllImport("libzbar")]
    public static extern IntPtr zbar_image_first_symbol(IntPtr image);

    /// <summary>
    /// Specify the fourcc image format code for image sample data.<br/>
    /// Refer to documentation for supported formats.
    /// </summary>
    /// <remarks>
    /// This does not convert image data (see <see cref="zbar_image_convert(IntPtr, uint)"/> for conversion)
    /// </remarks>
    [DllImport("libzbar")]
    public static extern void zbar_image_set_format(IntPtr image, uint format);

    /// <summary>
    /// Associate a sequence number with this image.
    /// </summary>
    [DllImport("libzbar")]
    public static extern void zbar_image_set_sequence(IntPtr image, uint sequence_num);

    /// <summary>
    /// Specify the pixel size of the image.
    /// </summary>
    /// <remarks>
    /// This does not affect image data.
    /// </remarks>
    [DllImport("libzbar")]
    public static extern void zbar_image_set_size(IntPtr image, uint width, uint height);

    public delegate void CleanupCallback(IntPtr image);

    /// <summary>
    /// Sets the image data associated with an instance of an image. When image data is no longer needed by the library the specific data cleanup handler will be called.
    /// </summary>
    /// <remarks>
    /// Provided image data will not be modified unless
    /// <see cref="zbar_image_convert(IntPtr, uint)"/> or
    /// <see cref="zbar_image_convert_resize(IntPtr, uint, uint, uint)"/> is called.
    /// </remarks>
    [DllImport("libzbar")]
    public static extern void zbar_image_set_data(IntPtr image, IntPtr data, uint data_byte_length, CleanupCallback cleanupCallback);

    /// <summary>
    /// Built-in cleanup handler.
    /// </summary>
    [DllImport("libzbar")]
    public static extern void zbar_image_free_data(IntPtr image);

    /// <summary>
    /// Associates arbitrary user-specified data value with an instance of an image.
    /// </summary>
    [DllImport("libzbar")]
    public static extern void zbar_image_set_userdata(IntPtr image, IntPtr userdata);

    /// <summary>
    /// Retrieves any user-specified data value associated with the image instance.
    /// </summary>
    [DllImport("libzbar")]
    public static extern IntPtr zbar_image_get_userdata(IntPtr image);
    
    /// <summary>
    /// Retrieve the type/specification of decoded symbol.
    /// </summary>
    /// <returns>
    /// An enumeration represented by <see cref="SymbolType"/>.
    /// </returns>
    [DllImport("libzbar")]
    public static extern int zbar_symbol_get_type(IntPtr symbol);

    /// <summary>
    /// Retrieve data decoded from symbol.
    /// </summary>
    /// <returns>
    /// The decoded data.
    /// </returns>
    [DllImport("libzbar")]
    public static extern IntPtr zbar_symbol_get_data(IntPtr symbol);

    /// <summary>
    /// Retrieve length of binary data.
    /// </summary>
    /// <returns>
    /// The length of the decoded data
    /// </returns>
    [DllImport("libzbar")]
    public static extern uint zbar_symbol_get_data_length(IntPtr symbol);

    /// <summary>
    /// Retrieve a symbol confidence metric.
    /// </summary>
    /// <returns>
    /// An unscaled, relative metric.
    /// </returns>
    /// <remarks>
    /// The ordered relationship between two values is defined and will remain stable.
    /// </remarks>
    [DllImport("libzbar")]
    public static extern int zbar_symbol_get_quality(IntPtr symbol);

    /// <summary>
    /// Retrieve current cache count.
    /// </summary>
    /// <remarks>
    /// When caching is enabled for image_scanner, this provides inter-frame reliability and redundancy information for video streams.
    /// </remarks>
    /// <returns>
    /// &lt; 0 if symbol is still uncertain<br/>
    /// 0 if symbol is newly verified<br/>
    /// &gt; 0 for duplicate symbols<br/>
    /// </returns>
    [DllImport("libzbar")]
    public static extern int zbar_symbol_get_count(IntPtr symbol);

    /// <summary>
    /// Retrieve the number of points in the location polygon. The location polygon defines the image area that the symbol was extracted from.
    /// </summary>
    /// <returns>
    /// The number of points in the location polygon.
    /// </returns>
    /// <remarks>
    /// This is currently not a polygon, but the scan locations where the symbol was decoded.
    /// </remarks>
    [DllImport("libzbar")]
    public static extern uint zbar_symbol_get_loc_size(IntPtr symbol);

    /// <summary>
    /// Retrieve location polygon x-coordinates. Points are specified by 0-based index.
    /// </summary>
    /// <returns>
    /// The x-coordinate for a point in the location polygon, or -1 if index is out of range.
    /// </returns>
    [DllImport("libzbar")]
    public static extern int zbar_symbol_get_loc_x(IntPtr symbol, uint index);

    /// <summary>
    /// Retrieve location polygon y-coordinates. Points are specified by 0-based index.
    /// </summary>
    /// <returns> 
    /// The y-coordinate for a point in the location polygon, or -1 if index is out of range.
    /// </returns>
    [DllImport("libzbar")]
    public static extern int zbar_symbol_get_loc_y(IntPtr symbol, uint index);

    /// <summary>
    /// Return the next symbol in the iterator sequence.
    /// </summary>
    /// <returns>
    /// The next result symbol, or <see cref="IntPtr.Zero"/> when no more results are available.
    /// </returns>
    [DllImport("libzbar")]
    public static extern IntPtr zbar_symbol_next(IntPtr symbol);

    /// <summary>
    /// Gets an XML representation to copied into a user-provided string buffer.<br/><br/>
    /// Requires a reference to an already allocated span of memory, <paramref name="buffer"/>, with specified length <paramref name="buflen"/>.<br/>
    /// If the buffer size is insufficient, the buffer may be realloc'ed as needed.
    /// </summary>
    /// <remarks>
    /// See <see href="http://zbar.sourceforge.net/2008/barcode.xsd"/> for schema.
    /// </remarks>
    /// <param name="symbol"> Symbol handle.</param>
    /// <param name="buffer"> InOut handle to a span of memory.</param>
    /// <param name="buflen"> InOut length of the provided buffer.</param>
    /// <returns>
    /// The handle to the buffer pointed at by <paramref name="buffer"/> with length <paramref name="buflen"/>.
    /// </returns>
    [DllImport("libzbar")]
    public static extern IntPtr zbar_symbol_xml(IntPtr symbol, ref IntPtr buffer, ref uint buflen);
  }
}
