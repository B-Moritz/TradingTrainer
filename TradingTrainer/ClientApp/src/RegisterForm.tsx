import React, { ReactNode, Component } from 'react';
import Box from '@mui/material/Box';
import { Link } from 'react-router-dom';
import TextField from '@mui/material/TextField';
import {Typography } from '@mui/material';


class RegisterForm extends Component {

    render(): ReactNode {
        return (
            <Box
                component="form"
                sx={{

                    '& > :not(style)': { m: 1}, m: 2, 

                }}
                noValidate
                autoComplete="off"
            >  
                <Typography variant="h6" mb={2} sx={{padding: 0, m: 0}}>Register Form</Typography>

                <TextField id="outlined-basic" label="First Name" variant="outlined" fullWidth />
                <TextField id="outlined-basic" label="Last Name" variant="outlined" fullWidth />
                <TextField id="outlined-basic" label="Email" variant="outlined" fullWidth />
                <TextField id="outlined-basic" label="Password" type="Password" variant="outlined" fullWidth />
                <TextField id="outlined-basic" label="Confirm Password" type="Password" variant="outlined" fullWidth />
                <TextField id="outlined-basic" label="AlphaVantage Key" variant="outlined" fullWidth />
                <nav className="landingNavigation">
                    <Link className="btn btn-lg btn-outline-secondary" to="/">Cancel</Link>
                    <Link className="btn btn-lg btn-outline-primary" to="/login">Register</Link>
                </nav>

                <Typography variant="body1" sx={{color: "red", pt: 5 }}>Sign up to <a href="https://www.alphavantage.co/">Alpha Vantage</a> get a free access key</Typography>      
                
                
            </Box>
        );
    }
}

export default RegisterForm;