namespace FsJs

open System
open Browser.Types
open Fable.Extras
open FsCore
open System.Collections.Generic
open Fable.Core.JsInterop
open FsCore.BaseModel


module Dom =
    type DeviceInfo =
        {
            Brands: (string * string) []
            IsMobile: bool
            IsElectron: bool
            IsExtension: bool
            GitHubPages: bool
            IsTesting: bool
            DeviceId: DeviceId
        }
        static member inline Default =
            {
                Brands = [||]
                IsMobile = false
                IsElectron = false
                IsExtension = false
                GitHubPages = false
                IsTesting = false
                DeviceId = DeviceId Guid.Empty
            }

    let inline window () =
        if jsTypeof Browser.Dom.window <> "undefined" then
            Some Browser.Dom.window
        else
            printfn "No window found"
            None

    let deviceInfo =
        match window () with
        | None ->
            printfn "deviceInfo: no window found"
            DeviceInfo.Default
        | Some window ->
            let userAgentData =
                window?navigator
                |> Option.ofObjUnbox
                |> Option.bind
                    (fun navigator ->
                        navigator?userAgentData
                        |> Option.ofObjUnbox<{| mobile: bool
                                                brands: {| brand: string; version: string |} [] |}>)

            let brands =
                userAgentData
                |> Option.map
                    (fun userAgentData ->
                        userAgentData.brands
                        |> Array.map (fun brand -> brand.brand, brand.version))
                |> Option.defaultValue [||]

            let userAgentDataMobile =
                userAgentData
                |> Option.map (fun userAgentData -> userAgentData.mobile)
                |> Option.defaultValue false

            let isTesting = Js.jestWorkerId || window?Cypress <> null

            let deviceId =
                match window.localStorage.getItem "deviceId" with
                | String.Valid deviceId -> DeviceId (Guid deviceId)
                | _ ->
                    let deviceId = DeviceId.NewId ()
                    window.localStorage.setItem ("deviceId", deviceId |> DeviceId.Value |> string)
                    deviceId

            {
                Brands = brands
                IsMobile =
                    if userAgentDataMobile then
                        true
                    elif brands.Length > 0 then
                        false
                    else
                        let userAgent = if window?navigator = None then "" else window?navigator?userAgent

                        JSe
                            .RegExp(
                                "Android|BlackBerry|iPhone|iPad|iPod|Opera Mini|IEMobile|WPDesktop",
                                JSe.RegExpFlag().i
                            )
                            .Test userAgent
                IsElectron = jsTypeof window?electronApi = "object"
                IsExtension = window.location.protocol = "chrome-extension:"
                GitHubPages = window.location.host.EndsWith "github.io"
                IsTesting = isTesting
                DeviceId = deviceId
            }

    let isDebugStatic =
        not deviceInfo.GitHubPages
        && not deviceInfo.IsExtension
        && not deviceInfo.IsElectron
        && not deviceInfo.IsMobile

    module Global =
        let private globalMap = Dictionary<string, obj> ()

        match window () with
        | Some window when isDebugStatic -> window?_globalMap <- globalMap
        | _ -> ()

        let internalGet<'T> (key: string) (defaultValue: 'T) =
            match globalMap.TryGetValue key with
            | true, value -> value |> unbox<'T>
            | _ -> defaultValue

        let internalSet key value = globalMap.[key] <- value

        let inline register<'T> key (defaultValue: 'T) =
            let result =
                {|
                    Key = key
                    Get = fun () -> internalGet key defaultValue
                    Set = fun (value: 'T) -> internalSet key value
                |}


            result.Set defaultValue
            result

    let rec globalDebug = Global.register (nameof globalDebug) isDebugStatic

    let deviceTag =
        deviceInfo.DeviceId
        |> DeviceId.Value
        |> string
        |> String.substringFrom -4

    let rec globalExit = Global.register (nameof globalExit) false

    let rec waitFor fn =
        async {
            if globalExit.Get () then
                return (unbox null)
            else
                let ok = fn ()

                if ok then
                    return ()
                else
                    printfn "waitFor: false. waiting..."

                    do! Js.sleep 100
                    return! waitFor fn
        }

    let rec waitForObject fn =
        async {
            if globalExit.Get () then
                return (unbox null)
            else
                let! obj = fn ()

                match box obj with
                | null ->
                    printfn "waitForObject: null. waiting..."

                    do! Js.sleep 100
                    return! waitForObject fn
                | _ -> return obj
        }

    let rec waitForSome fn =
        async {
            if globalExit.Get () then
                return (unbox null)
            else
                let! obj = fn ()

                match obj with
                | Some obj -> return obj
                | None ->
                    if deviceInfo.IsTesting then
                        do! Js.sleep 0
                    else
                        printfn $"waitForSome: none. waiting... {fn.ToString ()}"

                        do! Js.sleep 100

                    return! waitForSome fn
        }

    let inline download content fileName contentType =
        let a = Browser.Dom.document.createElement "a"

        let file =
            Browser.Blob.Blob.Create (
                [|
                    content
                |],
                { new BlobPropertyBag with
                    member _.``type`` = contentType
                    member _.endings = BlobEndings.Transparent

                    member _.``type``
                        with set value = ()

                    member _.endings
                        with set value = ()
                }
            )

        a?href <- Browser.Url.URL.createObjectURL file
        a?download <- fileName
        a.click ()
        a.remove ()
