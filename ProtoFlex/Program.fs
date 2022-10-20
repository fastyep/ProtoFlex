module ProtoFlex.CLI

open System.IO
open Argu
open Argu.ArguAttributes
open ProtoFlex.Generator

[<RequireQualifiedAccess>]
type CliArguments =
    | [<AltCommandLine "-i">] InputFile of path: string
    | [<AltCommandLine "-o"; Unique>] OutputFile of path: string
    | [<AltCommandLine "-n"; Unique>] Namespace of name: string
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | InputFile _ -> "path to proto files to build."
            | OutputFile _ -> "path where to write final FSharp types bundle."
            | Namespace _ -> "namespace that will be used in final bundle file."

[<Literal>]
let program_name = "pflex"

[<EntryPoint>]
let main args =
    let parser =
        ArgumentParser.Create<CliArguments> program_name

    try
        let result = parser.ParseCommandLine args

        let input =
            result.GetResults CliArguments.InputFile
            |> List.map (fun a -> Path.GetFileName a, File.OpenText a |> unbox)

        let name_space =
            result.TryGetResult CliArguments.Namespace

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
            printfn
                $"Output and at least one Input files should be specified! Use {program_name} --help to check list of available arguments."
    with
    | :? ArguParseException as e -> printfn $"%s{e.Message}"

    0
