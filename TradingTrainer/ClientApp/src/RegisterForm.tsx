import React, { ReactNode, Component, useState, ChangeEvent } from 'react';
import Box from '@mui/material/Box';
import { Link } from 'react-router-dom';
import TextField from '@mui/material/TextField';
import {Button, Typography } from '@mui/material';
import IUserData from './Models/Iuser';
//import UserService from "./Services/UserServices";
import axios from 'axios';
import red from '@mui/material/colors/red';
import { blue } from '@mui/material/colors';



const RegisterForm: React.FC = () => {

   
    const initialUserState = {
        id: null,
        firstName: "",
        lastName: "",
        email: "",
        password: "",
        alphaVantageApiKey: ""
    };

 

    const [nameStateError, nameSetStateError] = useState<boolean>(false);
    const [nameErrorMsg, nameSetErrorMsg] = useState<string>('');

    const [lNameStateError, lNameSetStateError] = useState<boolean>(false);
    const [lNameErrorMsg, lNameSetErrorMsg] = useState<string>('');

    const [emailStateError, emailSetStateError] = useState<boolean>(false);
    const [emailErrorMsg, emailSetErrorMsg] = useState<string>('');

    const [pwdStateError, pwdSetStateError] = useState<boolean>(false);
    const [pwdErrorMsg, pwdSetErrorMsg] = useState<string>('');

    const [cpwdStateError, cpwdSetStateError] = useState<boolean>(false);
    const [cpwdErrorMsg, cpwdSetErrorMsg] = useState<string>('');

    const [keyStateError, keySetStateError] = useState<boolean>(false);
    const [keyErrorMsg, keySetErrorMsg] = useState<string>('');

    // is used to hold the state of invalid input
    const [stateError, setStateError] = useState<boolean>(false);

    const [errorMsg, setErrorMsg] = useState<string>('');

    

    const [users, setUser] = useState<IUserData>(initialUserState);
    const [submitted, setSubmitted] = useState<boolean>(false);

  

    let confirmpwd = "";
    const handleInputChange = (event: ChangeEvent<HTMLTextAreaElement>) => {
        if (event.target.name === "confirmPassword") {
            confirmpwd = event.target.value;
        }
        const { name, value } = event.target;
        const test = { ...users, [name]: value }
        
        
        setUser(test);

    

      
    }

   

    const validateFirstName = (e: React.FocusEvent<HTMLTextAreaElement>) => {

        nameSetStateError(false);
        nameSetErrorMsg("");
        const namePattern: RegExp = /^[a-zæøåA-ZÆØÅ]{2,20}$/;
        const ok = namePattern.test(e.target.value);
        if (!ok) {

            nameSetStateError(true);
            setSubmitted(false);
            nameSetErrorMsg("Invalid First Name!");
            return false;
        }

        return true;
    }


    const validateLastName = (e: React.FocusEvent<HTMLTextAreaElement>) => {


        lNameSetStateError(false);
        lNameSetErrorMsg("");
        const namePattern: RegExp = /^[a-zæøåA-ZÆØÅ]{2,20}$/;
        const ok = namePattern.test(e.target.value);
        if (!ok) {

            lNameSetStateError(true);
            setSubmitted(false);
            lNameSetErrorMsg("Invalid Last Name!");
            return false;
        }

        return true;

    }



    const validateEmail = (e: React.FocusEvent<HTMLTextAreaElement>) => {

        emailSetStateError(false);
        emailSetErrorMsg("");
        const namePattern: RegExp = /^[a-zA-Z\\#\\!\\%\\$\\‘\\&\\+\\*\\–\\/\\=\\?\\^\\_\\`\\.\\{\\|\\}\\~]+@[a-zA-Z0-9\\-\\.]{1,63}$/;
        const ok = namePattern.test(e.target.value);
        if (!ok) {

            emailSetStateError(true);
            setSubmitted(false);
            emailSetErrorMsg("Invalid Email!");
            return false
        }

        return true;

    }


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


    const validateConfirmPassword = (e: React.FocusEvent<HTMLTextAreaElement>) => {

        cpwdSetStateError(false);
        cpwdSetErrorMsg("");
        if (e.target.value === "") {
            cpwdSetStateError(true);
            setSubmitted(false);
            cpwdSetErrorMsg("Invalid password!");
            return false;

        }
        return true;
    }


    const validateAlphaKey = (e: React.FocusEvent<HTMLTextAreaElement>) => {
        keySetStateError(false);
        keySetErrorMsg("");
        if (e.target.value === "") {
            keySetStateError(true);
            setSubmitted(false);
            keySetErrorMsg("Invalid key!");
            return false;

        }
        
        return true;
    }

   

   

    




    /**Sends data to the server */
    const send = () => {

        if (users.firstName === "" && users.lastName === "" && users.email === "" && users.password === "" && users.alphaVantageApiKey === "") {

            return;
        }
        else if ((nameStateError || emailStateError || emailStateError || pwdStateError || cpwdStateError || keyStateError)) {

            setErrorMsg("Please fill out the form to register!");
            return;

        }



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
        
   }

  

  
           
    return (

        <Box
            sx={{

                '& > :not(style)': { m: 1 }, m: 2,
            }}
        >
            <Typography variant="h6" mb={2} sx={{ padding: 0, m: 0 }} >Register Form</Typography>

            <Typography variant="body2" mb={2} mt={2} sx={{color: red} }></Typography>


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

