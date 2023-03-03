module ProtoFlex.CLI

open System
open System.IO
open System.Reflection
open Argu
open ProtoFlex.Generator

[<Literal>]
let program_name = "pflex"

[<RequireQualifiedAccess>]
type CliArguments =
    | [<AltCommandLine "-i">] InputFile of path: string
    | [<AltCommandLine "-o"; Unique>] OutputFile of path: string
    | [<AltCommandLine "-n"; Unique>] Namespace of name: string
    | [<AltCommandLine "-v"; Unique>] Version

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | InputFile _ -> "path to proto files to build."
            | OutputFile _ -> "path where to write final FSharp types bundle."
            | Namespace _ -> "namespace that will be used in final bundle file."
            | Version -> $"view {program_name} current version."

[<EntryPoint>]
let main args =
    let parser = ArgumentParser.Create<CliArguments> program_name

    try
        let result = parser.ParseCommandLine args

        let input =
            result.GetResults CliArguments.InputFile
            |> List.map (fun a -> Path.GetFileName a, File.OpenText a |> unbox)

        let name_space = result.TryGetResult CliArguments.Namespace

        match result.TryGetResult CliArguments.OutputFile with
        | Some output when (input.Length > 0) ->
            match genTemplate name_space input with
            | Ok temp ->
                File.WriteAllText(output, temp)
                printfn $"File {Path.GetFileName output} has been successfully generated."
            | Error err ->
                printfn "Errors during parsing proto file: "
                err |> Seq.iter (printfn "\t%A")
        | _ ->
            match result.TryGetResult CliArguments.Version with
            | Some _ ->
                Assembly.GetAssembly(typeof<CliArguments>).GetName().Version.ToString()
                |> printfn "%s"
            | None ->
                printfn
                    $"Output and at least one Input files should be specified! Use {program_name} --help to check list of available arguments."
    with :? ArguParseException as e ->
        printfn $"%s{e.Message}"

    0
