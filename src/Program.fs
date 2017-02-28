open FsInclude.Trace

open FSharp.Core.Printf

open System
open System.IO
open System.Net
open System.Text

type Configuration =
  {
    Repository  : string
    Namespace   : string
    Public      : bool
    Files       : string []
  }
  static member FsInclude  : Configuration = 
    { 
      Repository  = "https://raw.githubusercontent.com/atomizearm/FsInclude/mrange/initial/src"
      Namespace   = "FsInclude"
      Public      = false
      Files       = [||] 
    }

let config = { Configuration.FsInclude with Files = [|"Trace.fs"; "Numerical.fs"; "Disposable.fs"|] }

let download (config : Configuration) : string =
  let sb = StringBuilder 1024

  let app   msg = sb.AppendLine   (msg : string)                            |> ignore
  let line  ()  = app "// ----------------------------------------------------------------"
  let inf   msg = sb.AppendFormat ("// @@@ INFO : {0}\n", (msg: string))  |> ignore
  let err   msg = sb.AppendFormat ("// @@@ ERROR: {0}\n", (msg: string))  |> ignore
  let inff  fmt = kprintf inf fmt
  let errf  fmt = kprintf err fmt

  let get file =
    try
      use wc  = new WebClient ()
      let u   = sprintf "%s/%s" config.Repository file
      inff "Downloading file: %s" file
      let c   = wc.DownloadString u
      c
    with
    | e -> 
      errf "While processing file: %s caught exception: %s" file e.Message
      ""

  line ()
  inf  "FsInclude - https://raw.githubusercontent.com/atomizearm/FsInclude"
  line ()
  inff "Repository: %s" config.Repository
  inff "EnclosedBy: %s" config.Namespace
  inff "Public    : %b" config.Public
  inff "File count: %d" config.Files.Length
  line ()

  for file in config.Files do
    use sr = new StringReader (get file)
    line ()
    let rec loop () =
      let line = sr.ReadLine ()
      if line |> isNull |> not then
        app line
        loop ()
    loop ()
    line ()

  sb.ToString ()

[<EntryPoint>]
let main argv = 
  printfn "%s" <| download config
  0
