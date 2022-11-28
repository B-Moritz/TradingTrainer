import React, { ReactNode, Component, useState, ChangeEvent } from 'react';
import Box from '@mui/material/Box';
import { Link } from 'react-router-dom';
import TextField from '@mui/material/TextField';
import {Typography } from '@mui/material';
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

    const [users, setUser] = useState<IUserData>(initialUserState);
    const [submitted, setSubmitted] = useState<boolean>(false);

    const handleInputChange = (event: ChangeEvent<HTMLTextAreaElement>) => {
        const { name, value } = event.target;
        const test = { ...users, [name]: value }
        console.log(test)
        setUser(test);
    }

    const send = () => {

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

        const newUser = () => {
            setUser(initialUserState);
            setSubmitted(false);
        };
    }
           
    return (
        <Box
            component="form"

            sx={{

                '& > :not(style)': { m: 1 }, m: 2,


            }}
            noValidate
            autoComplete="off"
               
            >  
                <Typography variant="h6" mb={2} sx={{padding: 0, m: 0}}>Register Form</Typography>

                <TextField name="firstName" label="First Name" variant="outlined" value={users.firstName} onChange={handleInputChange} fullWidth />
                <TextField name="lastName" label="Last Name" variant="outlined" value={users.lastName} onChange={handleInputChange} fullWidth />
                <TextField name="email" label="Email" variant="outlined" value={users.email} onChange={handleInputChange} fullWidth />
                <TextField name="password" label="Password" type="Password" variant="outlined" value={users.password} onChange={handleInputChange} fullWidth />
                <TextField name="confirmPassword" label="Confirm Password" type="Password" variant="outlined"   fullWidth />
                <TextField name="alphaVantageApiKey" id="outlined-basic" label="AlphaVantage Key" variant="outlined" value={users.alphaVantageApiKey} onChange={handleInputChange} fullWidth />
                <nav className="landingNavigation">
                    <Link className="btn btn-lg btn-outline-secondary" to="/">Cancel</Link>
                    <Link className="btn btn-lg btn-outline-primary" to="/login" onClick={send} > Register</Link>
                </nav>

                <Typography variant="body1" sx={{color: "red", pt: 5 }}>Sign up to <a href="https://www.alphavantage.co/">Alpha Vantage</a> get a free access key</Typography>      
                

            </Box>
        );
    }

export default RegisterForm;