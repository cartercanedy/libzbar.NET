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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using static ZBar.Native.NativeFunctions;
using static ZBar.Internal.ImageUtils;
using ZBar.Native;

namespace ZBar
{
  /// <summary>
  /// The available formats for conversions from <see cref="Image"/> to <see cref="Bitmap"/>.
  /// </summary>
  public enum SupportedBitmapFormat
  {
    RGB3 = 0,
    RGB4,
  }

  /// <summary>
  /// Representation of an image in ZBar
  /// </summary>
  public class Image : IDisposable
  {
    private IntPtr _handle = IntPtr.Zero;
    private uint _width;
    private uint _height;

    private void SetData(byte[] data)
    {
      IntPtr lData = Marshal.AllocHGlobal(data.Length);

      Marshal.Copy(data, 0, lData, data.Length);

      zbar_image_set_data(_handle, lData, (uint)data.Length, Release);
    }

    /// <summary>
    /// Create a new image from an unmanaged object reference
    /// </summary>
    /// <remarks>This resource will be managed by this Image instance.</remarks>
    /// <param name="ptr">
    /// A <see cref="IntPtr"/> to unmananged ZBar image.
    /// </param>
    internal Image(IntPtr ptr)
    {
      if (ptr == IntPtr.Zero)
      {
        throw new Exception($"{nameof(Image)} error: Passed a nullptr into ctor.");
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
      using (Bitmap bitmap = new(image.Width, image.Height, PixelFormat.Format24bppRgb))
      {
        using (var g = Graphics.FromImage(bitmap))
        {
          g.PageUnit = GraphicsUnit.Pixel;
          g.DrawImageUnscaled(image, 0, 0);
        }

        // Vertically flip image as we are about to store it as BMP on a memory stream below
        // This way we don't need to worry about BMP being upside-down when copying to byte array
        bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

        using MemoryStream ms = new();
        bitmap.Save(ms, ImageFormat.Bmp);
        ms.Seek(54, SeekOrigin.Begin);
        ms.Read(data, 0, data.Length);
      }

      //Set the data
      SetData(data);
      Width = (uint)image.Width;
      Height = (uint)image.Height;
      Format = FourCC("RGB3");
    }

    /// <value>
    /// Gets the zbar_image_t ptr held by this instance.
    /// </value>
    internal IntPtr Handle => _handle;

    #region Wrapper methods


    /// <summary>
    /// Get a new <see cref="Bitmap"/> containing the a copy of the current image with the pixel format specified with <paramref name="pixelFormat"/>.
    /// </summary>
    /// <param name="pixelFormat">The pixel format to use for the <see cref="Bitmap"/> conversion.</param>
    /// <returns>
    /// A <see cref="Bitmap"/> copy of this image
    /// </returns>
    public Bitmap ToBitmap(PixelFormat pixelFormat)
    {
      Bitmap lBmp = new((int)_width, (int)_height, PixelFormat.Format24bppRgb);
      BitmapData lBmpData = lBmp.LockBits(new(0, 0, lBmp.Width, lBmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
      //TODO: Test and optimize this :)

      uint l4CC = pixelFormat.ToFourCC();

      IntPtr pNewImage = ConvertZBarImage(_handle, l4CC);
      IntPtr pNewImageData = zbar_image_get_data(pNewImage);

      int lNewImageDataLen = (int)zbar_image_get_data_length(pNewImage);

      byte[] lNewImageData = new byte[lNewImageDataLen];

      Marshal.Copy(pNewImageData, lNewImageData, 0, lNewImageDataLen);
      Marshal.Copy(lNewImageData, 0, lBmpData.Scan0, lNewImageDataLen);

      zbar_image_destroy(pNewImage);

      lBmp.UnlockBits(lBmpData);

      return lBmp;
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

    public Span<byte> GetData()
    {
      if (_handle == IntPtr.Zero) throw new NullReferenceException($"{typeof(Image).FullName} exception: image ptr was null");
      IntPtr pData = zbar_image_get_data(_handle);

      if (pData == IntPtr.Zero) throw new ZBarException(_handle);

      int lDataLength = (int)zbar_image_get_data_length(_handle);

      byte[] lData = new byte[lDataLength];

      Marshal.Copy(pData, lData, 0, lDataLength);

      return lData;
    }

    /// <value>
    /// Get/set the data associated with this image
    /// </value>
    /// <remarks>This method copies that data, using Marshal.Copy.</remarks>
    [Obsolete($"Getting/setting data via Image.{nameof(Data)} property is deprecated, use Image.{nameof(GetData)} instead", false)]
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
        zbar_image_set_data(_handle, data, (uint)value.Length, Release);
      }
    }

    private static void Release(IntPtr image)
    {
      IntPtr pData = zbar_image_get_data(image);

      if (pData != IntPtr.Zero)
      {
        Marshal.FreeHGlobal(pData);
      }
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
          pSym = NativeFunctions.zbar_symbol_next(pSym);
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
      return new Image(img);
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
  }
}
