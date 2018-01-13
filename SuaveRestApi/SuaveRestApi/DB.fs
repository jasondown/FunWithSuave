namespace SuaveRestApi.Db

open System.Collections.Generic

type Person = {
    Id : int
    Name : string
    Age : int
    Email : string
}

module Db =
    let private peopleStorage = new Dictionary<int, Person>()
    let getPeople () = peopleStorage.Values |> Seq.map id

    let createPerson person =
        let id = peopleStorage.Values.Count + 1
        let newPerson = {
            Id = id
            Name = person.Name
            Age = person.Age
            Email = person.Email
        }
        peopleStorage.Add(id, newPerson)
        newPerson