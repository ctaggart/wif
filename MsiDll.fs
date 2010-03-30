namespace Wif

open System
open System.Text
open System.Runtime.InteropServices

module MsiDll =

  [<DllImport("msi.dll")>]
  extern uint32 MsiEnumProducts(uint32 index, StringBuilder productCode)
  
  [<DllImport("msi.dll")>]
  extern uint32 MsiEnumFeatures(String product, uint32 index, StringBuilder feature, StringBuilder parent)
  
  [<DllImport("msi.dll")>]
  extern uint32 MsiGetProductInfo(String product, String property, StringBuilder value, uint32& valueCapacity)
  
  [<DllImport("msi.dll")>]
  extern uint32 MsiOpenDatabase(String path, IntPtr persist, IntPtr& msi)
  
  [<DllImport("msi.dll")>]
  extern uint32 MsiDatabaseOpenView(IntPtr database, String query, IntPtr& view)
  
  [<DllImport("msi.dll")>]
  extern uint32 MsiViewExecute(IntPtr view, IntPtr record)
  
  [<DllImport("msi.dll")>]
  extern uint32 MsiViewFetch(IntPtr view, IntPtr& record)
  
  [<DllImport("msi.dll")>]
  extern uint32 MsiViewGetColumnInfo(IntPtr view, int32 columnInfo, IntPtr& record)
  
  [<DllImport("msi.dll")>]
  extern uint32 MsiRecordGetFieldCount(IntPtr record)
  
  [<DllImport("msi.dll")>]
  extern uint32 MsiRecordGetString(IntPtr record, uint32 field, StringBuilder value, uint32& valueCapacity)
  
  [<DllImport("msi.dll")>]
  extern int32 MsiRecordGetInteger(IntPtr record, uint32 field)
  
  [<DllImport("msi.dll")>]
  extern uint32 MsiCloseHandle(IntPtr handle)
  
  [<DllImport("msi.dll")>]
  extern uint32 MsiVerifyPackage(String path)
  
  [<DllImport("msi.dll")>]
  extern IntPtr MsiCreateRecord(uint32 fields)
  
  [<DllImport("msi.dll")>]
  extern uint32 MsiRecordSetString(IntPtr record, uint32 fieldIndex, String value)
  
  [<DllImport("msi.dll")>]
  extern uint32 MsiRecordSetInteger(IntPtr record, uint32 fieldIndex, int32 value)