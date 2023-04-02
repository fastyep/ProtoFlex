[<Microsoft.FSharp.Core.AutoOpen>]
module ProtoFlex.Extensions.Abstractions

open System.Collections
open System.Collections.Generic
open System.Text
open Google.Protobuf.Reflection

type ExtensionGenerator =
   | GenService of package: string * desc: ServiceDescriptorProto
   | GenMsgType of desc: DescriptorProto
   | GenEnumType of desc: EnumDescriptorProto

type ExtensionHook =  StringBuilder -> ExtensionGenerator -> unit