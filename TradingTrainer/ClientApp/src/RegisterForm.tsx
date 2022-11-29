import React, { ReactNode, Component, useState, ChangeEvent } from 'react';
import { useForm, SubmitHandler } from "react-hook-form";
import Box from '@mui/material/Box';
import { Link } from 'react-router-dom';
import TextField from '@mui/material/TextField';
import {Button, makeStyles, Typography } from '@mui/material';
import IUserData from './Models/Iuser';
//import UserService from "./Services/UserServices";
import axios from 'axios';



const RegisterForm: React.FC = () => {

   
    const initialUserState = {
        id: null,
        firstName: "",
        lastName: "",
        email: "",
        password: "",
        alphaVantageApiKey: ""
    };

    /** Register validation */

    interface IFormInput {
        firstName: string, 
        lastName: string,
        email: string,
        password: string,
        confirmPassword: string,
        alphaVantageApiKey: string
    }

    const [stateError, setStateError] = useState<boolean>(false);
    const [errorMsg, setErrorMsg] = useState<string>('');
    const onSubmit: SubmitHandler<IFormInput> = data => console.log(data);

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

    /**
     * Validate each input feild*/

    const ValidateName = (name: string) => {

        const namePattern: RegExp = /^[a-zæøåA-ZÆØÅ]{2,20}$/;
        if (namePattern.test(name)) {

            return true;

        }

        return false;

    }

    const ValidateEmail = (email: string) => {
        const emailPattern: RegExp = /^[a-zA-Z\\#\\!\\%\\$\\‘\\&\\+\\*\\–\\/\\=\\?\\^\\_\\`\\.\\{\\|\\}\\~]+@[a-zA-Z0-9\\-\\.]{1,63}$/

        if (emailPattern.test(email)) {
            return true;
        }

        return false;
    }

    const ValidatePassword = (pwd: string) => {
        if (pwd.length < 9) {
            // The password length is invalid
            return false;
            
            
        }
        if (!(/\d/.test(pwd))) {
            // The passsword does not contain any numbers
           return false;
           
            
        }
        if (!(/\w/.test(pwd))) {
            // The password does not contain any word characters
           return false;
            
           
        }
        if (!(/[$&+,:;=?@#|'<>.-^*()%!]/.test(pwd))) {
            // The password does not contain any special characters
            return false;
           
            
        }

        return true;

    }

   

    const ValidateAlphaVantageKey = (key: string) => {
        if (users.alphaVantageApiKey === initialUserState.alphaVantageApiKey) {
            return false;
        }
        return true;
    }
   



    const validate = (e: React.FocusEvent<HTMLTextAreaElement>) => {
        setStateError(false);
        setErrorMsg("");

        let element = e.target.name;

        if (element === "firstName") {
            if (!ValidateName(e.target.value)) {

                setStateError(true);
                setSubmitted(false);
                setErrorMsg("Invalid First Name!");
            }
        }

        else if (element === "lastName") {
            if (!ValidateName(e.target.value)) {
                setStateError(true);
                setSubmitted(false);
                setErrorMsg("Invalid Last Name!");
            }
        }

        else if (element === "email") {
            if (!ValidateEmail(e.target.value)) {
                setStateError(true);
                setSubmitted(false);
                setErrorMsg("Invalid Email!");

            }
        }

        else if (element === "password") {
            if (!ValidatePassword(e.target.value)) {
                setStateError(true);
                setSubmitted(false);
                setErrorMsg("Invalid password. Password must contain more than 9 characters, numbers 0-9, letter characters and special characters!");
            }
        }

        else if (element === "alphaVantageApiKey") {
            if (!ValidateAlphaVantageKey(e.target.value)) {
                setStateError(true);
                setSubmitted(false);
                setErrorMsg("Invalid key!");
            }
        }
        

    }


    /**Checks if all fields are empty  */

    const emptyFeildCheck = () => {

        if (users === initialUserState) {
            setStateError(true);
            setSubmitted(false);
            setErrorMsg("The field can not be empty");
            return true;
        }

        return false;
    }

    const pwdConfirmpswMatch = () => {
        if (confirmpwd === users.password) {
            return true;
        }
        setStateError(true);
        setSubmitted(false);
        setErrorMsg("Confirm your password!");
        return false;

    }




    /**Sends data to the server */
    const send = () => {

        /*if all the feilds are empty retun without doing anything more*/
        const check = emptyFeildCheck();
        if (check) {

            return;

        }

        const pwdMatch = pwdConfirmpswMatch();
        if (!pwdMatch) {

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
            <Typography variant="h6" mb={2} sx={{ padding: 0, m: 0 }}>Register Form</Typography>
            <form noValidate autoComplete="off">

                <TextField
                    sx={{ mt: 2, mb: 2 }}

                    name="firstName"
                    label="First Name"
                    variant="outlined"
                    value={users.firstName}
                    onChange={handleInputChange}
                    onBlur={validate}
                    fullWidth
                    required
                    error={stateError}
                    helperText={errorMsg}

            />
          
                <TextField
                    sx={{ mt: 2, mb: 2 }}
                    name="lastName"
                    label="Last Name"
                    variant="outlined"
                    value={users.lastName}
                    onChange={handleInputChange}
                    onBlur={validate}
                    fullWidth />
                <TextField
                    sx={{ mt: 2, mb: 2 }}
                    name="email"
                    label="Email"
                    variant="outlined"
                    value={users.email}
                    onChange={handleInputChange}
                    onBlur={validate}
                    fullWidth
                />
                <TextField
                    sx={{ mt: 2, mb: 2 }}
                    name="password"
                    label="Password"
                    type="Password"
                    variant="outlined"
                    value={users.password}
                    onChange={handleInputChange}
                    onBlur={validate}
                    fullWidth
                />
                <TextField
                    sx={{ mt: 2, mb: 2 }}
                    name="confirmPassword"
                    label="Confirm Password"
                    type="Password"
                    variant="outlined"
                    onChange={handleInputChange}
                    onBlur={validate}
                    fullWidth
                />
                <TextField
                    sx={{ mt: 2, mb: 2 }}
                    name="alphaVantageApiKey"
                    id="outlined-basic"
                    label="AlphaVantage Key"
                    variant="outlined"
                    value={users.alphaVantageApiKey}
                    onChange={handleInputChange}
                    onBlur={validate}
                    fullWidth
                />
                <nav className="landingNavigation">
                    <Link className="btn btn-lg btn-outline-secondary" to="/">Cancel</Link>
                    <Button className="btn btn-lg btn-outline-primary" type="submit" onClick={send} > Register</Button>
            </nav>
    </form>
            <Typography variant="body1" sx={{ color: "red", pt: 5 }}>Sign up to <a href="https://www.alphavantage.co/">Alpha Vantage</a> get a free access key</Typography>


        </Box>



        );
    }

export default RegisterForm;

