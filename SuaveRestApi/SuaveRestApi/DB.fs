namespace SuaveRestApi.Db

open System.Collections.Generic

type Person = {
    Id      : int
    Name    : string
    Age     : int
    Email   : string
}

module Db =
    let private peopleStorage = new Dictionary<int, Person>()
    let getPeople () = peopleStorage.Values |> Seq.map id
    let personExists = peopleStorage.ContainsKey

    let createPerson person =
        let id = peopleStorage.Values.Count + 1
        let newPerson = {
            Id      = id
            Name    = person.Name
            Age     = person.Age
            Email   = person.Email
        }
        peopleStorage.Add(id, newPerson)
        newPerson

    let updatePersonById id person =
        match personExists id with
        | true -> 
            let updatedPerson = {
                Id      = id
                Name    = person.Name
                Age     = person.Age
                Email   = person.Email
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
