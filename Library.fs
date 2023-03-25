namespace SeqPrinter

open System

module internal Console =
    let private log (color: ConsoleColor) (s: string) =
        Console.ForegroundColor <- color
        printf "%s" s
        Console.ResetColor()

    let datetime = log ConsoleColor.Blue
    let number = log ConsoleColor.Green
    let bool = log ConsoleColor.White
    let string = log ConsoleColor.Yellow
    let typeName = log ConsoleColor.Cyan
    let generic s  = printf $"{s}"

[<AutoOpen>]
module internal Printable =
    open System.Reflection

    type ColumnName =
        ColumnName of string
        with member c.stringify () =
                match c with
                | ColumnName s -> s

    type Column =
        {
            Type: string
            Values: obj array
            Width: int
        }

    type Printable = Map<ColumnName, Column>

    let private getColumns (s: seq<'T>): PropertyInfo[] =
        s
        |> Seq.tryHead
        |> fun op ->
            match op with
            | Some v -> v.GetType().GetProperties()
            | None -> [||]

    let private handleColumn (s: seq<'T>) (col: PropertyInfo): ColumnName * Column =
        let values =    
            s
            |> Seq.map (fun row -> col.GetValue(row))
            |> Array.ofSeq

        let maxValueWidth =
            values
            |> Array.maxBy (fun v -> v.ToString().Length)
            |> fun x -> x.ToString().Length

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

    let getPrintable (pageSize: int) (page: int) (s: seq<'T>): Printable =
        let range = 
            s
            |> Seq.skip ((page - 1) * pageSize)
            |> Seq.truncate pageSize
        range
        |> getColumns
        |> Array.map (handleColumn range)
        |> Map

type Printer<'T>(s: seq<'T>) =
    let length = s |> Seq.length

    let mutable pageSize: int = 50

    let mutable page = 1

    let mutable lastPage = int (Math.Ceiling (float length / float pageSize))

    let mutable printable = getPrintable pageSize page s

    let mutable columns: ColumnName[] =
        printable.Keys |> Array.ofSeq

    member private _.Printable
        with  get () = printable
        and set (value: Printable) = printable <- value

    member private p.recalculatePrintable () =
        p.Printable <- getPrintable pageSize page s

    member private _.Length = length

    member private _.Columns
        with get () = columns
        and set (value: ColumnName []) = columns <- value

    member private p.PageSize
        with get () = pageSize
        and set (value: int) =
            pageSize <- value
            lastPage <- int (Math.Ceiling (float length / float pageSize))
            p.recalculatePrintable ()

    member private p.Page
        with get () = page
        and set (value: int) =
            page <- value
            p.recalculatePrintable ()

    member private _.LastPage
        with get () = lastPage
        and set (value: int) =
            lastPage <- value

    member private p.padR (col: ColumnName) (v: string): int =
        p.Printable[col].Width - v.Length - 1

    member private p.printFields =
        // Print Field Names
        p.Columns
        |> Array.iter (fun x ->
            let cellStr = String(' ', 1) + x.stringify() + String(' ', p.padR x (x.stringify()))
            printf $"{cellStr}"
        )
        printfn ""

        // Print Field Types
        p.Columns
        |> Array.iter (fun x ->
            let typeName = p.Printable[x].Type
            let strBracketed = "<" + typeName + ">"
            printf " <"
            sprintf $"{typeName}" |> Console.typeName
            printf ">"
            printf $"{String(' ', p.padR x strBracketed)}"
        )
        printfn ""

    member private p.printCell (rowIx: int) (col: ColumnName) =
        let v = p.Printable[col].Values[rowIx]
        let padR = p.padR col (v.ToString())

        printf $"{String(' ', 1).ToString()}"
        p.printValue v
        printf $"{String(' ', padR).ToString()}"

    member private _.printValue(v: obj) =
        let str = string v

        match v with
        | :? string -> Console.string str
        | :? int8
        | :? int16
        | :? int32
        | :? int64
        | :? uint8
        | :? uint16
        | :? uint32
        | :? uint64
        | :? float
        | :? single -> Console.number str
        | :? bool -> Console.bool str
        | :? DateTime
        | :? DateOnly
        | :? TimeSpan -> Console.datetime str
        | _ -> Console.generic str

    member private p.printRow(rowIx: int) =
        p.Columns |> Array.iter (p.printCell rowIx)
        printfn ""

    member private p.printValues =
        [|0 .. (p.Printable[p.Columns[0]].Values|>Array.length) - 1|]
        |> Array.iteri (fun i _ -> p.printRow i)

    member private p.printInfo =
        let start = (p.Page - 1) * pageSize + 1
        let terminus = Math.Min(p.Page * pageSize, p.Length)
        printfn $"Displaying {start}-{terminus} of {p.Length} items."

    /// Prints the first page.
    static member print(p: Printer<'T>) : Printer<'T> =
        p.printFields
        p.printValues
        p.printInfo
        p

    /// Prints the first page.
    static member print(s: seq<'T>) =
        Printer s
        |> Printer.print

    /// Determines which fields to diaplya and the order thereof.
    static member withColumns (cols: string seq) (p: Printer<'T>): Printer<'T> =
        let colAr =
            cols
            |> Seq.map ColumnName
            |> Array.ofSeq

        let diff = set colAr - set p.Columns

        if diff.Count > 0 then
            failwith $"The following fields are not present in the array provided: {diff}"
        else
            p.Columns <- colAr
            p

    /// Sets how many items to display per page.
    static member withPageSize (pageSize: int) (p: Printer<'T>): Printer<'T> =
            p.PageSize <- pageSize
            p

    /// Navigates to the next page if it exists.
    /// If not, it displays the last page.
    static member nextPage (p: Printer<'T>) =
        p.Page <- Math.Min(p.Page + 1, p.LastPage)
        p
        |>Printer.print

    member p.NextPage () =
        p
        |> Printer.nextPage

    /// Navigates to the previous page if it exists.
    /// If not, it displays the first page.
    static member previousPage (p: Printer<'T>) =
        p.Page <- Math.Max (p.Page - 1, 1)
        p
        |>Printer.print

    member p.PreviousPage () =
        p
        |> Printer.previousPage