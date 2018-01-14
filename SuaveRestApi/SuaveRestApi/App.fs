namespace SuaveRestApi

module App =   

    open Suave.Web
    open SuaveRestApi.Rest
    open SuaveRestApi.Db

    [<EntryPoint>]
    let main argv = 
        let personWebPart = rest "people" { 
            GetAll = Db.getPeople
            Create = Db.createPerson
            Update = Db.updatePerson
        }
        startWebServer defaultConfig personWebPart
        0
