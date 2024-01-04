using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Text;
using static ZBar.Interop.NativeFunctions;

namespace ZBar.Internal
{
  internal static class ImageUtils
  {
    internal static IntPtr ConvertZBarImage(IntPtr zbarImagePtr, uint fourCC)
    {
      if (zbarImagePtr == IntPtr.Zero) throw new NullReferenceException($"{nameof(ConvertZBarImage)} exception: {nameof(zbarImagePtr)}: null argument");

      IntPtr pNewImage = zbar_image_convert(zbarImagePtr, fourCC);

      if (pNewImage == IntPtr.Zero) throw new ZBarException(zbarImagePtr);

      return pNewImage;
    }

    /// <summary>
    /// Get the FourCC value that is represented by a <see cref="PixelFormat"/> enumeration.
    /// <br/><br/>
    /// Alpha-premultiplied, indexed, and non-specific <see cref="PixelFormat"/> enumerations
    /// <br/>
    /// (<see cref="PixelFormat.Extended"/>, <see cref="PixelFormat.Format32bppPArgb"/>, <see cref="PixelFormat.Format1bppIndexed"/>, etc.)
    /// <br/>
    /// will assume the FourCC code 'RGB3' (<see cref="PixelFormat.Format24bppRgb"/>) for conversion.
    /// </summary>
    /// <param name="pixFormat"></param>
    /// <returns></returns>
    internal static uint ToFourCC(this PixelFormat pixFormat)
    {
      return pixFormat switch
      {
        PixelFormat.Format16bppRgb555 or PixelFormat.Format16bppArgb1555 => FourCC("AR15"),
        PixelFormat.Format16bppRgb565 => FourCC("RGBP"),
        PixelFormat.Format16bppGrayScale => FourCC("Y16 "),
        PixelFormat.Format32bppRgb or PixelFormat.Format32bppArgb => FourCC("BA24"),
        PixelFormat.Format48bppRgb => FourCC("RGB0"),
        PixelFormat.Format64bppArgb => FourCC("b64a"),
        _ => FourCC('R', 'G', 'B', '3')
      };
    }

    /// <summary>
    /// Get FourCC code from four chars
    /// </summary>
    /// <remarks>
    /// See FourCC.org for more information on FourCC.
    /// For information on format supported by zbar see:
    /// http://sourceforge.net/apps/mediawiki/zbar/index.php?title=Supported_image_formats
    /// </remarks>
    internal static uint FourCC(char c0, char c1, char c2, char c3)
    {
      return ((uint)c0 & 0xff) | (((uint)c1 & 0xff) << 8) | (((uint)c2 & 0xff) << 16) | (((uint)c3 & 0xff) << 24);
    }

    internal static uint FourCC(string code)
    {
      if (code.Length != 4) throw new ArgumentException("Must specify a string with 4 characters");
      
      return FourCC(code[0], code[1], code[2], code[3]);
    }
  }
}
