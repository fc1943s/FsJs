namespace FsJs

open System
open FsCore
open FsJs
open Fable.Core.JsInterop
open Fable.Core

module Logger =
    let inline logWithFn logFn fn =
        let result = fn ()

        match Dom.deviceTag |> List.ofSeq, result |> Seq.toList |> Option.ofObjUnbox with
        | a :: b :: c :: d :: _, Some result ->
            let format = "HH:mm:ss.SSS"

            logFn [|
                ("%c%s"
                 |> String.replicate (1 + int (Math.Ceiling (float result.Length / 2.))))
                $"color: #f{a}{b}{c}f{d}"
                $"[{Dom.deviceTag} {DateTime.Now |> DateTime.format format}] "
                yield! result
            |]
        | _ -> eprintfn $"Logger.logWithFn. invalid log. Dom.deviceTag={Dom.deviceTag} result={result}"

    let inline consoleLog (x: _ []) = emitJsExpr x "console.log(...$0)"
    let inline consoleError (x: _ []) = emitJsExpr x "console.error(...$0)"
    let inline log (fn: unit -> _ []) = logWithFn consoleLog fn
    let inline elog (fn: unit -> _ []) = logWithFn consoleError fn


    type LogLevel =
        | Trace = 0
        | Debug = 1
        | Info = 2
        | Warning = 3
        | Error = 4
        | Critical = 5

    let DEFAULT_LOG_LEVEL = if Dom.globalDebug.Get () then LogLevel.Trace else LogLevel.Info

    type LogFn = (unit -> string) -> (unit -> string) -> unit

    type Logger =
        {
            Trace: LogFn
            Debug: LogFn
            Info: LogFn
            Warning: LogFn
            Error: LogFn
        }

    let inline logIf currentLogLevel logLevel (fn: unit -> string) getLocals =
        if currentLogLevel <= logLevel then
            let result = fn ()

            if result |> Option.ofObjUnbox |> Option.isSome then
                log
                    (fun () ->
                        [|
                            $"""color: {match logLevel with
                                        | LogLevel.Trace -> "#EEE"
                                        | LogLevel.Debug -> "green"
                                        | LogLevel.Info -> "cyan"
                                        | LogLevel.Warning -> "yellow"
                                        | LogLevel.Error -> "red"
                                        | LogLevel.Critical -> "magenta"
                                        | _ -> "white"}"""
                            $"[{Enum.name logLevel}] "
                            "color: #AAA"
                            result
                            "color: #888"
                            (getLocals ())
                        |])

    type Logger with

        static member inline Create currentLogLevel =
            let log = logIf currentLogLevel

            {
                Trace = log LogLevel.Trace
                Debug = log LogLevel.Debug
                Info = log LogLevel.Info
                Warning = log LogLevel.Warning
                Error = log LogLevel.Error
            }

        static member inline Default = Logger.Create DEFAULT_LOG_LEVEL


    module State =
        let mutable lastLogger = Logger.Default
        let inline getLogger () = lastLogger

    let inline logTrace fn getLocals =
        if Dom.globalDebug.Get () then State.getLogger().Trace fn getLocals

    let inline logDebug fn getLocals =
        if Dom.globalDebug.Get () then State.getLogger().Debug fn getLocals

    let inline logInfo fn getLocals = State.getLogger().Info fn getLocals
    let inline logWarning fn getLocals = State.getLogger().Warning fn getLocals
    let inline logError fn getLocals = State.getLogger().Error fn getLocals

    let getLocals () =
        $"deviceInfo={JS.JSON.stringify Dom.deviceInfo} {getLocals ()}"

    logInfo (fun () -> "Logger body") getLocals
