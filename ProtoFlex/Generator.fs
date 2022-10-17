﻿module ProtoFlex.Generator

open System.IO
open System.Text
open Google.Protobuf.Reflection

[<Literal>]
let private disclaimer =
    "//------------------------------------------------------------------------------
//     This code was generated by the ProtoFlex tool.
//     Changes to this file will be lost when code is regenerated.
//------------------------------------------------------------------------------"

[<Literal>]
let private imports =
    "open System
open System.Collections.Generic
open System.Runtime.Serialization
open System.ServiceModel
open System.Threading.Tasks
"

[<Literal>]
let private space = "    "

let private split (a: string) = a.Split '.' |> Seq.last

let rec genType (f: FieldDescriptorProto) =
    match f.``type`` with
    | FieldDescriptorProto.Type.TypeDouble -> nameof float
    | FieldDescriptorProto.Type.TypeFloat -> nameof float32
    | FieldDescriptorProto.Type.TypeInt64 -> nameof int64
    | FieldDescriptorProto.Type.TypeUint64 -> nameof uint64
    | FieldDescriptorProto.Type.TypeInt32 -> nameof int
    | FieldDescriptorProto.Type.TypeFixed64 -> nameof double
    | FieldDescriptorProto.Type.TypeFixed32 -> nameof double
    | FieldDescriptorProto.Type.TypeBool -> nameof bool
    | FieldDescriptorProto.Type.TypeString -> nameof string
    | FieldDescriptorProto.Type.TypeGroup -> nameof double
    | FieldDescriptorProto.Type.TypeMessage -> split f.TypeName
    | FieldDescriptorProto.Type.TypeBytes -> "byte []"
    | FieldDescriptorProto.Type.TypeUint32 -> nameof uint32
    | FieldDescriptorProto.Type.TypeEnum -> nameof double
    | FieldDescriptorProto.Type.TypeSfixed32 -> nameof double
    | FieldDescriptorProto.Type.TypeSfixed64 -> nameof double
    | FieldDescriptorProto.Type.TypeSint32 -> nameof double
    | _ -> nameof obj

let genMsgType (msg: DescriptorProto) =
    let sb =
        StringBuilder()
            .AppendLine("[<DataContract; CLIMutable>]")
            .AppendLine($"type {msg.Name} =")

    msg.Fields
    |> Seq.iteri (fun i f ->
        if i = 0 then
            sb.Append $"{space}{{ "
        else
            sb.Append $"{space}  "
        |> ignore

        sb
            .AppendLine($"[<DataMember(Order = {f.Number})>]")
            .Append($"{space}  {f.Name}: {genType f}")
        |> ignore

        if i = msg.Fields.Count - 1 then
            sb.AppendLine " }"
        else
            sb.AppendLine()
        |> ignore)

    string sb

let genService (srv: ServiceDescriptorProto) =
    let sb =
        StringBuilder()
            .AppendLine($"[<ServiceContract(Name = \"{srv.Name}\")>]")
            .AppendLine($"type I{srv.Name} =")

    for rpc in srv.Methods do
        sb.AppendLine $"{space}abstract member {rpc.Name}: {split rpc.InputType} -> Task<{split rpc.OutputType}>"
        |> ignore

    string sb

let genTemplate (name_space: string option) (protos: list<string * TextReader>) =
    let set = FileDescriptorSet()

    for n, r in protos do
        set.Add(n, source = r) |> ignore

    set.Process()

    match set.GetErrors() with
    | errors when (errors.Length > 0) -> Error errors
    | _ ->
        let name_space =
            defaultArg name_space set.Files[0].Package

        let sb =
            StringBuilder()
                .AppendLine(disclaimer)
                .AppendLine($"namespace {name_space}\n")
                .AppendLine(imports)

        for file in set.Files do
            for msg in file.MessageTypes do
                genMsgType msg |> sb.AppendLine |> ignore

            for srv in file.Services do
                genService srv |> sb.Append |> ignore

        Ok(string sb)
