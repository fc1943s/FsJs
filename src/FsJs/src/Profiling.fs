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
                Logger.logTrace
                    (fun () ->
                        $"Profiling.globalClearProfilingState
             CountMap.Count={profilingState.CountMap.Count}
             TimestampMap.Count={profilingState.TimestampMap.Count} ")

                profilingState.CountMap.Clear ()
                profilingState.TimestampMap.Clear ())

    let removeCount id =
        if Dom.globalDebug.Get () then
            match profilingState.CountMap.ContainsKey id with
            | false -> profilingState.CountMap.[id] <- -1
            | true -> profilingState.CountMap.[id] <- profilingState.CountMap.[id] - 1

            Logger.logTrace (fun () -> $"Profiling.removeCount [{id}] --{profilingState.CountMap.[id]}")

    let addCount id =
        if Dom.globalDebug.Get () then
            match profilingState.CountMap.ContainsKey id with
            | false -> profilingState.CountMap.[id] <- 1
            | true -> profilingState.CountMap.[id] <- profilingState.CountMap.[id] + 1

            Logger.logTrace (fun () -> $"Profiling.addCount [{id}] ++{profilingState.CountMap.[id]}")

    let addTimestamp id =
        if Dom.globalDebug.Get () then
            let newTicks = DateTime.ticksDiff initialTicks
            profilingState.TimestampMap.Add (id, newTicks)
            let newId = $"Profiling.addTimestamp [{id}] ticks={newTicks}"
            Logger.logTrace (fun () -> newId)
            addCount newId

    addTimestamp $"{nameof FsJs} | Profiling body"
