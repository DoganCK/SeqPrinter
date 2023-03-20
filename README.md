# About
This package provides a colored and tabularized output for sequences of named or anonymous records.

(Colors not visible in markdown.)

# Installation

## FSI
```fsharp
#r "nuget: SeqPrinter"
open SeqPrinter
```

## .NET CLI
```bash
dotnet add package SeqPrinter
```


# Usage

## Basic

```fsharp
type Person = { Name: string; Age: int; Height: int }

let typedList =
    [
        {
            Name = "Stanley"
            Age = 32
            Height = 175
        }
        {
            Name = "Akira"
            Age = 27
            Height = 175
        }
        {
            Name = "Andrei"
            Age = 35
            Height = 200
        }
    ]

typedList |> Printer.print
```

```output
 Age      Height   Name     
 <Int32>  <Int32>  <String> 
 32       175      Stanley  
 27       175      Akira    
 35       200      Andrei
 ```

## Selecting Columns

You can select which fields of your type to display as columns.

```fsharp
typedList
|> Printer
|> Printer.withColumns [ "Name"; "Height" ]
|> Printer.print
```

```
 Name      Height  
 <String>  <Int32> 
 Stanley   175     
 Akira     175     
 Andrei    200 
```

## Ordering columns
Similarly you can also determine the order of fields to display.

```fsharp
typedList
|> Printer
|> Printer.withColumns [ "Name"; "Age"; "Height" ]
|> Printer.print
```

```
 Name      Age      Height  
 <String>  <Int32>  <Int32> 
 Stanley   32       175     
 Akira     27       175     
 Andrei    35       200 
```

## Anonymous Records
```fsharp
open System

let anonRecords =
    [|
        for i in 1. .. 10. ->
            {|
                Number = i
                IsEven = i % 2. = 0.
                Log = Math.Log i
            |}
    |]


anonRecords
|> Printer
|> Printer.withColumns [ "Number"; "IsEven"; "Log" ]
|> Printer.print
```

```
 Number    IsEven     Log                
 <Double>  <Boolean>  <Double>           
 1         False      0                  
 2         True       0.6931471805599453 
 3         False      1.0986122886681098 
 4         True       1.3862943611198906 
 5         False      1.6094379124341003 
 6         True       1.791759469228055  
 7         False      1.9459101490553132 
 8         True       2.0794415416798357 
 9         False      2.1972245773362196 
 10        True       2.302585092994046  
```