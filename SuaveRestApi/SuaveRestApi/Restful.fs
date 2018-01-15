namespace SuaveRestApi.Rest

open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Suave
open Suave.Operators
open Suave.Http
open Suave.Successful
open System.Text

[<AutoOpen>]
module RestFul =  
    open Suave.Filters
    open Suave.RequestErrors
    open SuaveRestApi.Db.Db

    type RestResource<'a> = {
        GetAll      : unit -> 'a seq
        Create      : 'a -> 'a
        Update      : 'a -> 'a option
        Delete      : int -> unit
        GetById     : int -> 'a option
        UpdateById  : int -> 'a -> 'a option
        Exists      : int -> bool
    }

    let JSON v =     
        let jsonSerializerSettings = new JsonSerializerSettings()
        jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()
    
        JsonConvert.SerializeObject(v, jsonSerializerSettings)
        |> OK 
        >=> Writers.setMimeType "application/json; charset=utf-8"

    let fromJson<'a> json = JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

    let getResourceFromReq<'a> (req : HttpRequest) =
        let getString rawForm = Encoding.UTF8.GetString(rawForm)
        req.rawForm |> getString |> fromJson<'a>

    let rest resourceName resource =
        let resourcePath = "/" + resourceName

        let resourceIdPath = 
            new PrintfFormat<(int -> string), unit, string, string, int>(resourcePath + "/%d")

        let badRequest = BAD_REQUEST "Resource not found"

        let getAll = warbler (fun _ -> resource.GetAll () |> JSON)
        
        let handleResource requestError = function
            | Some r    -> r |> JSON
            | None      -> requestError

        let deleteResourceById id =
            resource.Delete id
            NO_CONTENT

        let getResourceById =
            resource.GetById >> handleResource (NOT_FOUND "Resource not found")

        let updateResourceById id =
            request (getResourceFromReq >> (resource.UpdateById id) >> handleResource badRequest)

        let resourceExists id =
            match resource.Exists id with
            | true  -> OK ""
            | false -> NOT_FOUND ""
            

        choose [
            path resourcePath >=> choose [
                GET     >=> getAll
                POST    >=> request (getResourceFromReq >> resource.Create >> JSON)
                PUT     >=> request (getResourceFromReq >> resource.Update >> handleResource badRequest)
            ]
            DELETE  >=> pathScan resourceIdPath deleteResourceById
            GET     >=> pathScan resourceIdPath getResourceById
            PUT     >=> pathScan resourceIdPath updateResourceById
            HEAD    >=> pathScan resourceIdPath resourceExists
        ]