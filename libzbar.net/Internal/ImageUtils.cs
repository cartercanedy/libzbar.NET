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

    internal static uint ToFourCC(this PixelFormat pixFormat)
    {
      return pixFormat switch
      {
        PixelFormat.Format24bppRgb => FourCC('R', 'G', 'B', '3'),
        PixelFormat.Format32bppArgb => FourCC('R', 'G', 'B', '4'),
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
      return (uint)c0 | ((uint)c1) << 8 | ((uint)c2) << 16 | ((uint)c3) << 24;
    }
  }
}
