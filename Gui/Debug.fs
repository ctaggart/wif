namespace Wif

open System.Diagnostics

/// debug print functions
// credit: blog by Dustin Campbell on 2008-12-29 "Printf and Formatting Debug Output in F#"
// http://diditwith.net/CommentView,guid,befb5f67-34d7-4ab2-a510-eb94cb4f6666.aspx#commentstart

module Debug =

  let dprintf fmt = Printf.ksprintf Debug.Write fmt
  let dprintfn fmt = Printf.ksprintf Debug.WriteLine fmt