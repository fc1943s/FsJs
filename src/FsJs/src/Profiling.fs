namespace FsJs

open System
open System.Collections.Generic
open FsCore


module Profiling =
    let private initialTicks = DateTime.Now.Ticks

    let private profilingState =
        {|
            CountMap = Dictionary<string, int> ()
            TimestampMap = List<string * float> ()
        |}

    Dom.Global.set (nameof profilingState) profilingState

    Dom.Global.set
        "clearProfilingState"
        (fun () ->
            profilingState.CountMap.Clear ()
            profilingState.TimestampMap.Clear ())

    let addTimestamp id =
        if Dom.globalDebug.Get () then
            profilingState.TimestampMap.Add (id, DateTime.ticksDiff initialTicks)

    let removeCount id =
        if Dom.globalDebug.Get () then
            match profilingState.CountMap.ContainsKey id with
            | false -> profilingState.CountMap.[id] <- -1
            | true -> profilingState.CountMap.[id] <- profilingState.CountMap.[id] - 1

    let addCount id =
        if Dom.globalDebug.Get () then
            match profilingState.CountMap.ContainsKey id with
            | false -> profilingState.CountMap.[id] <- 1
            | true -> profilingState.CountMap.[id] <- profilingState.CountMap.[id] + 1

    addTimestamp "Profiling body"
