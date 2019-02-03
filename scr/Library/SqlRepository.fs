module internal Library.SqlRepository

open Domain
open FSharp.Data
open System.Data.SqlClient
open System.Drawing

[<AutoOpen>]
module private DB =
    //let [<Literal>] Conn = "Name=LibraryDb"
    let [<Literal>] Conn = @"Data Source=(localdb)\MSSQLLocalDB;Database=Library;Integrated Security=True;Connect Timeout=60"
    type LibraryDb = SqlProgrammabilityProvider<Conn>
    type GetBooks = SqlCommandProvider<"SELECT Id, Title, ISBN, Description FROM dbo.Book", Conn>
    type GetBooksByTitle = SqlCommandProvider<"SELECT b.Id, b.Title, b.[Description], b.ISBN, a.[Name] FROM Book b INNER JOIN Author_Book ab ON b.Id = ab.BookId INNER JOIN Author a ON a.Id = ab.AuthorId WHERE Title = @title", Conn, SingleRow = true>
    type GetAuthors = SqlCommandProvider<"SELECT Id, Name FROM dbo.Author", Conn>
    type GetAuthorsByName = SqlCommandProvider<"SELECT Id, Name FROM dbo.Author WHERE Name = @Name", Conn, SingleRow = true>

type private DbTables = DB.LibraryDb.dbo.Tables

let (|PrimaryKeyConstraint|_|) (ex:exn) =
    match ex with
    | :? SqlException as ex when ex.Message.Contains "Violation of PRIMARY KEY constraint" -> Some PrimaryKeyConstraint
    | _ -> None

let getBook (connectionString:string) (title : string) : ExistingBook option=
    match GetBooksByTitle.Create(connectionString).Execute(title) with
    | Some book -> Some({Id =book.Id; Title = book.Title; Description= book.Description; ISBN=book.ISBN})
    | _ -> None

let getAuthor  (connectionString:string) (name : string) : ExistingAuthor option=
    match GetAuthorsByName.Create(connectionString).Execute(name) with
    | Some author -> Some({Id =author.Id; Name = author.Name})
    | _ -> None

let addAuthor (connectionString:string) (author: Author) = 
    use authorDb = new DbTables.Author()
    authorDb.AddRow(author.Name)
    use connection = new SqlConnection(connectionString)
    connection.Open()
    try authorDb.Update(connection) |> ignore
    with
    | PrimaryKeyConstraint -> ()
    | _ -> reraise()

let private addBookAuthor (connectionString:string) (bookId: int) (authorId: int) tran= 
    use reference = new LibraryDb.dbo.Tables.Author_Book()
    reference.AddRow(authorId, bookId)
    use connection = new SqlConnection(connectionString)
    connection.Open()
    try reference.Update(connection,tran) |> ignore
    with
    | PrimaryKeyConstraint -> ()
    | _ -> reraise()
    
let addBook (connectionString:string) (book: Book) = 
    use bookDb = new DbTables.Book()
    bookDb.AddRow(book.Title,book.ISBN, book.Description)
    use connection = new SqlConnection(connectionString)
    connection.Open()
    use tran = connection.BeginTransaction()
    try bookDb.Update(connection) |> ignore
    with
    | PrimaryKeyConstraint -> match (getBook connectionString book.Title , getAuthor connectionString book.Author.Name) with
                              | Some existingBook, Some existingAuthor -> addBookAuthor connectionString existingBook.Id existingAuthor.Id tran
                              | _, _-> failwith "Book not saved"
    | _ -> reraise()
    