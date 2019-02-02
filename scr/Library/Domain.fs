module Library.Domain

type Author = { Name: string }
type ExistingAuthor = {Id : int ; Name : string }
type Book = { Title : string; ISBN : string option; Description : string option ; Author : Author }
type ExistingBook =  {Id: int; Title : string; ISBN : string option; Description : string option }