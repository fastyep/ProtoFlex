module ProtoFlex.Extensions.SqlProps

open System.Text

let hook (sb: StringBuilder) =
    function
    | GenMsgType desc ->
        sb.AppendLine $"[<RequireQualifiedAccess>]\nmodule {desc.Name} =" |> ignore
        
        for f in desc.Fields do
            sb.AppendLine $"    [<Literal>]\n    let {f.Name} = \"{f.Name}\"" |> ignore

        ()
    | _ -> ()
