namespace SuaveRestApi.Db

open System
open System.Collections.Generic

type Person = {
    Id          : int
    Name        : string
    BirthYear   : string
    Email       : string
}

module Db =
    open FSharp.Data

    let private peopleStorage = new Dictionary<int, Person>()

    //---------------- Begin - Star Wars API stuff to get a list of people in our storage
    [<Literal>]
    let baseUrl = "https://swapi.co/api/"
    [<Literal>] 
    let peopleUrl = baseUrl + "people/"

    type StarWarsAllPeople = JsonProvider<"https://swapi.co/api/people/?page=3">
    type Paging = JsonProvider<"""{"next": "https://swapi.co/api/people/?page=2"}""">
    let peopleParser url = StarWarsAllPeople.Parse(url).Results

    let rec getAll parser nextUrl acc =
        match nextUrl with 
        | "" -> List.rev acc |> Array.concat
        | url ->
            let text = Http.RequestString(url)
            let next = Paging.Parse(text).Next
            let contents = parser text
            getAll parser next (contents::acc)

    let getAllStarWarsPeople () = getAll peopleParser peopleUrl []
    let starWarsPeople = getAllStarWarsPeople()

    let extractNumber (s : string) =
        let nums = 
            s 
            |> Seq.filter (fun c -> Char.IsDigit c)
            |> Seq.toArray
            |> String.Concat
        let result, num = Int32.TryParse nums
        match result with
        | true -> num
        | false -> failwith "No number present"

    starWarsPeople
    |> Seq.iter (fun p -> 
            let newPerson = {
                Id          = p.Url.Substring(peopleUrl.Length) |> extractNumber
                Name        = p.Name
                BirthYear   = p.BirthYear
                Email       = sprintf "%s@starwars.com" p.Name
            }
            peopleStorage.Add(newPerson.Id, newPerson))
    
    //---------------- End - Star Wars API stuff to get a list of people in our storage

    let getPeople () = peopleStorage.Values |> Seq.map id
    let personExists = peopleStorage.ContainsKey

    let createPerson person =
        let id = peopleStorage.Values.Count + 1
        let newPerson = {
            Id          = id
            Name        = person.Name
            BirthYear   = person.BirthYear
            Email       = person.Email
        }
        peopleStorage.Add(id, newPerson)
        newPerson

    let updatePersonById id person =
        match personExists id with
        | true -> 
            let updatedPerson = {
                Id          = id
                Name        = person.Name
                BirthYear   = person.BirthYear
                Email       = person.Email
            }
            peopleStorage.[id] <- updatedPerson
            Some updatedPerson

        | false -> None

    let updatePerson person =
        updatePersonById person.Id person

    let deletePerson id =
        peopleStorage.Remove(id) |> ignore

    let getPerson id =
        match personExists id with
        | true -> Some peopleStorage.[id]
        | false -> None
