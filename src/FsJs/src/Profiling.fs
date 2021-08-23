namespace FsJs

open System
open System.Collections.Generic
open FsCore


module Profiling =
    let private initialTicks = DateTime.Now.Ticks


    let rec globalProfilingState =
        Dom.globalWrapper
            (nameof globalProfilingState)
            {|
                CountMap = Dictionary<string, int> ()
                TimestampMap = List<string * float> ()
            |}

    let rec globalClearProfilingState =
        Dom.globalWrapper
            (nameof globalClearProfilingState)
            (fun () ->
                let profilingState = globalProfilingState.Get ()

                Logger.logTrace
                    (fun () ->
                        $"Profiling.globalClearProfilingState
             CountMap.Count={profilingState.CountMap.Count}
             TimestampMap.Count={profilingState.TimestampMap.Count} ")

                profilingState.CountMap.Clear ()
                profilingState.TimestampMap.Clear ())

    let removeCount id =
        if Dom.globalDebug.Get () then
            let profilingState = globalProfilingState.Get ()

            match profilingState.CountMap.ContainsKey id with
            | false -> profilingState.CountMap.[id] <- -1
            | true -> profilingState.CountMap.[id] <- profilingState.CountMap.[id] - 1

            Logger.logTrace (fun () -> $"Profiling.removeCount [{id}] --{profilingState.CountMap.[id]}")

    let addCount id =
        if Dom.globalDebug.Get () then
            let profilingState = globalProfilingState.Get ()

            match profilingState.CountMap.ContainsKey id with
            | false -> profilingState.CountMap.[id] <- 1
            | true -> profilingState.CountMap.[id] <- profilingState.CountMap.[id] + 1

            Logger.logTrace (fun () -> $"Profiling.addCount [{id}] ++{profilingState.CountMap.[id]}")

    let addTimestamp id =
        if Dom.globalDebug.Get () then
            let profilingState = globalProfilingState.Get ()
            let newTicks = DateTime.ticksDiff initialTicks
            profilingState.TimestampMap.Add (id, newTicks)
            let newId = $"Profiling.addTimestamp [{id}] ticks={newTicks}"
            Logger.logTrace (fun () -> newId)
            addCount id

    addTimestamp "Profiling body"
