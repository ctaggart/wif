namespace Wif

open System
open System.Text
open Microsoft.FSharp.NativeInterop
open System.Runtime.InteropServices

module Interop =
  
  type Error =
    | Success = 0u
    | NonEnoughMemory = 8u
    | ErrorInvalidParameter = 87u
    | ErrorNoMoreItems = 259u
    | ErrorUnknownProduct = 1605u
    | ErrorBadConfiguration = 1610u
    
    // ERROR_INSTALL_PACKAGE_INVALID
    // ERROR_INSTALL_PACKAGE_OPEN_FAILED
  
  let checkSuccess (errorCode:uint32) =
    if errorCode = uint32 Error.Success then
      ()
    else
      raise (ComponentModel.Win32Exception(int32 errorCode))