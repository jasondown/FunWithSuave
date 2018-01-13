namespace SuaveRestApi

module App =   

    open Suave.Web
    open Suave.Successful
    open SuaveRestApi.Rest
    open SuaveRestApi.Db

    [<EntryPoint>]
    let main argv = 
        let personWebPart = rest "people" { 
            GetAll = Db.getPeople
            Create = Db.createPerson
        }
        startWebServer defaultConfig personWebPart
        0
