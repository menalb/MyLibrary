module Library.Api

open Library.Domain

type ILibraryApi =
    abstract member GetBook : title : string -> ExistingBook option
    abstract member AddBook : book : Book -> Unit

let private buildApi getBooksByTitle (getAuthor : string -> ExistingAuthor option) addBook addAuthor (addBookAuthor ) =
    { new ILibraryApi with
        member __.GetBook(title : string) = 
            getBooksByTitle title
        member __.AddBook ( book : Book ) = 

            match getBooksByTitle book.Title with
            | None -> addBook(book)
            | _ -> failwith "Book already existing"

            match getAuthor book.Author.Name with
            | Some _ -> ()
            | None -> addAuthor(book.Author)

            match (getBooksByTitle book.Title , getAuthor book.Author.Name) with
            | Some existingBook, Some existingAuthor -> addBookAuthor existingBook.Id existingAuthor.Id
            | _, _-> failwith "Book not saved"
    }

let createSqlApi connectionString = 
    buildApi
        (SqlRepository.getBook connectionString)
        (SqlRepository.getAuthor connectionString)
        (SqlRepository.addBook connectionString)
        (SqlRepository.addAuthor connectionString)
        (SqlRepository.addBookAuthor connectionString)