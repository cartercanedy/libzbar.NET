using System.Runtime.InteropServices;
using System;
using static ZBar.Image;

namespace ZBar.Interop
{
  internal static class NativeFunctions
  {
    [DllImport("libzbar")]
    internal static extern int zbar_version(ref uint major, ref uint minor);

    [DllImport("libzbar")]
    internal static unsafe extern IntPtr _zbar_error_string(errinfo_t* errorInfo, int verbosity);

    /// <summary>new image constructor.
    /// </summary>
    /// <returns>
    /// a new image object with uninitialized data and format.
    /// this image should be destroyed (using zbar_image_destroy()) as
    /// soon as the application is finished with it
    /// </returns>
    [DllImport("libzbar")]
    internal static extern IntPtr zbar_image_create();

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
    internal static extern void zbar_image_destroy(IntPtr image);

    /// <summary>image reference count manipulation.
    /// increment the reference count when you store a new reference to the
    /// image.  decrement when the reference is no longer used.  do not
    /// refer to the image any longer once the count is decremented.
    /// zbar_image_ref(image, -1) is the same as zbar_image_destroy(image)
    /// </summary>
    [DllImport("libzbar")]
    internal static extern void zbar_image_ref(IntPtr image, int refs);

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
    internal static extern IntPtr zbar_image_convert(IntPtr image, uint format);

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
    internal static extern IntPtr zbar_image_convert_resize(IntPtr image, uint format, uint width, uint height);

    /// <summary>retrieve the image format.
    /// </summary>
    /// <returns> the fourcc describing the format of the image sample data</returns>
    [DllImport("libzbar")]
    internal static extern uint zbar_image_get_format(IntPtr image);

    /// <summary>retrieve a "sequence" (page/frame) number associated with this image.
    /// </summary>
    [DllImport("libzbar")]
    internal static extern uint zbar_image_get_sequence(IntPtr image);

    /// <summary>retrieve the width of the image.
    /// </summary>
    /// <returns> the width in sample columns</returns>
    [DllImport("libzbar")]
    internal static extern uint zbar_image_get_width(IntPtr image);

    /// <summary>retrieve the height of the image.
    /// </summary>
    /// <returns> the height in sample rows</returns>
    [DllImport("libzbar")]
    internal static extern uint zbar_image_get_height(IntPtr image);

    /// <summary>return the image sample data.  the returned data buffer is only
    /// valid until zbar_image_destroy() is called
    /// </summary>
    [DllImport("libzbar")]
    internal static extern IntPtr zbar_image_get_data(IntPtr image);

    /// <summary>return the size of image data.
    /// </summary>
    [DllImport("libzbar")]
    internal static extern uint zbar_image_get_data_length(IntPtr img);

    /// <summary>image_scanner decode result iterator.
    /// </summary>
    /// <returns> the first decoded symbol result for an image
    /// or NULL if no results are available
    /// </returns>
    [DllImport("libzbar")]
    internal static extern IntPtr zbar_image_first_symbol(IntPtr image);

    /// <summary>specify the fourcc image format code for image sample data.
    /// refer to the documentation for supported formats.
    /// </summary>
    /// <remarks> this does not convert the data!
    /// (see zbar_image_convert() for that)
    /// </remarks>
    [DllImport("libzbar")]
    internal static extern void zbar_image_set_format(IntPtr image, uint format);

    /// <summary>associate a "sequence" (page/frame) number with this image.
    /// </summary>
    [DllImport("libzbar")]
    internal static extern void zbar_image_set_sequence(IntPtr image, uint sequence_num);

    /// <summary>specify the pixel size of the image.
    /// </summary>
    /// <remarks>this does not affect the data!</remarks>
    [DllImport("libzbar")]
    internal static extern void zbar_image_set_size(IntPtr image, uint width, uint height);

    internal delegate void CleanupCallback(IntPtr image);

    /// <summary>specify image sample data.  when image data is no longer needed by
    /// the library the specific data cleanup handler will be called
    /// (unless NULL)
    /// </summary>
    /// <remarks>application image data will not be modified by the library</remarks>
    [DllImport("libzbar")]
    internal static extern void zbar_image_set_data(IntPtr image, IntPtr data, uint data_byte_length, CleanupCallback cleanupCallback);

    /// <summary>built-in cleanup handler.
    /// passes the image data buffer to free()
    /// </summary>
    [DllImport("libzbar")]
    internal static extern void zbar_image_free_data(IntPtr image);

    /// <summary>associate user specified data value with an image.
    /// </summary>
    [DllImport("libzbar")]
    internal static extern void zbar_image_set_userdata(IntPtr image, IntPtr userdata);

    /// <summary>return user specified data value associated with the image.
    /// </summary>
    [DllImport("libzbar")]
    internal static extern IntPtr zbar_image_get_userdata(IntPtr image);
  }
}
