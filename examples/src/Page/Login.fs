module Page.Login

open Warded.Simple
open Elmish

/// <summary>
/// Type used to represent the form values
/// </summary>
type FormValues =
    {
        Email : string
        Password : string
        RememberMe : bool
    }

/// <summary>
/// Represents the model of your Elmish component
///
/// In the case of the Login example, we just need to keep track of the Form model state
/// </summary>
type Model =
    Form.View.Model<FormValues>

/// <summary>
/// Represents the different messages that your application can react too
/// </summary>
type Msg =
    // Used when a change occure in the form
    | FormChanged of Model
    // Used when the user submit the form
    | LogIn of EmailAddress.T * string * bool

let init () =
    {
        Email = ""
        Password = ""
        RememberMe = false
    }
    |> Form.View.idle
    , Cmd.none

let update (msg : Msg) (model : Model) =
    match msg with
    // Update our model to it's new state
    | FormChanged newModel ->
        newModel
        , Cmd.none

    // Form has been submitted
    // Here, we have access to the value submitted from the from
    | LogIn (email, password, rememberMe) ->
        // For the example, we just set a message in the Form view
        { model with
            State = Form.View.Success "You have been logged in successfully"
        }
        , Cmd.none


/// <summary>
/// Define the form logic
///
/// We need to define each field logic first and then define how the fields are wired together to make the form
/// </summary>
/// <returns>The form ready to be used in the view</returns>
let form : Form.Form<FormValues, Msg> =
    let emailField =
        Form.textField
            {
                // Use a custom EmailAddress parser and map the result back into a string
                // as the email field is represented using a string in the Form
                Parser =
                    EmailAddress.tryParse >> Result.map EmailAddress.toString
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


    /// <summary>
    /// Function used to map the form values into the message to send back to the update function
    ///
    /// As you can see, it is possible here to transform the "loosy" types into domain types
    /// </summary>
    /// <returns></returns>
    let formOutput =
        fun email password rememberMe ->
            LogIn (EmailAddress.create email, password, rememberMe)

    Form.succeed formOutput
        |> Form.append emailField
        |> Form.append passwordField
        |> Form.append rememberMe

let view (model : Model) (dispatch : Dispatch<Msg>) =
    Form.View.asHtml
        {
            Dispatch = dispatch
            OnChange = FormChanged
            Action = "Submit"
            Loading = "Loading"
            Validation = Form.View.Validation.ValidateOnSubmit
        }
        form
        model

let code =
    """
Form.succeed formOutput
    |> Form.append emailField
    |> Form.append passwordField
    |> Form.append rememberMe
    """

let githubLink =
    Env.generateGithubUrl __SOURCE_DIRECTORY__ __SOURCE_FILE__