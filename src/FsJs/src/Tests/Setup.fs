namespace FsJs.Tests

open Fable.Jester
open Fable.ReactTestingLibrary
open Fable.Core.JsInterop
open FsJs
open FsJs.Dom
open Microsoft.FSharp.Core.Operators


module RTL =
    let inline sleep ms =
        promise {
            printfn $"RTL.sleep({ms})"

            for _ in 0 .. ms / 500 do
                do! RTL.waitFor (Promise.sleep 500)

            printfn $"RTL.sleep({ms}) end"
        }


module Setup =
    import "jest" "@jest/globals"

    [<Literal>]
    let jsHookFnBody =
        "
window.hookCache=window.hookCache||{};
window.hookCache[$0] = $1;
$1 = jest.fn()"

    let maxTimeout = 5 * 60 * 1000

    let inline handlePromise promise =
        promise
        |> Promise.catch (fun ex -> Logger.logError (fun () -> $"Setup.handlePromise. ex={ex}"))

    Jest.afterAll (
        promise {
            Logger.logTrace (fun () -> "Setup body. Jest.afterAll")
            Global.set "exit" true
        }
    )