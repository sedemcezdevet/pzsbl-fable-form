namespace Fable.Form.Simple.Field

module SelectSearchField =

    open Fable.Core
    open Fable.Form

    [<NoComparison; NoEquality>]
    type Attributes =
        { Label: string
          Placeholder: string
          LoadOptions: string -> JS.Promise<obj list>
          GetOptionValue: obj -> string
          GetOptionLabel: obj -> string }

    type SelectSearchField<'Values> = Field.Field<Attributes, obj, 'Values>

    let form<'Values, 'Field, 'Output>
        : ((SelectSearchField<'Values> -> 'Field)
              -> Base.FieldConfig<Attributes, obj, 'Values, 'Output>
              -> Base.Form<'Values, 'Output, 'Field>) =
        Base.field (fun _ -> false)
