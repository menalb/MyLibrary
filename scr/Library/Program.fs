// Learn more about F# at http://fsharp.org

open Library.SqlRepository

[<EntryPoint>]
let main argv =
    getBook "Data Source=(localdb)\MSSQLLocalDB;Database=Library;Integrated Security=True;Connect Timeout=30" "Get Programming with F#"
    |> printfn "%A"
    0 // return an integer exit code
