using System;
using System.Runtime.InteropServices;

namespace ZBar.Interop
{
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
    SEV_FATAL = -2,
    SEV_ERROR = -1,
    SEV_OK = 0,
    SEV_WARNING = 1,
    SEV_NOTE = 2
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
}
