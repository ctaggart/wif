namespace Wif

open Wif.Msi
open Wif.Debug

open System
open System.Collections.Generic
open System.Globalization
open System.Windows
open System.Windows.Controls
open System.Windows.Data
open System.Windows.Media
open Microsoft.Win32
open Microsoft.Windows.Controls

module Gui =
  
  // View WPF Types
  
  type StringArrayConverter (i:int32) =
    interface IValueConverter with
      member v.Convert (value:obj, targetType:Type, parameter:obj, culture:CultureInfo) =
        let sa = value :?> String[]
        box sa.[i]
      member v.ConvertBack (value:obj, targetType:Type, parameter:obj, culture:CultureInfo) =
        null
  
  // View factory helper functions
  
  let dock (dockPanel:DockPanel) (uiElement:UIElement) position =
    DockPanel.SetDock(uiElement, position)
    dockPanel.Children.Add uiElement |> ignore
  
  let menuItem header =
    let mi = MenuItem()
    mi.Header <- header
    mi
  
  // Controller functions
  
  let fillColumns (dg:DataGrid) (columns:seq<string*string>) (data:seq<string array>) =
    dg.Columns.Clear()
    let i = ref 0
    for name,typ in columns do
      let c = DataGridTextColumn()
      let b = Binding()
      b.Mode <- BindingMode.OneWay
      b.Converter <- StringArrayConverter !i
      c.Binding <- b
      c.Header <- name
      dg.Columns.Add c
      incr i
      ()
    dg.ItemsSource <- data
    ()
  
  let main() =
    
    // View
    
    let lbTables = ListBox()

    let m = Menu()
    let miFile = menuItem "_File"
    let miOpen = menuItem "_Open..."
    miFile.Items.Add miOpen |> ignore
    miFile.Items.Add (Separator()) |> ignore
    let miExit = menuItem "E_xit"
    miFile.Items.Add miExit |> ignore
    m.Items.Add miFile |> ignore
    
    let dgColumns = DataGrid()
    dgColumns.AutoGenerateColumns <- false
    
    // layout using dock panel, order is important
    let dp = DockPanel()
    dock dp m Dock.Top
    dock dp lbTables Dock.Left
    dp.Children.Add dgColumns |> ignore
    
    let window = Window()
    window.Title <- "WIF GUI"
    window.Content <- dp
    
    let ofd = OpenFileDialog()
    ofd.DefaultExt <- ".msi"
    ofd.Filter <- "Windows Installer (*.msi)|*.msi"
    
    // Model
    
    let msi = ref IntPtr.Zero
    
    // Controller
    
    lbTables.SelectionChanged.Add(fun _ ->
      let table = lbTables.SelectedItem :?> string
      let columns, data = viewAll !msi table
      fillColumns dgColumns columns data
    )
        
    miOpen.Click.Add(fun _ ->
      let result = ofd.ShowDialog()
      if result.HasValue && result.Value = true then
        let path = ofd.FileName
        verifyPackage path
        msi := openDatabase path OpenDatabasePersist.ReadOnly
        
        let tables = tables !msi |> List.of_seq
        lbTables.ItemsSource <- tables |> List.sort
        //let sysTables = ["_Columns"; "_Tables"; "_Storages"; "_Streams" ]  
        //lbTables.ItemsSource <- sysTables @ tables |> List.sort
    )
    
    let app = Application()
    miExit.Click.Add(fun _ -> app.Shutdown())
    app.Run window |> ignore
    ()
  
  [<STAThread>]       
  main()