module Domain

type Book = { Title : string; ISBN : string option; Description : string option }
type Author = { Name: string }
type ExistingAuthor = { Id: int ; Name: string }
type ExistingBook =  {Id: int; Title : string; ISBN : string option; Description : string option }