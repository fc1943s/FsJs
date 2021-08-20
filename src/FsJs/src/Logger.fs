namespace FsJs

open System
open FsCore
open Fable.Core.JsInterop
open Fable.Core

module Logger =
    let inline logWithFn logFn fn =
        match Dom.deviceTag |> List.ofSeq, fn () |> Seq.toList |> Option.ofObjUnbox with
        | a :: b :: c, Some result ->
            logFn [|
                ("%c%s"
                 |> String.replicate (2 + (result.Length - 1)))
                $"color: f{a}{c}f{b}"
                $"""[{Dom.deviceTag} {DateTime.Now |> DateTime.format "HH:mm:ss SSS"}]"""
                ""
                yield! result
            |]
        | _ -> ()

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

    type LogFn = (unit -> string) -> unit

    type Logger =
        {
            Trace: LogFn
            Debug: LogFn
            Info: LogFn
            Warning: LogFn
            Error: LogFn
        }

    let logIf currentLogLevel logLevel (fn: unit -> string) =
        if currentLogLevel <= logLevel then
            let result = fn ()

            if result |> Option.ofObjUnbox |> Option.isSome then
                log
                    (fun () ->
                        [|
                            $"""color: {match logLevel with
                                        | LogLevel.Trace -> "white"
                                        | LogLevel.Debug -> "green"
                                        | LogLevel.Info -> "cyan"
                                        | LogLevel.Warning -> "yellow"
                                        | LogLevel.Error -> "red"
                                        | LogLevel.Critical -> "magenta"
                                        | _ -> "white"}"""
                            $"[{Enum.name logLevel}]"
                            ""
                            result
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

    let inline logTrace fn = State.getLogger().Trace fn
    let inline logDebug fn = State.getLogger().Debug fn
    let inline logInfo fn = State.getLogger().Info fn
    let inline logWarning fn = State.getLogger().Warning fn
    let inline logError fn = State.getLogger().Error fn

    logInfo (fun () -> $"Logger. deviceInfo={JS.JSON.stringify Dom.deviceInfo}")
