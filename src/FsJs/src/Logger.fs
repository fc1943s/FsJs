namespace FsJs

open System
open FsCore
open Fable.Core.JsInterop
open Fable.Core


module Logger =
    module ConsoleFlag =
        let reset = "\x1b[0m"
        let bright = "\x1b[1m"
        let dim = "\x1b[2m"
        let underscore = "\x1b[4m"
        let blink = "\x1b[5m"
        let reverse = "\x1b[7m"
        let hidden = "\x1b[8m"

        let fgBlack = "\x1b[30m"
        let fgRed = "\x1b[31m"
        let fgGreen = "\x1b[32m"
        let fgYellow = "\x1b[33m"
        let fgBlue = "\x1b[34m"
        let fgMagenta = "\x1b[35m"
        let fgCyan = "\x1b[36m"
        let fgWhite = "\x1b[37m"

        let bgBlack = "\x1b[40m"
        let bgRed = "\x1b[41m"
        let bgGreen = "\x1b[42m"
        let bgYellow = "\x1b[43m"
        let bgBlue = "\x1b[44m"
        let bgMagenta = "\x1b[45m"
        let bgCyan = "\x1b[46m"
        let bgWhite = "\x1b[47m"

        let fg =
            [
                fgBlack
                fgRed
                fgGreen
                fgYellow
                fgBlue
                fgMagenta
                fgCyan
                fgWhite
            ]

    let inline consoleLog (x: _ []) = emitJsExpr x "console.log(...$0)"
    let inline consoleError x = Browser.Dom.console.error x

    let inline logWithFn logFn fn =
        let result = fn ()

        if result |> Option.ofObjUnbox |> Option.isSome then

            let output =
                [|
                    let tagValue =
                        Dom.deviceTag
                        |> Seq.map Char.getNumericValue
                        |> Seq.map Math.Abs
                        |> Seq.map float
                        |> Seq.sum

                    let tagIndex = ((tagValue / 60.) * 5.) - 5. |> int

                    //                    printfn $"tagValue={tagValue} tagIndex={tagIndex}"

                    ConsoleFlag.fg.[Math.Min (ConsoleFlag.fg.Length - 1, Math.Max (0, tagIndex))]
                    $"""[{Dom.deviceTag} {DateTime.Now |> DateTime.format "HH:mm:ss"}]"""
                    ConsoleFlag.reset
                    yield! result
                |]
                |> String.concat " "

            logFn output

    let inline log fn = logWithFn (fun x -> printfn $"{x}") fn

    let inline logFiltered newValue fn =
        log
            (fun () ->
                if (string newValue).StartsWith "Ping " then
                    null
                else
                    let result: string = fn ()

                    if result.Contains "devicePing" then
                        null
                    else
                        [|
                            result
                        |])

    let inline logArray (fn: unit -> _ []) = logWithFn (fun x -> printfn $"{x}") fn
    let inline elog fn = logWithFn (fun x -> eprintfn $"{x}") fn


    type LogLevel =
        | Trace = 0
        | Debug = 1
        | Info = 2
        | Warning = 3
        | Error = 4
        | Critical = 5

    let DEFAULT_LOG_LEVEL = if Dom.isDebug () then LogLevel.Debug else LogLevel.Info

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
                logArray
                    (fun () ->
                        [|
                            match logLevel with
                            | LogLevel.Trace -> ConsoleFlag.fgBlack
                            | LogLevel.Debug -> ConsoleFlag.fgGreen
                            | LogLevel.Info -> ConsoleFlag.fgWhite
                            | LogLevel.Warning -> ConsoleFlag.fgYellow
                            | LogLevel.Error -> ConsoleFlag.fgRed
                            | LogLevel.Critical -> ConsoleFlag.fgMagenta
                            | _ -> ConsoleFlag.fgWhite
                            $"[{Enum.name logLevel}]"
                            ConsoleFlag.fgWhite
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
