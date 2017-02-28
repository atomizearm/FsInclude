// Copyright 2017 Atomize AB
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace FsInclude

[<RequireQualifiedAccess>]
type internal TraceLevel =
  | Success     = 0x10
  | Highlight   = 0x08
  | Info        = 0x04
  | Warning     = 0x02
  | Error       = 0x01

module internal Trace =
  open FSharp.Core.Printf
  open System
  open System.Text

  let mutable traceMask = 0x10 // All

  module Tracer =
    type internal ITracer =
      interface
        abstract Trace        : TraceLevel*string -> unit
      end

    let prelude tl =
      match tl with
        | TraceLevel.Success    -> "SUCCESS   : "
        | TraceLevel.Highlight  -> "HIGHLIGHT : "
        | TraceLevel.Info       -> "INFO      : "
        | TraceLevel.Warning    -> "WARNING   : "
        | TraceLevel.Error      -> "ERROR     : "
        | _                     -> "UNKNOWN   : "

    let shouldTrace tl = (traceMask &&& (int (tl : TraceLevel))) <> 0
     
    let traceableMessage tl msg = 
      let prelude = prelude tl
      let sb      = StringBuilder prelude
      ignore      <| sb.Append (msg : string)
      sb.ToString ()

    let diagnosticTracer =
      { new ITracer with
          member x.Trace (tl, msg) = if shouldTrace tl then traceableMessage tl msg |> System.Diagnostics.Trace.WriteLine
      }

    let consoleTracer =
      { new ITracer with
          member x.Trace (tl, msg) = 
            if shouldTrace tl then
              let tm  = traceableMessage tl msg
              let old = Console.ForegroundColor
              try
                Console.ForegroundColor <- 
                  match tl with
                    | TraceLevel.Success    -> ConsoleColor.Green
                    | TraceLevel.Highlight  -> ConsoleColor.White
                    | TraceLevel.Info       -> ConsoleColor.Gray
                    | TraceLevel.Warning    -> ConsoleColor.Yellow
                    | TraceLevel.Error      -> ConsoleColor.Red
                    | _                     -> ConsoleColor.Red
                Console.WriteLine tm
              finally
                Console.ForegroundColor <- old
      }

    let defaultTracer = consoleTracer

    let mutable tracer = defaultTracer

  open Tracer

  let success     msg = tracer.Trace (TraceLevel.Success    , msg)
  let highlight   msg = tracer.Trace (TraceLevel.Highlight  , msg)
  let info        msg = tracer.Trace (TraceLevel.Info       , msg)
  let warning     msg = tracer.Trace (TraceLevel.Warning    , msg)
  let error       msg = tracer.Trace (TraceLevel.Error      , msg)

  let successf    fmt = kprintf success   fmt
  let highlightf  fmt = kprintf highlight fmt
  let infof       fmt = kprintf info      fmt
  let warningf    fmt = kprintf warning   fmt
  let errorf      fmt = kprintf error     fmt

