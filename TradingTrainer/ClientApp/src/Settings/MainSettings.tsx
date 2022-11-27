import { User } from '../LoginForm';
import React, { useState } from 'react';
import TextField from '@mui/material/TextField';
import Box from '@mui/material/Box';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem'
import { Profile, ProfileInput } from './Settings';

type MainSettingsProps = {
    Profile : Profile
    setProfile : React.Dispatch<React.SetStateAction<Profile>>
}

function MainSettings(props : MainSettingsProps) : JSX.Element {

    const onFormChange = (curProfileInput : ProfileInput, val : string | null) => {
        let curValue = ""
        if (val) {
            // Input is undefined
            curValue = val;
        }
        // Validate input
        if (!curProfileInput.RegexPattern.test(curValue)) {
            // The input value is not valid
            curProfileInput.IsValid = false;
        } else {
            // The input value is valid
            curProfileInput.IsValid = true;
        }

        // Setting the current value
        curProfileInput.Value = curValue;
        props.setProfile(props.Profile);
    }
    
    let inputForm : JSX.Element[] = [];
    const inputValues = [props.Profile.FirstName, props.Profile.LastName, props.Profile.Email];
    for (const curValue of inputValues) {
        if (!curValue.IsValid) {
            inputForm.push(<TextField 
                                key={curValue.Label}
                                id="outlined-error" 
                                label={curValue.Label} 
                                variant="outlined" 
                                defaultValue={curValue.Value}
                                helperText={curValue.ErrorMsg}
                                onChange={(e : React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
                                    let inputValue = e.target.value; 
                                    if (!inputValue) {
                                        inputValue = "";
                                    }
                                    onFormChange(curValue, inputValue);
                                }}
                                required
                                error
                            />);
        } else {
            inputForm.push(<TextField 
                                key={curValue.Label}
                                id="outlined-basic" 
                                label={curValue.Label} 
                                variant="outlined" 
                                defaultValue={curValue.Value}
                                helperText={curValue.ErrorMsg}
                                onChange={(e : React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
                                    let inputValue = e.target.value; 
                                    if (!inputValue) {
                                        inputValue = "";
                                    }
                                    onFormChange(curValue, inputValue);
                                }}
                                required
                            />);
        }
    }

    inputForm.push(
        <FormControl fullWidth key="Currency">
            <InputLabel id="currency-label">Currency</InputLabel>
            <Select
                labelId="currency-label"
                id="demo-simple-select"
                value={props.Profile.Currency.Value}
                label="Currency"
            >
                <MenuItem value={'NOK'}>NOK</MenuItem>
                <MenuItem value={'USD'}>USD</MenuItem>
                <MenuItem value={'CHF'}>CHF</MenuItem>
                <MenuItem value={'EUR'}>EUR</MenuItem>
                <MenuItem value={'DKK'}>DKK</MenuItem>
                <MenuItem value={'SEK'}>SEK</MenuItem>
                <MenuItem value={'RUB'}>RUB</MenuItem>
                <MenuItem value={'CAD'}>CAD</MenuItem>
                <MenuItem value={'BRL'}>BRL</MenuItem>
            </Select>
        </FormControl>
        );

    return(
        <>
            <h2>Settings</h2>
            <p>
                Change your user and app settings. Make sure to save the changes you make.
            </p>
            <Box component="form" sx={{margin: 2}}>
                {inputForm}
                <nav>
                    <button className="btn btn-lg btn-warning">Save Settings</button>
                    <button className='btn btn-lg btn-danger'>Reset profile</button>
                    <button className="btn btn-lg btn-primary">Reset password</button>
                </nav>
            </Box>
        </>
    );
}

export default MainSettings