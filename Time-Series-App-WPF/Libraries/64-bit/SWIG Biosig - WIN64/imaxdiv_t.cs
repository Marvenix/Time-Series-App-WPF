//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace BiosigLibWin64 {

public class imaxdiv_t : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal imaxdiv_t(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(imaxdiv_t obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~imaxdiv_t() {
    Dispose(false);
  }

  public void Dispose() {
    Dispose(true);
    global::System.GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          biosigPINVOKE.delete_imaxdiv_t(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public long quot {
    set {
      biosigPINVOKE.imaxdiv_t_quot_set(swigCPtr, value);
    } 
    get {
      long ret = biosigPINVOKE.imaxdiv_t_quot_get(swigCPtr);
      return ret;
    } 
  }

  public long rem {
    set {
      biosigPINVOKE.imaxdiv_t_rem_set(swigCPtr, value);
    } 
    get {
      long ret = biosigPINVOKE.imaxdiv_t_rem_get(swigCPtr);
      return ret;
    } 
  }

  public imaxdiv_t() : this(biosigPINVOKE.new_imaxdiv_t(), true) {
  }

}

}