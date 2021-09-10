namespace FsJs.Tests

open Fable.Jester
open Fable.Core.JsInterop
open FsJs
open FsJs.Bindings


module Jest =
    Jest.test (
        "trace log",
        promise {
            emitJsExpr ("console.log", (emitJsExpr () "console.log")) Jest.jsHookFnBody

            let text = "Jest test"
            let getLocals () = "a=1"
            Logger.logTrace (fun () -> text) getLocals

            (Jest.expect ((emitJsExpr () "console.log.mock.calls[0]"): string list))
                .toEqual (
                    expect.arrayContaining [
                        "%c%s%c%s%c%s%c%s"
                        "color: #EEE"
                        "[Trace] "
                        "color: #AAA"
                        text
                        "color: #888"
                        $" [{getLocals ()}]"
                    ]
                )
        },
        Jest.maxTimeout
    )
