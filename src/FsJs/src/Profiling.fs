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

    Dom.Global.set (nameof profilingState) profilingState

    let clearProfilingState () =
        Logger.logTrace
            (fun () ->
                $"Profiling.clearProfilingState
                                profilingState.CountMap.Count={profilingState.CountMap.Count} profilingState.TimestampMap.Count={profilingState.TimestampMap.Count} ")

        profilingState.CountMap.Clear ()
        profilingState.TimestampMap.Clear ()

    Dom.Global.set (nameof clearProfilingState) clearProfilingState

    let addTimestamp id =
        if Dom.globalDebug.Get () then
            let newTicks = DateTime.ticksDiff initialTicks
            profilingState.TimestampMap.Add (id, newTicks)
            Logger.logTrace (fun () -> $"Profiling.addTimestamp id={id} newTimestamp={newTicks}")

    let removeCount id =
        if Dom.globalDebug.Get () then
            match profilingState.CountMap.ContainsKey id with
            | false -> profilingState.CountMap.[id] <- -1
            | true -> profilingState.CountMap.[id] <- profilingState.CountMap.[id] - 1

            Logger.logTrace
                (fun () -> $"Profiling.removeCount id={id} profilingState.CountMap.[id]={profilingState.CountMap.[id]}")

    let addCount id =
        if Dom.globalDebug.Get () then
            match profilingState.CountMap.ContainsKey id with
            | false -> profilingState.CountMap.[id] <- 1
            | true -> profilingState.CountMap.[id] <- profilingState.CountMap.[id] + 1

            Logger.logTrace
                (fun () -> $"Profiling.addCount id={id} profilingState.CountMap.[id]={profilingState.CountMap.[id]}")

    addTimestamp "Profiling body"
