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

public class HDR_STRUCT_Patient : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal HDR_STRUCT_Patient(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(HDR_STRUCT_Patient obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~HDR_STRUCT_Patient() {
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
          biosigPINVOKE.delete_HDR_STRUCT_Patient(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public long Birthday {
    set {
      biosigPINVOKE.HDR_STRUCT_Patient_Birthday_set(swigCPtr, value);
    } 
    get {
      long ret = biosigPINVOKE.HDR_STRUCT_Patient_Birthday_get(swigCPtr);
      return ret;
    } 
  }

  public SWIGTYPE_p_unsigned_short Headsize {
    set {
      biosigPINVOKE.HDR_STRUCT_Patient_Headsize_set(swigCPtr, SWIGTYPE_p_unsigned_short.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = biosigPINVOKE.HDR_STRUCT_Patient_Headsize_get(swigCPtr);
      SWIGTYPE_p_unsigned_short ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_unsigned_short(cPtr, false);
      return ret;
    } 
  }

  public string Name {
    set {
      biosigPINVOKE.HDR_STRUCT_Patient_Name_set(swigCPtr, value);
    } 
    get {
      string ret = biosigPINVOKE.HDR_STRUCT_Patient_Name_get(swigCPtr);
      return ret;
    } 
  }

  public string Id {
    set {
      biosigPINVOKE.HDR_STRUCT_Patient_Id_set(swigCPtr, value);
    } 
    get {
      string ret = biosigPINVOKE.HDR_STRUCT_Patient_Id_get(swigCPtr);
      return ret;
    } 
  }

  public byte Weight {
    set {
      biosigPINVOKE.HDR_STRUCT_Patient_Weight_set(swigCPtr, value);
    } 
    get {
      byte ret = biosigPINVOKE.HDR_STRUCT_Patient_Weight_get(swigCPtr);
      return ret;
    } 
  }

  public byte Height {
    set {
      biosigPINVOKE.HDR_STRUCT_Patient_Height_set(swigCPtr, value);
    } 
    get {
      byte ret = biosigPINVOKE.HDR_STRUCT_Patient_Height_get(swigCPtr);
      return ret;
    } 
  }

  public sbyte Sex {
    set {
      biosigPINVOKE.HDR_STRUCT_Patient_Sex_set(swigCPtr, value);
    } 
    get {
      sbyte ret = biosigPINVOKE.HDR_STRUCT_Patient_Sex_get(swigCPtr);
      return ret;
    } 
  }

  public sbyte Handedness {
    set {
      biosigPINVOKE.HDR_STRUCT_Patient_Handedness_set(swigCPtr, value);
    } 
    get {
      sbyte ret = biosigPINVOKE.HDR_STRUCT_Patient_Handedness_get(swigCPtr);
      return ret;
    } 
  }

  public sbyte Smoking {
    set {
      biosigPINVOKE.HDR_STRUCT_Patient_Smoking_set(swigCPtr, value);
    } 
    get {
      sbyte ret = biosigPINVOKE.HDR_STRUCT_Patient_Smoking_get(swigCPtr);
      return ret;
    } 
  }

  public sbyte AlcoholAbuse {
    set {
      biosigPINVOKE.HDR_STRUCT_Patient_AlcoholAbuse_set(swigCPtr, value);
    } 
    get {
      sbyte ret = biosigPINVOKE.HDR_STRUCT_Patient_AlcoholAbuse_get(swigCPtr);
      return ret;
    } 
  }

  public sbyte DrugAbuse {
    set {
      biosigPINVOKE.HDR_STRUCT_Patient_DrugAbuse_set(swigCPtr, value);
    } 
    get {
      sbyte ret = biosigPINVOKE.HDR_STRUCT_Patient_DrugAbuse_get(swigCPtr);
      return ret;
    } 
  }

  public sbyte Medication {
    set {
      biosigPINVOKE.HDR_STRUCT_Patient_Medication_set(swigCPtr, value);
    } 
    get {
      sbyte ret = biosigPINVOKE.HDR_STRUCT_Patient_Medication_get(swigCPtr);
      return ret;
    } 
  }

  public HDR_STRUCT_Patient_Impairment Impairment {
    get {
      global::System.IntPtr cPtr = biosigPINVOKE.HDR_STRUCT_Patient_Impairment_get(swigCPtr);
      HDR_STRUCT_Patient_Impairment ret = (cPtr == global::System.IntPtr.Zero) ? null : new HDR_STRUCT_Patient_Impairment(cPtr, false);
      return ret;
    } 
  }

  public HDR_STRUCT_Patient() : this(biosigPINVOKE.new_HDR_STRUCT_Patient(), true) {
  }

}

}