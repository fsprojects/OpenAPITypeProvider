module internal OpenAPITypeProvider.Names

open System

// Taken from: https://github.com/fsharp/FSharp.Data/blob/master/src/CommonRuntime/NameUtils.fs

// --------------------------------------------------------------------------------------
// Active patterns & operators for parsing strings

let private tryAt (s:string) i = if i >= s.Length then None else Some s.[i]
let private sat f (c:option<char>) = match c with Some c when f c -> Some c | _ -> None
let private (|EOF|_|) c = match c with Some _ -> None | _ -> Some ()
let private (|LetterDigit|_|) = sat Char.IsLetterOrDigit
let private (|Upper|_|) = sat (fun c -> Char.IsUpper c || Char.IsDigit c)
let private (|Lower|_|) = sat (fun c -> Char.IsLower c || Char.IsDigit c)

// --------------------------------------------------------------------------------------

/// Turns a given non-empty string into a nice 'PascalCase' identifier
let pascalName (s:string) = 
  if s.Length = 1 then s.ToUpperInvariant() else
  // Starting to parse a new segment 
  let rec restart i = seq {
    match tryAt s i with 
    | EOF -> ()
    | LetterDigit _ & Upper _ -> yield! upperStart i (i + 1)
    | LetterDigit _ -> yield! consume i false (i + 1)
    | _ -> yield! restart (i + 1) }
  // Parsed first upper case letter, continue either all lower or all upper
  and upperStart from i = seq {
    match tryAt s i with 
    | Upper _ -> yield! consume from true (i + 1) 
    | Lower _ -> yield! consume from false (i + 1) 
    | _ ->
        yield from, i
        yield! restart (i + 1) }
  // Consume are letters of the same kind (either all lower or all upper)
  and consume from takeUpper i = seq {
    match tryAt s i with
    | Lower _ when not takeUpper -> yield! consume from takeUpper (i + 1)
    | Upper _ when takeUpper -> yield! consume from takeUpper (i + 1)
    | Lower _ when takeUpper ->
        yield from, (i - 1)
        yield! restart (i - 1)
    | _ -> 
        yield from, i
        yield! restart i }
    
  // Split string into segments and turn them to PascalCase
  seq { for i1, i2 in restart 0 do 
          let sub = s.Substring(i1, i2 - i1) 
          if Array.forall Char.IsLetterOrDigit (sub.ToCharArray()) then
            yield sub.[0].ToString().ToUpperInvariant() + sub.ToLowerInvariant().Substring(1) }
  |> String.Concat


/// Turns a given non-empty string into a nice 'camelCase' identifier
let camelName (s:string) = 
    let name = pascalName s
    if name.Length > 0 then
        name.[0].ToString().ToLowerInvariant() + name.Substring(1)
    else name