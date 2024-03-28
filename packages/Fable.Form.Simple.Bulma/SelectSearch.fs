namespace Fable.Form.Simple.Bulma

[<AutoOpen>]
module SelectSearch =
    open Feliz
    open Fable.Core

    [<ReactComponent(import = "SelectSearch", from = "./SelectSearch.jsx")>]
    let SelectSearch
        (props:
            {| disabled: bool
               placeholder: string
               value: obj
               onChange: obj -> unit
               onBlur: Option<Browser.Types.FocusEvent -> unit>
               loadOptions: string -> JS.Promise<obj array>
               getOptionValue: obj -> string
               getOptionLabel: obj -> string |})
        =
        React.imported ()
