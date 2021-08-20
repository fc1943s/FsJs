namespace FsJs.Tests

open Fable.Jester
open Fable.Core.JsInterop
open FsJs


module Jest =
    Jest.test (
        "trace log",
        promise {
            emitJsExpr ("console.log", (emitJsExpr () "console.log")) Setup.jsHookFnBody

            let text = "Jest test"
            Logger.logTrace (fun () -> text)

            (Jest.expect ((emitJsExpr () "console.log.mock.calls[0]"): string list))
                .toEqual (
                    expect.arrayContaining [
                        "%c%s%c%s%c%s"
                        "color: white"
                        "[Trace] "
                        ""
                        text
                    ]
                )
        },
        Setup.maxTimeout
    )
