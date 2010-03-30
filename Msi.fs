namespace Wif

open System
open System.Text
open Wif.Interop

module Msi =
  
  let guidLength = 38
  let maxPath = 260
  
  // MSI API Functions & Constants
  
  let enumProducts() =
    let i = ref 0u
    let product = StringBuilder guidLength
    seq {
      while MsiDll.MsiEnumProducts(!i, product) = uint32 Error.Success do
        yield product.ToString()
        i := !i + 1u
    }
    
  let enumFeatures product =
    let i = ref 0u
    let feature = StringBuilder guidLength
    let parent = StringBuilder guidLength
    seq {
      while MsiDll.MsiEnumFeatures(product, !i, feature, parent) = uint32 Error.Success do
        yield feature.ToString(), parent.ToString()
        i := !i + 1u
    }
  
  let getProductInfo product property (valueCapacity:int32) =
    let value = StringBuilder valueCapacity
    let mutable capacity = uint32 valueCapacity
    if MsiDll.MsiGetProductInfo(product, property, value, &capacity) = uint32 Error.Success then
      value.ToString()
    else
      String.Empty
  
  module ProductInfo =
    // advertised information
    let PackageName = "PackageName"
    let Transforms = "Transforms"
    let Language = "Language"
    let ProductName = "ProductName"
    let AssignmentType = "AssignmentType"
    // if >= 150
    let InstanceType = "InstanceType"
    // if >= 300
    let AuthorizedLUAApp = "AuthorizedLUAApp"
    
    let getPackageName product = getProductInfo product PackageName maxPath
    let getProductName product = getProductInfo product ProductName maxPath
    
    // installed information
    let InstalledProductName = "InstalledProductName"
    let VersionString = "VersionString"
    let HelpLink = "HelpLink"
    let HelpTelephone = "HelpTelephone"
    let InstallLocation = "InstallLocation"
    let InstallSource = "InstallSource"
    let InstallDate = "InstallDate"
    let Publisher = "Publisher"
    let LocalPackage = "LocalPackage"
    let URLInfoAbout = "URLInfoAbout"
    let URLUpdateInfo = "URLUpdateInfo"
    let VersionMinor = "VersionMinor"
    let VersionMajor = "VersionMajor"
    let ProductID = "ProductID"
    let RegCompany = "RegCompany"
    let RegOwner = "RegOwner"
    // if >= 300
    let Uninstallable = "Uninstallable"
    let State = "State"
    let PatchType = "PatchType"
    let LUAEnabled = "LUAEnabled"
    let DisplayName = "DisplayName"
    let MoreInfoURL = "MoreInfoURL"
    
    let getInstalledProductName product = getProductInfo product InstalledProductName maxPath
    let getVersionString product = getProductInfo product VersionString maxPath
    let getInstallLocation product = getProductInfo product InstallLocation maxPath
    
  module SourceListInfo =
    // advertised information
    let LastUsedSource = "LastUsedSource"
    let LastUsedType = "LastUsedType"
    let MediaPackagePath = "MediaPackagePath"
    let DiskPrompt = "DiskPrompt"
  
  module OpenDatabasePersist =
    let ReadOnly = IntPtr 0
    let Transact = 1
    let Direct = 2
    let Create = 3
    let CreateDirect = 4
  
  let openDatabase path persist =
    let mutable database = IntPtr.Zero
    checkSuccess (MsiDll.MsiOpenDatabase(path, persist, &database))
    database
  
  let databaseOpenView database query =
    let mutable view = IntPtr.Zero
    checkSuccess (MsiDll.MsiDatabaseOpenView(database, query, &view))
    view
  
  let viewExecute view record =
    checkSuccess (MsiDll.MsiViewExecute(view, record))
  
  let viewFetch view =
    let mutable record = IntPtr.Zero
    MsiDll.MsiViewFetch(view, &record) |> ignore
    record
  
  let closeHandle handle =
    MsiDll.MsiCloseHandle(handle) |> ignore
  
  let recordGetFieldCount record =
    MsiDll.MsiRecordGetFieldCount record
  
  let recordGetString record field (valueCapacity:int32) =
    let value = StringBuilder valueCapacity
    let mutable capacity = uint32 valueCapacity
    if MsiDll.MsiRecordGetString(record, field, value, &capacity) = uint32 Error.Success then
      value.ToString()
    else
      String.Empty
  
  let recordGetInteger record field =
    MsiDll.MsiRecordGetInteger(record, field)
  
  let verifyPackage path =
    checkSuccess (MsiDll.MsiVerifyPackage(path))
  
  let createRecord fields =
    MsiDll.MsiCreateRecord fields
    
  let recordSetString record fieldIndex value =
    MsiDll.MsiRecordSetString(record, fieldIndex, value)
  
  let recordSetInteger record fieldIndex value =
    MsiDll.MsiRecordSetInteger(record, fieldIndex, value)
  
  let viewGetColumnInfo view columnInfo =
    let mutable record = IntPtr.Zero
    checkSuccess (MsiDll.MsiViewGetColumnInfo(view, columnInfo, &record))
    let count = int32 (recordGetFieldCount record)
    let values = Array.zeroCreate<string> count
    for i = 1 to count do
      values.[i-1] <- recordGetString record (uint32 i) maxPath
    closeHandle record
    values
   
  // WIF API Installed
  
  let products = enumProducts
  let features = enumFeatures
  
  // WIF API Database
  
  let view = databaseOpenView
  
  // you must close the view handle yourself
  let viewSeq view args fn =
    viewExecute view args
    let record = ref IntPtr.Zero
    seq {
      while (record := viewFetch view; !record <> IntPtr.Zero) do
        let value = fn !record
        closeHandle !record
        yield value
    }
  
  // closes the view handle for you
  let viewList view args fn =
    let values = viewSeq view args fn |> List.of_seq
    closeHandle view
    values
  
  let viewListNoArgs view fn =
    viewList view IntPtr.Zero fn
  
  let tables msi =
    let v = view msi "SELECT `Name` FROM `_Tables`"
    viewListNoArgs v (fun record ->
      recordGetString record 1u maxPath
    )

  let columns msi table =
    let v = view msi "SELECT `Number`, `Name` FROM `_Columns` WHERE `Table` = ?"
    let args = createRecord 1u
    checkSuccess (recordSetString args 1u table)
    let columns = viewSeq v args (fun record ->
      let number = recordGetInteger record 1u
      let name = recordGetString record 2u maxPath
      number, name
    )
    closeHandle args
    columns
  
  let viewColumns view =
    let names = viewGetColumnInfo view 0
    let types = viewGetColumnInfo view 1
    Array.zip names types
  
  let viewAll msi table =
    let query = sprintf "SELECT * FROM `%s`" table 
    let v = view msi query
    let columns = viewColumns v
    let data = viewListNoArgs v (fun record ->
      let row = Array.zeroCreate<string> columns.Length
      for i = 0 to (columns.Length - 1) do
        row.[i] <- sprintf "data %d" i // TODO
      row
    )
    columns, data