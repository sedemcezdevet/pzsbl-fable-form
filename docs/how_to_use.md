---
title: How to use?
---

In this section, we are going to focus on the easiest way to use Fable.Form.

We are going to create a login form as the one available [here](Fable.Form/examples/index.html#login)

It will allows you to quickly test it and decide if it can fit your needs.

Later, once you are more experience with Fable.Form and decide to use it in your application, you should read the [Advanced section](Fable.Form/advance)

<ul class="textual-steps">
<li>

Add **Fable.Form.Simple.Feliz.Bulma** to your project

For example, if you are using `dotnet` you need to do:

```
dotnet add yourProject.fsproj package Fable.Form.Simple.Feliz.Bulma
```

</li>

<li>

Open the library modules

```fsharp
open Fable.Form.Simple
open Fable.Form.Simple.Feliz.Bulma
```

</li>

<li>

Define a type `Values` which is used to represents the different fields we have in the form.

```fsharp
type Values =
    {
        Email : string
        Password : string
        RememberMe : bool
    }
```

</li>

<li>
Create your model

```fsharp
type Model =
    Form.View.Model<Values>
```

*To keep our example simple, we use a type alias but in a real application you will generaly host it inside a discriminated union or a record*

</li>

<li>

Register 2 messages:

```fsharp
type Msg =
    // Used when a change occure in the form
    | FormChanged of Model
    // Used when the user submit the form
    | LogIn of string * string * bool
```

</li>

<li>

Initialize your `Model`, we set the default value of each fields. Then pass the values to the function `Form.View.idle` which will returns a `Form.View.Model`

```fsharp
let init () =
    {
        Email = ""
        Password = ""
        RememberMe = false
    }
    |> Form.View.idle
    , Cmd.none
```

</li>

<li>

Write the logic of the `update` function.

```fsharp
let update (msg : Msg) (model : Model) =
    match msg with
    // We received a new form model, store it
    | FormChanged newModel ->
        newModel
        , Cmd.none

    // The form has been submitted
    // Here, we have access to the value submitted from the from
    | LogIn (_email, _password, _rememberMe) ->
        // For this example, we just set a message in the Form view
        { model with
            State = Form.View.Success "You have been logged in successfully"
        }
        , Cmd.none
```

</li>

<li>

Create the form logic:

1. First create each field
2. Create an `onSubmit` function which maps the result of the form into a `Msg`
3. Tie the fields and the `onSubmit` function together

```fsharp
let form : Form.Form<Values, Msg> =
    let emailField =
        Form.textField
            {
                Parser =
                    EmailAddress.tryParse
                Value =
                    fun values -> values.Email
                Update =
                    fun newValue values ->
                        { values with Email = newValue }
                Error =
                    fun _ -> None
                Attributes =
                    {
                        Label = "Email"
                        Placeholder = "some@email.com"
                    }
            }

    let passwordField =
        Form.passwordField
            {
                Parser = Ok
                Value =
                    fun values -> values.Password
                Update =
                    fun newValue values ->
                        { values with Password = newValue }
                Error =
                    fun _ -> None
                Attributes =
                    {
                        Label = "Password"
                        Placeholder = "Your password"
                    }
            }

    let rememberMe =
        Form.checkboxField
            {
                Parser = Ok
                Value =
                    fun values -> values.RememberMe
                Update =
                    fun newValue values ->
                        { values with RememberMe = newValue }
                Error =
                    fun _ -> None
                Attributes =
                    {
                        Text = "Remember me"
                    }
            }

    let onSubmit =
        fun email password rememberMe ->
            LogIn (email, password, rememberMe)

    Form.succeed onSubmit
        |> Form.append emailField
        |> Form.append passwordField
        |> Form.append rememberMe
```

</li>

<li>

Call `Form.View.asHtml` in the `view` function to render the form

```fsharp
let view (model : Model) (dispatch : Dispatch<Msg>) =
    Form.View.asHtml
        {
            Dispatch = dispatch
            OnChange = FormChanged
            Action = "Sign in"
            Validation = Form.View.ValidateOnSubmit
        }
        form
        model
```

</li>

</ul>