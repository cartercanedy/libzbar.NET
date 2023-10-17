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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ZBar
{
  /// <summary>
  /// Representation of an image in ZBar
  /// </summary>
  public class Image : IDisposable
  {
    private IntPtr _handle = IntPtr.Zero;
    private uint _width;
    private uint _height;

    /// <summary>
    /// Create a new image from an unmanaged object reference
    /// </summary>
    /// <remarks>This resource will be managed by this Image instance.</remarks>
    /// <param name="ptr">
    /// A <see cref="IntPtr"/> to unmananged ZBar image.
    /// </param>
    /// <param name="incrementRefCount">
    /// Whether or not to increment the reference counter.
    /// </param>
    internal Image(IntPtr ptr, bool incrementRefCount)
    {
      if (ptr == IntPtr.Zero)
      {
        throw new Exception($"{nameof(Image)} error: Passed a nullptr into ctor.");
      }

      //If we must increment the reference counter here
      if (incrementRefCount)
      {
        zbar_image_ref(ptr, 1);
      }

      _handle = ptr;
    }

    /// <summary>
    /// Allocate a new uninitialized image, requiring image parameters to be manually set.
    /// </summary>
    /// <remarks>
    /// Be aware that this image is NOT initialized, allocated.
    /// And you must set width, height, format, data etc...
    /// </remarks>
    public Image()
    {
      _handle = zbar_image_create();

      if (_handle == IntPtr.Zero)
      {
        throw new Exception("Failed to create new image!");
      }
    }

    /// <summary>
    /// Create image from an instance of System.Drawing.Image
    /// </summary>
    /// <param name="image">
    /// Image to convert to ZBar.Image
    /// </param>
    /// <remarks>
    /// The converted image is in RGB3 format, so it should be converted using Image.Convert()
    /// before it is scanned, as ZBar only reads images in GREY/Y800
    /// </remarks>
    public Image(System.Drawing.Image image) : this()
    {
      Byte[] data = new byte[image.Width * image.Height * 3];

      //Convert the image to RBG3
      using (Bitmap bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb))
      {
        using (Graphics g = Graphics.FromImage(bitmap))
        {
          g.PageUnit = GraphicsUnit.Pixel;
          g.DrawImageUnscaled(image, 0, 0);
        }

        // Vertically flip image as we are about to store it as BMP on a memory stream below
        // This way we don't need to worry about BMP being upside-down when copying to byte array
        bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

        using MemoryStream ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Bmp);
        ms.Seek(54, SeekOrigin.Begin);
        ms.Read(data, 0, data.Length);
      }

      //Set the data
      Data = data;
      Width = (uint)image.Width;
      Height = (uint)image.Height;
      Format = FourCC('R', 'G', 'B', '3');
    }

    /// <value>
    /// Gets the zbar_image_t ptr held by this instance.
    /// </value>
    internal IntPtr Handle => _handle;

    #region Wrapper methods

    /// <summary>
    /// Get a new <see cref="Bitmap"/> containing the a copy of the current image, converted to the FourCC RGB3 format.
    /// </summary>
    /// <returns>
    /// A <see cref="Bitmap"/> copy of this image
    /// </returns>
    public Bitmap ToBitmap()
    {
      Bitmap img = new Bitmap((int)Width, (int)Height, PixelFormat.Format24bppRgb);
      //TODO: Test and optimize this :)
      using (Image rgb = Convert(FourCC('R', 'G', 'B', '3')))
      {
        byte[] data = rgb.Data;
        BitmapData bdata = img.LockBits(new Rectangle(0, 0, img.Width, img.Height),
                                        ImageLockMode.WriteOnly,
                                        PixelFormat.Format24bppRgb);
        Marshal.Copy(data, 0, bdata.Scan0, data.Length);
        img.UnlockBits(bdata);
      }

      return img;
    }

    /// <value>
    /// Get/set the pixel width. Doesn't affect underlying data.
    /// </value>
    public uint Width
    {
      get => _width;
      set => zbar_image_set_size(_handle, _width = value, _height);
    }

    /// <value>
    /// Get/set the pixel height. Doesn't affect underlying data.
    /// </value>
    public uint Height
    {
      get => _height;
      set => zbar_image_set_size(_handle, _width, _height = value);
    }

    /// <value>
    /// Get/set the fourcc image format code for image sample data. 
    /// </value>
    /// <remarks>
    /// Chaning this doesn't affect the data.
    /// See Image.FourCC for how to get the fourCC code.
    /// For information on supported format see:
    /// http://sourceforge.net/apps/mediawiki/zbar/index.php?title=Supported_image_formats
    /// </remarks>
    public uint Format
    {
      get => zbar_image_get_format(_handle);
      set => zbar_image_set_format(_handle, value);
    }

    /// <value>
    /// Get/set a "sequence" (page/frame) number associated with this image. 
    /// </value>
    public uint SequenceNumber
    {
      get => zbar_image_get_sequence(_handle);
      set => zbar_image_set_sequence(_handle, value);
    }

    /// <value>
    /// Get/set the data associated with this image
    /// </value>
    /// <remarks>This method copies that data, using Marshal.Copy.</remarks>
    public byte[] Data
    {
      get
      {
        IntPtr pData = zbar_image_get_data(_handle);
        if (pData == IntPtr.Zero)
          throw new Exception("Image data pointer is null!");
        uint length = zbar_image_get_data_length(_handle);
        byte[] data = new byte[length];
        Marshal.Copy(pData, data, 0, (int)length);
        return data;
      }
      set
      {
        IntPtr data = Marshal.AllocHGlobal(value.Length);
        Marshal.Copy(value, 0, data, value.Length);
        zbar_image_set_data(_handle, data, (uint)value.Length, CleanupHandler);
      }
    }

    /// <summary>
    /// Cleanup handler, by holding the reference statically the delegate won't be released
    /// </summary>
    private static zbar_image_cleanup_handler CleanupHandler = new zbar_image_cleanup_handler(ReleaseAllocatedUnmanagedMemory);

    private static void ReleaseAllocatedUnmanagedMemory(IntPtr image)
    {
      IntPtr pData = zbar_image_get_data(image);
      if (pData != IntPtr.Zero)
        Marshal.FreeHGlobal(pData);
    }

    /// <value>
    /// Get ImageScanner decode result iterator. 
    /// </value>
    public IEnumerable<Symbol> Symbols
    {
      get
      {
        IntPtr pSym = zbar_image_first_symbol(_handle);
        while (pSym != IntPtr.Zero)
        {
          yield return new Symbol(pSym);
          pSym = Symbol.zbar_symbol_next(pSym);
        }
      }
    }

    /// <summary>
    /// Image format conversion. refer to the documentation for supported image formats
    /// </summary>
    /// <remarks>
    /// The converted image size may be rounded (up) due to format constraints.
    /// See Image.FourCC for how to get the fourCC code.
    /// </remarks>
    /// <param name="format">
    /// FourCC format to convert to.
    /// </param>
    /// <returns>
    /// A new <see cref="Image"/> with the sample data from the original image converted to the requested format.
    /// The original image is unaffected.
    /// </returns>
    public Image Convert(uint format)
    {
      IntPtr img = zbar_image_convert(_handle, format);
      if (img == IntPtr.Zero)
        throw new ZBarException(_handle);
      return new Image(img, false);
    }

    /// <summary>
    /// Get FourCC code from four chars
    /// </summary>
    /// <remarks>
    /// See FourCC.org for more information on FourCC.
    /// For information on format supported by zbar see:
    /// http://sourceforge.net/apps/mediawiki/zbar/index.php?title=Supported_image_formats
    /// </remarks>
    public static uint FourCC(char c0, char c1, char c2, char c3)
    {
      return (uint)c0 | ((uint)c1) << 8 | ((uint)c2) << 16 | ((uint)c3) << 24;
    }

    #endregion

    #region IDisposable Implementation
    //This pattern for implementing IDisposable is recommended by:
    //Framework Design Guidelines, 2. Edition, Section 9.4

    /// <summary>
    /// Dispose this object
    /// </summary>
    /// <remarks>
    /// This boolean disposing parameter here ensures that objects with a finalizer is not disposed,
    /// this is method is invoked from the finalizer. Do overwrite, and call, this method in base 
    /// classes if you use any unmanaged resources.
    /// </remarks>
    /// <param name="disposing">
    /// A <see cref="System.Boolean"/> False if called from the finalizer, True if called from Dispose.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
      if (_handle != IntPtr.Zero)
      {
        zbar_image_destroy(_handle);
        _handle = IntPtr.Zero;
      }
    }

    /// <summary>
    /// Release resources held by this object
    /// </summary>
    public void Dispose()
    {
      //We're disposing this object and can release objects that are finalizable
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalize this object
    /// </summary>
    ~Image()
    {
      //Dispose this object, but do NOT release finalizable objects, we don't know in which order
      //these are release and they may already be finalized.
      this.Dispose(false);
    }
    #endregion

    #region Extern C functions
    /// <summary>new image constructor.
    /// </summary>
    /// <returns>
    /// a new image object with uninitialized data and format.
    /// this image should be destroyed (using zbar_image_destroy()) as
    /// soon as the application is finished with it
    /// </returns>
    [DllImport("libzbar")]
    private static extern IntPtr zbar_image_create();

    /// <summary>image destructor.  all images created by or returned to the
    /// application should be destroyed using this function.  when an image
    /// is destroyed, the associated data cleanup handler will be invoked
    /// if available
    /// </summary><remarks>
    /// make no assumptions about the image or the data buffer.
    /// they may not be destroyed/cleaned immediately if the library
    /// is still using them.  if necessary, use the cleanup handler hook
    /// to keep track of image data buffers
    /// </remarks>
    [DllImport("libzbar")]
    private static extern void zbar_image_destroy(IntPtr image);

    /// <summary>image reference count manipulation.
    /// increment the reference count when you store a new reference to the
    /// image.  decrement when the reference is no longer used.  do not
    /// refer to the image any longer once the count is decremented.
    /// zbar_image_ref(image, -1) is the same as zbar_image_destroy(image)
    /// </summary>
    [DllImport("libzbar")]
    private static extern void zbar_image_ref(IntPtr image, int refs);

    /// <summary>image format conversion.  refer to the documentation for supported
    /// image formats
    /// </summary>
    /// <returns> a new image with the sample data from the original image
    /// converted to the requested format.  the original image is
    /// unaffected.
    /// </returns>
    /// <remarks> the converted image size may be rounded (up) due to format
    /// constraints
    /// </remarks>
    [DllImport("libzbar")]
    private static extern IntPtr zbar_image_convert(IntPtr image, uint format);

    /// <summary>image format conversion with crop/pad.
    /// if the requested size is larger than the image, the last row/column
    /// are duplicated to cover the difference.  if the requested size is
    /// smaller than the image, the extra rows/columns are dropped from the
    /// right/bottom.
    /// </summary>
    /// <returns> a new image with the sample data from the original
    /// image converted to the requested format and size.
    /// </returns>
    /// <remarks>the image is not scaled</remarks>
    [DllImport("libzbar")]
    private static extern IntPtr zbar_image_convert_resize(IntPtr image, uint format, uint width, uint height);

    /// <summary>retrieve the image format.
    /// </summary>
    /// <returns> the fourcc describing the format of the image sample data</returns>
    [DllImport("libzbar")]
    private static extern uint zbar_image_get_format(IntPtr image);

    /// <summary>retrieve a "sequence" (page/frame) number associated with this image.
    /// </summary>
    [DllImport("libzbar")]
    private static extern uint zbar_image_get_sequence(IntPtr image);

    /// <summary>retrieve the width of the image.
    /// </summary>
    /// <returns> the width in sample columns</returns>
    [DllImport("libzbar")]
    private static extern uint zbar_image_get_width(IntPtr image);

    /// <summary>retrieve the height of the image.
    /// </summary>
    /// <returns> the height in sample rows</returns>
    [DllImport("libzbar")]
    private static extern uint zbar_image_get_height(IntPtr image);

    /// <summary>return the image sample data.  the returned data buffer is only
    /// valid until zbar_image_destroy() is called
    /// </summary>
    [DllImport("libzbar")]
    private static extern IntPtr zbar_image_get_data(IntPtr image);

    /// <summary>return the size of image data.
    /// </summary>
    [DllImport("libzbar")]
    private static extern uint zbar_image_get_data_length(IntPtr img);

    /// <summary>image_scanner decode result iterator.
    /// </summary>
    /// <returns> the first decoded symbol result for an image
    /// or NULL if no results are available
    /// </returns>
    [DllImport("libzbar")]
    private static extern IntPtr zbar_image_first_symbol(IntPtr image);

    /// <summary>specify the fourcc image format code for image sample data.
    /// refer to the documentation for supported formats.
    /// </summary>
    /// <remarks> this does not convert the data!
    /// (see zbar_image_convert() for that)
    /// </remarks>
    [DllImport("libzbar")]
    private static extern void zbar_image_set_format(IntPtr image, uint format);

    /// <summary>associate a "sequence" (page/frame) number with this image.
    /// </summary>
    [DllImport("libzbar")]
    private static extern void zbar_image_set_sequence(IntPtr image, uint sequence_num);

    /// <summary>specify the pixel size of the image.
    /// </summary>
    /// <remarks>this does not affect the data!</remarks>
    [DllImport("libzbar")]
    private static extern void zbar_image_set_size(IntPtr image, uint width, uint height);

    /// <summary>
    /// Cleanup handler callback for image data.
    /// </summary>
    private delegate void zbar_image_cleanup_handler(IntPtr image);

    /// <summary>specify image sample data.  when image data is no longer needed by
    /// the library the specific data cleanup handler will be called
    /// (unless NULL)
    /// </summary>
    /// <remarks>application image data will not be modified by the library</remarks>
    [DllImport("libzbar")]
    private static extern void zbar_image_set_data(IntPtr image, IntPtr data, uint data_byte_length, zbar_image_cleanup_handler cleanup_handler);

    /// <summary>built-in cleanup handler.
    /// passes the image data buffer to free()
    /// </summary>
    [DllImport("libzbar")]
    private static extern void zbar_image_free_data(IntPtr image);

    /// <summary>associate user specified data value with an image.
    /// </summary>
    [DllImport("libzbar")]
    private static extern void zbar_image_set_userdata(IntPtr image, IntPtr userdata);

    /// <summary>return user specified data value associated with the image.
    /// </summary>
    [DllImport("libzbar")]
    private static extern IntPtr zbar_image_get_userdata(IntPtr image);
    #endregion
  }
}
