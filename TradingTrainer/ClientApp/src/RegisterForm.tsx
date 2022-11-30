import React, { ReactNode, Component, useState, ChangeEvent } from 'react';
import Box from '@mui/material/Box';
import { Link, useNavigate } from 'react-router-dom';
import TextField from '@mui/material/TextField';
import {Button, Typography } from '@mui/material';
import IUserData from './Models/Iuser';
//import UserService from "./Services/UserServices";
import axios from 'axios';
import red from '@mui/material/colors/red';
import { blue } from '@mui/material/colors';



const RegisterForm: React.FC = () => {

   /**intial user state object that will hold the user detail information to send to the server */
    const initialUserState = {
        id: null,
        firstName: "",
        lastName: "",
        email: "",
        password: "",
        alphaVantageApiKey: ""
    };

    const navigate = useNavigate();

 
    //name error state and name error message state to handle validation and error message showing. This state used by the error and helpertext props of the text feild.
    const [nameStateError, nameSetStateError] = useState<boolean>(false);
    const [nameErrorMsg, nameSetErrorMsg] = useState<string>('');

    //last name error state and name error message state to handle validation and error message showing. This state used by the error and helpertext props of the text feild.
    const [lNameStateError, lNameSetStateError] = useState<boolean>(false);
    const [lNameErrorMsg, lNameSetErrorMsg] = useState<string>('');

    //email error state and password error message state to handle validation and error message showing.This state used by the error and helpertext props of the text feild.
    const [emailStateError, emailSetStateError] = useState<boolean>(false);
    const [emailErrorMsg, emailSetErrorMsg] = useState<string>('');

    //password error state and password error message state to handle validation and error message showing. This state used by the error and helpertext props of the text feild.
    const [pwdStateError, pwdSetStateError] = useState<boolean>(false);
    const [pwdErrorMsg, pwdSetErrorMsg] = useState<string>('');

    //confirm password error state and conforim password error message state to handle validation and error message showing. This state used by the error and helpertext props of the text feild.
    const [cpwdStateError, cpwdSetStateError] = useState<boolean>(false);
    const [cpwdErrorMsg, cpwdSetErrorMsg] = useState<string>('');

    //alphavantage key error state and alphavantage key error message state to handle validation and error message showing. This state used by the error and helpertext props of the text feild.
    const [keyStateError, keySetStateError] = useState<boolean>(false);
    const [keyErrorMsg, keySetErrorMsg] = useState<string>('');

    
    //general error message state to handle errors of empty form sending. This state used by the error and helpertext props of a typography at the top of the form.
    const [errorMsg, setErrorMsg] = useState<string>('');

    
    // use state hook to handle the user state. users holds initial user object at the start and gets updates by adding a user information object to the state
    const [users, setUser] = useState<IUserData>(initialUserState);
    const [submitted, setSubmitted] = useState<boolean>(false);

  


    /**
     * function to keep track of change event in the text feild and add the input value of the feild to the user object using useState set function. 
     * When a value and a name of the value is being added to the users object, the previous objects contet is expanded inside the new object such that the previous added values are not replaced by the new ones
     * This function is called by the onchange props of the text feild.
     * @param event
     */
    const handleInputChange = (event: ChangeEvent<HTMLTextAreaElement>) => {

        const { name, value } = event.target;
        const test = { ...users, [name]: value }
        setUser(test);

    }


    /**
     * Function to validate first name by checking its value after the text feild has lost focus. It checks the value against a given regexp specification. 
     * If the test method of regexp returns false then the state of the feild is set true and error message state is given a string value to print out.
     * This function is called from the first name text feild prop onBlur.
     * @param e
     */

    const validateFirstName = (e: React.FocusEvent<HTMLTextAreaElement>) => {

        nameSetStateError(false);
        nameSetErrorMsg("");
        const namePattern: RegExp = /^[a-zæøåA-ZÆØÅ]{2,20}$/;
        const ok = namePattern.test(e.target.value);
        if (!ok) {

            nameSetStateError(true);
            setSubmitted(false);
            nameSetErrorMsg("Invalid First Name!");
           
        }

    }


    /**
     * Function to validate last name by checking its value after the text feild has lost focus. It checks the value against a given regexp specification. 
     * If the test method of regexp returns false then the state of the feild is set true and error message state is given a string value to print out.
     * This function is called from the first name text feild prop onBlur.
     * @param e
     */
    const validateLastName = (e: React.FocusEvent<HTMLTextAreaElement>) => {


        lNameSetStateError(false);
        lNameSetErrorMsg("");
        const namePattern: RegExp = /^[a-zæøåA-ZÆØÅ]{2,20}$/;
        const ok = namePattern.test(e.target.value);
        if (!ok) {

            lNameSetStateError(true);
            setSubmitted(false);
            lNameSetErrorMsg("Invalid Last Name!");
           
        }

    }



    /**
     * Function to validate email by checking its value after the text feild has lost focus. It checks the value against a given regexp specification. 
     * If the test method of regexp returns false then the state of the feild is set true and error message state is given a string value to print out.
     * This function is called from the first name text feild prop onBlur.
     * @param e
     */

    const validateEmail = (e: React.FocusEvent<HTMLTextAreaElement>) => {

        emailSetStateError(false);
        emailSetErrorMsg("");
        const namePattern: RegExp = /^[a-zA-Z\\#\\!\\%\\$\\‘\\&\\+\\*\\–\\/\\=\\?\\^\\_\\`\\.\\{\\|\\}\\~]+@[a-zA-Z0-9\\-\\.]{1,63}$/;
        const ok = namePattern.test(e.target.value);
        if (!ok) {

            emailSetStateError(true);
            setSubmitted(false);
            emailSetErrorMsg("Invalid Email!");
           
        }

    }


    /**
     * Function to validate password by checking its value after the text feild has lost focus. It checks the value against a given specifications. 
     * If true then the state of the feild is set true and error message state is given a string value to print out.
     * This function is called from the first name text feild prop onBlur.
     * @param e
     */

    const validatePassword = (e: React.FocusEvent<HTMLTextAreaElement>) => {

        
        if (e.target.value.length < 9) {
            // The password length is invalid

            pwdSetStateError(true);
            setSubmitted(false);
            pwdSetErrorMsg("Invalid password.Password must contain more than 9 characters, numbers 0 - 9, letter characters and special characters!");
            return false;


        }
        if (!(/\d/.test(e.target.value))) {
            // The passsword does not contain any numbers
            pwdSetStateError(true);
            setSubmitted(false);
            pwdSetErrorMsg("Invalid password.Password must contain more than 9 characters, numbers 0 - 9, letter characters and special characters!");

            return false;
        }
        if (!(/\w/.test(e.target.value))) {
            // The password does not contain any word characters
            pwdSetStateError(true);
            setSubmitted(false);
            pwdSetErrorMsg("Invalid password.Password must contain more than 9 characters, numbers 0 - 9, letter characters and special characters!");
            return false;

        }
        if (!(/[$&+,:;=?@#|'<>-^*()%!]/.test(e.target.value))) {
            // The password does not contain any special characters
            pwdSetStateError(true);
            setSubmitted(false);
            pwdSetErrorMsg("Invalid password.Password must contain more than 9 characters, numbers 0 - 9, letter characters and special characters!");

            return false;
        }

        pwdSetStateError(false);
        pwdSetErrorMsg("");
        return true;
    }



    /**
     * Function to validate confirm password by checking its value after the text feild has lost focus. It checks the value against a given regexp specification. 
     * If the test method of regexp returns false then the state of the feild is set true and error message state is given a string value to print out.
     * This function is called from the first name text feild prop onBlur.
     * @param e
     */

    const validateConfirmPassword = (e: React.FocusEvent<HTMLTextAreaElement>) => {

        cpwdSetStateError(false);
        cpwdSetErrorMsg("");

        if (e.target.value !== users.password) {

              cpwdSetStateError(true);
        }
       
    }




    /**
     * Function to validate the key from alphavantagekey by checking its value after the text feild has lost focus. It checks the value against a given regexp specification. 
     * If the test method of regexp returns false then the state of the feild is set true and error message state is given a string value to print out.
     * This function is called from the first name text feild prop onBlur.
     * @param e
     */

    const validateAlphaKey = (e: React.FocusEvent<HTMLTextAreaElement>) => {

        keySetStateError(false);
        keySetErrorMsg("")
      
        if (e.target.value !== "") {

            return true;

        }
            keySetStateError(true);
            keySetErrorMsg("Invalid key!")
            return false;
    }

   
  


    /**
     * this function is triggered when register button is clicked. It maps the state object users to a user object.
     * Uses axios to send the data to the right endpoint
     * */
    const send = () => {

         //checks if all feilds are provided, if not halt the call
        if (users.firstName === "" && users.lastName === "" && users.email === "" && users.password === "" && users.alphaVantageApiKey === "") {

            return;
        }
        //checks is all provided information are valid, if not halt the call and print a message on top of the form
        else if ((nameStateError || lNameStateError || emailStateError || pwdStateError || cpwdStateError || keyStateError)) {

            const msg = ("Please fill out the form to register!");
            setErrorMsg(msg);
            return;

        }
         //check if confirm password matches the password, if not halt the call an print not valid message
        else if (cpwdStateError) {
            cpwdSetStateError(true);
            cpwdSetErrorMsg("Passwords do not match!");
        }


        //user object
        const user = {
            firstName: users.firstName,
            lastName: users.lastName,
            email: users.email,
            password: users.password,
            alphaVantageApiKey: users.alphaVantageApiKey
        }

        axios
            .post("/trading/createuser/", user)
            .then((response: any) => {
                console.log(user)
                setSubmitted(true);
                console.log(response.data);
            })
            .catch((e: Error) => {
                console.log(e);
            });

       
            setUser(initialUserState);
            setSubmitted(false);
            navigate("/login");
        
   }
           
    return (

        <Box
            sx={{

                '& > :not(style)': { m: 1 }, m: 2,
            }}
        >
            <Typography variant="h6" mb={2} sx={{ padding: 0, m: 0 }} >Register Form</Typography>

            <Typography variant="body2" mb={2} mt={2} sx={{ color: red }} >{errorMsg}</Typography>


            <form noValidate autoComplete="off">

                <TextField
                    sx={{ mt: 2, mb: 2 }}

                    name="firstName"
                    label="First Name"
                    variant="outlined"
                    value={users.firstName}
                    onChange={handleInputChange}
                    onBlur={validateFirstName}
                    fullWidth
                    required
                    error={nameStateError}
                    helperText={nameErrorMsg}

            />
          
                <TextField
                    sx={{ mt: 2, mb: 2 }}
                    name="lastName"
                    label="Last Name"
                    variant="outlined"
                    value={users.lastName}
                    onChange={handleInputChange}
                    onBlur={validateLastName}
                    fullWidth
                    error={lNameStateError}
                    helperText={lNameErrorMsg}
                />
                <TextField
                    sx={{ mt: 2, mb: 2 }}
                    name="email"
                    label="Email"
                    variant="outlined"
                    value={users.email}
                    onChange={handleInputChange}
                    onBlur={validateEmail}
                    fullWidth
                    error={emailStateError}
                    helperText={emailErrorMsg}
                />
                <TextField
                    sx={{ mt: 2, mb: 2 }}
                    name="password"
                    label="Password"
                    type="Password"
                    variant="outlined"
                    value={users.password}
                    onChange={handleInputChange}
                    onBlur={validatePassword}
                    fullWidth
                    error={pwdStateError}
                    helperText={pwdErrorMsg}
                />
                <TextField
                    sx={{ mt: 2, mb: 2 }}
                    name="confirmPassword"
                    label="Confirm Password"
                    type="Password"
                    variant="outlined"
                    onChange={handleInputChange}
                    onBlur={validateConfirmPassword}
                    fullWidth
                    error={cpwdStateError}
                    helperText={cpwdErrorMsg}
                />
                <TextField
                    sx={{ mt: 2, mb: 2 }}
                    name="alphaVantageApiKey"
                    id="outlined-basic"
                    label="AlphaVantage Key"
                    variant="outlined"
                    value={users.alphaVantageApiKey}
                    onChange={handleInputChange}
                    onBlur={validateAlphaKey}
                    fullWidth
                    error={keyStateError}
                    helperText={keyErrorMsg}
                />
                <nav className="landingNavigation">
                    <Link className="btn btn-lg btn-outline-secondary" to="/">Cancel</Link>
                    <Button
                        className="btn btn-lg btn-outline-primary"
                        variant="contained"
                        sx={{ color: blue, borderRedius: 30, ml: 2, p: 1.5 } }
                        onClick={send} > Register</Button>
                </nav>
            </form>
            <Typography variant="body1" sx={{ color: "red", pt: 5 }}>Sign up to <a href="https://www.alphavantage.co/">Alpha Vantage</a> get a free access key</Typography>


        </Box>

        );
    }

export default RegisterForm;

