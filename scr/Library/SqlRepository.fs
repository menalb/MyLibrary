module internal SqlRepository

open Domain
open FSharp.Data
open System.Data.SqlClient

[<AutoOpen>]
module private DB =
    let [<Literal>] Conn = "Name=LibraryDb"
    type LibraryDb = SqlProgrammabilityProvider<Conn>
    type GetBooks = SqlCommandProvider<"SELECT Id, Title, ISBN, Description FROM dbo.Book", Conn>
    type GetBooksByTitle = SqlCommandProvider<"SELECT Id, Title, ISBN, Description FROM dbo.Book WHERE Title = @title", Conn, SingleRow = true>
    type GetAuthors = SqlCommandProvider<"SELECT Id, Name FROM dbo.Author", Conn>
    type GetAuthorsByName = SqlCommandProvider<"SELECT Id, Name FROM dbo.Author WHERE Name = @Name", Conn, SingleRow = true>

type private DbTables = DB.LibraryDb.dbo.Tables

let (|PrimaryKeyConstraint|_|) (ex:exn) =
    match ex with
    | :? SqlException as ex when ex.Message.Contains "Violation of PRIMARY KEY constraint" -> Some PrimaryKeyConstraint
    | _ -> None

let getBook (connection:string) title : ExistingBook option=
    match GetBooksByTitle.Create(Conn).Execute(title) with
    | Some book -> Some({Id =book.Id; Title = book.Title; Description= book.Description; ISBN=book.ISBN})
    | _ -> None

let getAuthor  (connection:string) name : ExistingAuthor option=
    match GetAuthorsByName.Create(Conn).Execute(name) with
    | Some author -> Some({Id =author.Id; Name = author.Name})
    | _ -> None

let addAuthor (connection:string) (author: Author) = 
    use a = new DbTables.Author()
    a.AddRow(author.Name)
    use connection = new SqlConnection(Conn)
    connection.Open()
    try a.Update(connection) |> ignore
    with
    | PrimaryKeyConstraint -> ()
    | _ -> reraise()

let addBook  (connection:string) (book: Book) = 
    use b = new DbTables.Book()
    b.AddRow(book.Title,book.ISBN, book.Description)
    use connection = new SqlConnection(Conn)
    connection.Open()
    try b.Update(connection) |> ignore
    with
    | PrimaryKeyConstraint -> ()
    | _ -> reraise()