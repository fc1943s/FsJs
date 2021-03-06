namespace FsJs

open System
open System.Collections.Generic
open FsCore


module Profiling =
    let private initialTicks = DateTime.Now.Ticks


    let profilingState =
        {|
            CountMap = Dictionary<string, int> ()
            TimestampMap = List<string * float> ()
        |}

    let rec globalProfilingState = Dom.Global.register (nameof globalProfilingState) profilingState

    let rec globalClearProfilingState =
        Dom.Global.register
            (nameof globalClearProfilingState)
            (fun () ->
                let getLocals () =
                    $"CountMap.Count={profilingState.CountMap.Count} TimestampMap.Count={profilingState.TimestampMap.Count} {getLocals ()}"

                Logger.logTrace (fun () -> "Profiling.globalClearProfilingState") getLocals
                profilingState.CountMap.Clear ()
                profilingState.TimestampMap.Clear ())

    let removeCount fn =
        if Dom.globalDebug.Get () then
            let id = fn ()

            match profilingState.CountMap.ContainsKey id with
            | false -> profilingState.CountMap.[id] <- -1
            | true -> profilingState.CountMap.[id] <- profilingState.CountMap.[id] - 1

            let getLocals () =
                $"profilingState.CountMap.[id]={profilingState.CountMap.[id]} {getLocals ()}"

            Logger.logTrace (fun () -> $"Profiling.removeCount / {id}") getLocals

    let inline private addCountMap id =
        match profilingState.CountMap.ContainsKey id with
        | false -> profilingState.CountMap.[id] <- 1
        | true -> profilingState.CountMap.[id] <- profilingState.CountMap.[id] + 1

    let addCount fn getLocals =
        if Dom.globalDebug.Get () then
            let id = fn ()
            let newTxt = $"{id} {getLocals ()}"
            addCountMap newTxt

            let getLocals () =
                $"profilingState.CountMap.[newTxt]={profilingState.CountMap.[newTxt]} {getLocals ()}"

            Logger.logTrace (fun () -> $"Profiling.addCount / {id}") getLocals

    let addTimestamp fn getLocals =
        if Dom.globalDebug.Get () then
            let newTicks = DateTime.ticksDiff initialTicks
            let id = fn ()
            let newTxt = $"{id} {getLocals ()}"

            profilingState.TimestampMap.Add (newTxt, newTicks)
            addCountMap newTxt

            let inline getLogFn (txt: string) =
                if txt.Contains "Ping " then Logger.logInfo else Logger.logTrace

            let logFn = getLogFn newTxt

            logFn (fun () -> $"Profiling.addTimestamp / {id}") getLocals

    addTimestamp (fun () -> $"{nameof FsJs} | Profiling body") getLocals


    let measureTimeN n name fn =
        Browser.Dom.console.time name

        for i in 0 .. n do
            fn ()

        Browser.Dom.console.timeEnd name

    let measureTime = measureTimeN 999999
