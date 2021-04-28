module Fable.Form.Base

/// <summary>
/// Represents a filled field
/// </summary>
type FilledField<'Field> =
    {
        State : 'Field
        Error : Error.Error option
        IsDisabled : bool
    }

/// <summary>
/// Represents a filled form
///
/// You can obtain this by using <see cref="fill"/>
/// </summary>
type FilledForm<'Output, 'Field> =
    {
        Fields : FilledField<'Field> list
        Result : Result<'Output, (Error.Error * Error.Error list)>
        IsEmpty : bool
    }

/// <summary>
/// A <see cref="T:Form"/> which can contain any type of 'field'
/// </summary>
type Form<'Values, 'Output, 'Field> =
    Form of ('Values -> FilledForm<'Output, 'Field>)

/// <summary>
/// <see cref="T:FieldConfig"/> is a contract allowing you to describe how a field will behave
/// </summary>
type FieldConfig<'Attributes, 'Input, 'Values, 'Output> =
    {
        /// <summary>
        /// Function that valides the <c>'Input</c> value and produce an <c>Ok 'Ouput</c> on success or an <c>Error</c> describing the problem
        /// </summary>
        Parser : 'Input -> Result<'Output, string>
        /// <summary>
        /// Function which defined how to access the <c>'Input</c> from <c>'Value</c> type
        /// </summary>
        Value : 'Values -> 'Input
        /// <summary>
        /// Function which defined how the current form <c>'Values</c> should be update with the new <c>'Input</c>
        /// </summary>
        Update : 'Input -> 'Values -> 'Values
        /// <summary>
        /// Defube how to obtain a potential external error. Useful when dealing with Server-side validation for example
        /// </summary>
        Error : 'Values -> string option
        /// <summary>
        /// Type used to represents data specific to the field. For example, you can use it to ask the user to provide a label and placeholder.
        /// </summary>
        Attributes : 'Attributes
    }

/// <summary>
/// Represents a custom field on a form that has been filled with some values.
/// </summary>
type CustomField<'Output, 'Field> =
    {
        /// <summary>
        ///
        /// </summary>
        State : 'Field
        /// <summary>
        ///
        /// </summary>
        Result : Result<'Output, (Error.Error * Error.Error list)>
        /// <summary>
        ///
        /// </summary>
        IsEmpty : bool
    }

let succeed (output : 'Output) : Form<'Values, 'Output, 'Field> =
    Form (
        fun _ ->
            {
                Fields = []
                Result = Ok output
                IsEmpty = true
            }
    )

let fill<'Values, 'Output, 'Field> (Form form : Form<'Values, 'Output, 'Field>) : 'Values -> FilledForm<'Output, 'Field> =
    form

let custom (fillField : 'Values -> CustomField<'Output, 'Field>) : Form<'Values, 'Output, 'Field> =
    Form (
        fun values ->
            let filled =
                fillField values

            {
                Fields =
                    [
                        {
                            State = filled.State
                            Error =
                                if filled.IsEmpty then
                                    Some Error.RequiredFieldIsEmpty

                                else
                                    match filled.Result with
                                    | Ok _ ->
                                        None

                                    | Error (firstError, _) ->
                                        Some firstError
                            IsDisabled = false
                        }
                    ]
                Result = filled.Result
                IsEmpty = filled.IsEmpty
            }
    )

let meta (fn : 'Values -> Form<'Values, 'Output, 'Field>) : Form<'Values, 'Output, 'Field> =
    Form (
        fun values ->
            fill (fn values) values
    )

let mapValues
    (fn : 'A -> 'B)
    (form : Form<'B, 'Output, 'Field>)
    : Form<'A, 'Output, 'Field> =

    Form (fn >> fill form)

let mapField
    (fn : 'A -> 'B)
    (form : Form<'Values, 'Output, 'A>)
    : Form<'Values, 'Output, 'B> =
    Form (
        fun values ->
            let filled =
                fill form values

            {
                Fields =
                    filled.Fields
                    |> List.map (fun filledField ->
                        {
                            State = fn filledField.State
                            Error = filledField.Error
                            IsDisabled = filledField.IsDisabled
                        }
                    )
                Result = filled.Result
                IsEmpty = filled.IsEmpty
            }
    )

let append (newForm : Form<'Values, 'A, 'Field>) (currentForm : Form<'Values, 'A -> 'B, 'Field>) : Form<'Values, 'B, 'Field> =
    Form (
        fun values ->
            let filledNew =
                fill newForm values

            let filledCurrent =
                fill currentForm values

            let fields =
                filledCurrent.Fields @ filledNew.Fields

            let isEmpty =
                filledCurrent.IsEmpty && filledNew.IsEmpty

            match filledCurrent.Result with
            | Ok fn ->
                {
                    Fields = fields
                    Result = Result.map fn filledNew.Result
                    IsEmpty = isEmpty
                }

            | Error (firstError, otherErrors) ->
                match filledNew.Result with
                | Ok _ ->
                    {
                        Fields = fields
                        Result = Error (firstError, otherErrors)
                        IsEmpty = isEmpty
                    }

                | Error (newFirstError, newOtherErrors) ->
                    {
                        Fields = fields
                        Result =
                            Error (
                                firstError,
                                otherErrors @ (newFirstError :: newOtherErrors)
                            )
                        IsEmpty = isEmpty
                    }
    )

let andThen
    (child : 'A -> Form<'Values, 'B, 'Field>)
    (parent : Form<'Values, 'A, 'Field>)
    : Form<'Values, 'B, 'Field> =

    Form (
        fun values ->
            let filled =
                fill parent values

            match filled.Result with
            | Ok output ->
                let childFilled =
                    fill (child output) values

                {
                    Fields = filled.Fields @ childFilled.Fields
                    Result = childFilled.Result
                    IsEmpty = filled.IsEmpty && childFilled.IsEmpty
                }

            | Error errors ->
                {
                    Fields = filled.Fields
                    Result = Error errors
                    IsEmpty = filled.IsEmpty
                }
    )

/// <summary>
/// Transform the 'output' of a form
///
/// You can use it to keep your forms decoupled from your specific view messages:
///
/// <code lang="fsharp">
/// Base.map SignUp signupForm
/// </code>
/// </summary>
let map
    (fn : 'A -> 'B)
    (form : Form<'Values, 'A, 'Field>)
    : Form<'Values, 'B, 'Field> =

    Form (
        fun values ->
            let filled =
                fill form values

            {
                Fields = filled.Fields
                Result = Result.map fn filled.Result
                IsEmpty = filled.IsEmpty
            }
    )

let field
    (isEmpty : 'Input -> bool)
    (build : Field.Field<'Attributes, 'Input, 'Values> -> 'Field)
    (config : FieldConfig<'Attributes, 'Input, 'Values, 'Output>)
    : Form<'Values, 'Output, 'Field> =

    let requiredParser value =
        if isEmpty value then
            Error (Error.RequiredFieldIsEmpty, [])
        else
            config.Parser value
            |> Result.mapError (fun error ->
                (Error.ValidationFailed error, [])
            )

    let parse values =
        requiredParser (config.Value values)
        |> Result.andThen (fun output ->
            config.Error values
            |> Option.map (fun error ->
                Error (Error.External error, [])
            )
            |> Option.defaultValue (Ok output)
        )

    let field_ values =
        let value =
            config.Value values

        let update newValue =
            config.Update newValue values

        build
            {
                Value = value
                Update = update
                Attributes = config.Attributes
            }

    Form (
        fun values ->
            let result =
                parse values

            let (error, isEmpty_) =
                match result with
                | Ok _ ->
                    (None, false)

                | Error (firstError, _) ->
                    Some firstError, firstError = Error.RequiredFieldIsEmpty
            {
                Fields =
                    [ { State = field_ values; Error = error; IsDisabled = false } ]
                Result = result
                IsEmpty = isEmpty_
            }
    )

let optional
    (form : Form<'Values, 'Output, 'Field>)
    : Form<'Values, 'Output option, 'Field> =

    Form (
        fun values ->
            let filled =
                fill form values

            match filled.Result with
            | Ok value ->
                {
                    Fields = filled.Fields
                    Result = Ok (Some value)
                    IsEmpty = filled.IsEmpty
                }

            | Error (firstError, otherErrors) ->
                if filled.IsEmpty then
                    {
                        Fields =
                            filled.Fields
                            |> List.map (fun field ->
                                { field with Error = None }
                            )
                        Result = Ok None
                        IsEmpty = filled.IsEmpty
                    }
                else
                    {
                        Fields = filled.Fields
                        Result = Error (firstError, otherErrors)
                        IsEmpty = false
                    }
    )
