#r "System.Drawing"
#r "packages/FSharp.Data/lib/net45/FSharp.Data.Dll"

open System.Drawing
open System.Drawing.Imaging
open System.IO
open FSharp.Data
open FSharp.Data.JsonExtensions

let getPlayers = 

  let getPage page = 
    async {
      let! data = JsonValue.AsyncLoad ("https://www.easports.com/fifa/ultimate-team/api/fut/item?page=" + page.ToString())
      let items = 
        [| for item in data?items -> item |] 
        |> Array.filter (fun item -> item?playerType.AsString() = "rare" || item?playerType.AsString() = "standard")

      return items
    }

  let value = JsonValue.Load "https://www.easports.com/fifa/ultimate-team/api/fut/item"
  let totalPages = value?totalPages.AsInteger()

  [|1..totalPages|] 
  |> Array.map getPage
  |> Async.Parallel
  |> Async.RunSynchronously
  |> Array.concat

let isPortly player = 
  let height = player?height.AsFloat() / 100.0
  let weight = player?weight.AsFloat() 
  let bmi = weight / (height * height)
  bmi >= 25.0

let getImage url = 
  match Http.Request(url).Body with
  | Binary bytes -> bytes
  | _ -> failwith "Text"

let detectFace player = 
  let headshotImgUrl = player?headshotImgUrl.AsString()
  let bytes = getImage headshotImgUrl

  let face = 
    Http.RequestString
      ("https://westeurope.api.cognitive.microsoft.com/face/v1.0/detect", 
        httpMethod = "POST",
        body    = BinaryUpload bytes,
        headers = [ "Ocp-Apim-Subscription-Key", "<insert key here>" ],
        query   = [ "returnFaceId", "false"; "returnFaceLandmarks", "false"; "returnFaceAttributes", "smile,facialHair" ])
    |> JsonValue.Parse

  player, face

let hasFace (playerFaces: JsonValue * JsonValue) = 
  let _, faces = playerFaces
  match faces.AsArray().Length with
  | 1 -> true
  | _ -> false

let joyAndBeard (playerFaces1: JsonValue * JsonValue) (playerFaces2: JsonValue * JsonValue) = 

  let getJoyAndBeard (face: JsonValue) =
    let smile = face?faceAttributes?smile.AsFloat()
    let beard = face?faceAttributes?facialHair?beard.AsFloat()
    let moustache = face?faceAttributes?facialHair?moustache.AsFloat()
    let sideburns = face?faceAttributes?facialHair?sideburns.AsFloat()
    smile + beard + moustache + sideburns

  let _, faces1 = playerFaces1
  let _, faces2 = playerFaces2
  let face1 = getJoyAndBeard faces1.[0]
  let face2 = getJoyAndBeard faces2.[0]

  if face1 > face2 then -1
  elif face1 < face2 then 1
  else 0

let santafy index (playerFaces: JsonValue * JsonValue) =
  let player, faces = playerFaces
  let id = player?id.AsString()
  let name = (player?firstName.AsString() + "_" + player?lastName.AsString()).Replace(' ', '_')
  let headshotImgUrl = player?headshotImgUrl.AsString()
  let bytes = getImage headshotImgUrl

  let face = faces.AsArray().[0]
  let left = face?faceRectangle?left.AsInteger()
  let top = face?faceRectangle?top.AsInteger()
  let width = face?faceRectangle?width.AsInteger()
  let height = face?faceRectangle?height.AsInteger()

  use inputStream = new MemoryStream(bytes)
  use faceImage = Image.FromStream(inputStream)
  let santaHatImage = Image.FromFile("santa-hat.png")
  let image = Image.FromFile("background.png")
  use graphics = Graphics.FromImage(image)
  graphics.DrawImage(faceImage, 30, 56)
  graphics.DrawImage(santaHatImage, left + 2, top - 5, width + 38, height + 50)

  let filename = sprintf "%i-%s-%s.jpg" (index + 1) id name
  image.Save(filename, ImageFormat.Jpeg)

getPlayers
|> Array.filter isPortly
|> Array.map detectFace
|> Array.filter hasFace
|> Array.sortWith joyAndBeard
|> Array.take 1
|> Array.iteri santafy