namespace FsJs

open FsCore
open System
open Fable.DateFunctions
open Fable.Core.JsInterop


module Char =
    let inline getNumericValue (char: char) =
        (((string char).ToLower ())?charCodeAt 0) - 97 + 1


module DateTime =
    let inline format format (dateTime: DateTime) = dateTime.Format format

    let inline addDays (days: int) (dateTime: DateTime) = dateTime.AddDays days


module Enum =
    let inline formatIfEnum<'T> (value: 'T) =
        if typeof<'T>.IsEnum then value |> Enum.name |> unbox else value


module Object =
    let inline invokeOrReturnParam param argFn =
        match jsTypeof argFn with
        | "function" -> (argFn |> box |> unbox) param |> unbox
        | _ -> argFn

    let inline invokeOrReturn argFn = invokeOrReturnParam () argFn
