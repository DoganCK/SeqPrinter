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
Displaying 1-3 of 3 items.
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
Displaying 1-3 of 3 items.
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
Displaying 1-3 of 3 items.
```

## Pagination

### Page Size
```fsharp
let printer = 
    typedList
    |> List.replicate 25
    |> List.concat
    |> Printer
    |> Printer.withPageSize 10
```

```
 Age      Height   Name     
 <Int32>  <Int32>  <String> 
 32       175      Stanley  
 27       175      Akira    
 35       200      Andrei   
 32       175      Stanley  
 27       175      Akira    
 35       200      Andrei   
 32       175      Stanley  
 27       175      Akira    
 35       200      Andrei   
 32       175      Stanley  
Displaying 1-10 of 75 items.
```

### Navigation
```fsharp
printer
|>Printer.nextPage

// Alternative

printer.NextPage()
```

```
 Age      Height   Name     
 <Int32>  <Int32>  <String> 
 27       175      Akira    
 35       200      Andrei   
 32       175      Stanley  
 27       175      Akira    
 35       200      Andrei   
 32       175      Stanley  
 27       175      Akira    
 35       200      Andrei   
 32       175      Stanley  
 27       175      Akira    
Displaying 11-20 of 75 items.
```