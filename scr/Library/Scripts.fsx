//module Scripts

#load "Domain.fs"
#r @"C:\Users\menal\.nuget\packages\fsharp.data.sqlclient\2.0.2\lib\net40\FSharp.Data.SqlClient.dll"

open Domain
open FSharp.Data
open FSharp.Data.SqlClient
open System.Data.SqlClient

let [<Literal>] Conn = @"Data Source=(localdb)\MSSQLLocalDB;Database=Library;Integrated Security=True;Connect Timeout=60"
type LibraryDb = SqlProgrammabilityProvider<Conn>

type GetBooks = SqlCommandProvider<"SELECT Id, Title, ISBN, Description FROM dbo.Book", Conn>
type GetBooksByTitle = SqlCommandProvider<"SELECT Id, Title, ISBN, Description FROM dbo.Book WHERE Title = @title", Conn, SingleRow = true>

let books = GetBooks.Create(Conn).Execute();

books |> printfn "%A"

type GetAuthors = SqlCommandProvider<"SELECT Id, Name FROM dbo.Author", Conn>
type GetAuthorsByName = SqlCommandProvider<"SELECT Id, Name FROM dbo.Author WHERE Name = @Name", Conn, SingleRow = true>
let authors = GetAuthors.Create(Conn).Execute();

authors |> printfn "%A"


let (|PrimaryKeyConstraint|_|) (ex:exn) =
    match ex with
    | :? SqlException as ex when ex.Message.Contains "Violation of PRIMARY KEY constraint" -> Some PrimaryKeyConstraint
    | _ -> None

let addBook (book: Book) = 
    use b = new LibraryDb.dbo.Tables.Book()
    b.AddRow(book.Title,book.ISBN, book.Description)
    use connection = new SqlConnection(Conn)
    connection.Open()
    try b.Update(connection) |> ignore
    with
    | PrimaryKeyConstraint -> ()
    | _ -> reraise()

let b : Book = { 
    Title = "Get Programming with F#";  
    ISBN = Some("9781617293993");
    Description =Some("F# leads to quicker development time and a lower total cost of ownership. Its powerful feature set allows developers to more succinctly express their intent, and encourages best practices - leading to higher quality deliverables in less time. Programming with F#: A guide for .NET developers shows you how to upgrade your .NET development skills by adding a touch of functional programming in F#. In just 43 bite-size chunks, you'll learn to use F# to tackle the most common .NET programming tasks. You'll start with the basics of F# and functional programming, building on your existing skills in the .NET framework. Examples use the familiar Visual Studio environment, so you'll be instantly comfortable. Packed with enlightening examples, real-world use cases, and plenty of easy-to-digest code, this easy-to-follow tutorial will make you wonder why you didn't pick up F# years ago! Purchase of the print book includes a free eBook in PDF, Kindle, and ePub formats from Manning Publications.")
    }
addBook(b)

let addAuthor (author: Author) = 
    use a = new LibraryDb.dbo.Tables.Author()
    a.AddRow(author.Name)
    use connection = new SqlConnection(Conn)
    connection.Open()
    try a.Update(connection) |> ignore
    with
    | PrimaryKeyConstraint -> ()
    | _ -> reraise()

let a : Author = { Name = "Isaac Abraham" }
addAuthor(a)

let getAuthor name : ExistingAuthor option=
    match GetAuthorsByName.Create(Conn).Execute(name) with
    | Some author -> Some({Id =author.Id; Name = author.Name})
    | _ -> None

let auth = getAuthor "Isaac Abraham"

let getBook title : ExistingBook option=
    match GetBooksByTitle.Create(Conn).Execute(title) with
    | Some book -> Some({Id =book.Id; Title = book.Title; Description= book.Description; ISBN=book.ISBN})
    | _ -> None

let book1 = getBook "Get Programming with F#"

let addBookAuthor (bookId: int) (authorId: int) = 
    use ab = new LibraryDb.dbo.Tables.Author_Book()
    ab.AddRow(authorId, bookId)
    use connection = new SqlConnection(Conn)
    connection.Open()
    try ab.Update(connection) |> ignore
    with
    | PrimaryKeyConstraint -> ()
    | _ -> reraise()

addBookAuthor book1.Value.Id auth.Value.Id
