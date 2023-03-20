namespace SeqPrinter

open System

module internal Console =
    let private log =
        fun color s ->
            Console.ForegroundColor <- color
            printf "%s" s
            Console.ResetColor()

    let datetime = log ConsoleColor.Blue
    let number = log ConsoleColor.Green
    let bool = log ConsoleColor.White
    let string = log ConsoleColor.Yellow
    let typeName = log ConsoleColor.Cyan

[<AutoOpen>]
module internal Printable =
    open System.Reflection

    type ColumnName =
        ColumnName of string

    type Column =
        {
            Type: string
            Values: obj array
            Width: int
        }

    type Printable = Map<ColumnName, Column>

    let private getColumns (s: seq<'T>) =
        s
        |> Seq.tryHead
        |> fun op ->
            match op with
            | Some v -> v.GetType().GetProperties()
            | None -> [||]

    let private handleColumn (s: seq<'T>) (col: PropertyInfo) =
        let values =    
            s
            |> Seq.map (fun row -> col.GetValue(row))
            |> Array.ofSeq

        let maxValueWidth =
            values
            |> Array.map (fun v -> v.ToString().Length)
            |> Array.max

        let typeName = col.PropertyType.Name

        let width =
            [| maxValueWidth; col.Name.ToString().Length; typeName.Length + 2 |]
            |> Array.max
            |> fun x -> x + 2 // Add padding on both sides

        ColumnName col.Name, {
                                Values = values
                                Type = col.PropertyType.Name
                                Width = width
                            }

    let getPrintable (s: seq<'T>): Printable =
        s
        |> getColumns
        |> Array.map (handleColumn s)
        |> Map

type Printer<'T>(s: seq<'T>) =
    let printable = getPrintable s

    let mutable columns: ColumnName[] =
        printable.Keys |> Array.ofSeq

    member internal _.Printable = printable

    member _.Columns
        with internal get () = columns
        and internal set (value) = columns <- value

    member private p.padR (col: ColumnName) (v: string) =
        p.Printable[col].Width - v.Length - 1

    member p.printFields =
        // Print Field Names
        p.Columns
        |> Array.iter (fun x ->
            let colNameStr =
                match x with
                | ColumnName s -> s

            let strr = String(' ', 1) + colNameStr + String(' ', p.padR x colNameStr)
            printf $"{strr}")

        printfn ""

        // Print Field Types
        p.Columns
        |> Array.iter (fun x ->
            let typeName = p.Printable[x].Type
            let strBracketed = "<" + typeName + ">"
            printf " <"
            sprintf $"{typeName}" |> Console.typeName
            printf ">"
            printf $"{String(' ', p.padR x strBracketed)}")

        printfn ""

        p

    member private p.printCell (rowIx: int) (col: ColumnName) =
        let v = p.Printable[col].Values[rowIx]
        let padR = p.padR col (v.ToString())

        printf $"{String(' ', 1).ToString()}"
        p.printValue v
        printf $"{String(' ', padR).ToString()}"

    member private p.printValue(v: obj) =
        let strr = string v

        match v with
        | :? string -> Console.string strr
        | :? int8
        | :? int16
        | :? int32
        | :? int64
        | :? uint8
        | :? uint16
        | :? uint32
        | :? uint64
        | :? float
        | :? single -> Console.number strr
        | :? bool -> Console.bool strr
        | :? DateTime
        | :? TimeSpan -> Console.datetime strr
        | _ -> printf $"{strr}"

    member private p.printRow(rowIx: int) =
        p.Columns |> Array.iter (p.printCell rowIx)
        printfn ""

    member private p.printValues =
        p.Printable[p.Columns[0]].Values |> Array.iteri (fun i _ -> p.printRow i)
        p

    static member print(p: Printer<'T>) : unit =
        p.printFields |> ignore
        p.printValues |> ignore

    static member print(s: seq<'T>) = Printer s |> Printer.print

    static member withColumns (cols: string seq) (p: Printer<'T>) =
        let colAr =
            cols
            |> Seq.map ColumnName
            |> Array.ofSeq

        let diff = set colAr - set p.Columns

        if diff.Count <> 0 then
            failwith $"The following fields are not present in the array provided: {diff}"
        else
            p.Columns <- colAr
            p
