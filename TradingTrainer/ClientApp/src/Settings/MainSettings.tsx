import { User } from '../LoginForm';
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import TextField from '@mui/material/TextField';
import Box from '@mui/material/Box';
import FormControl from '@mui/material/FormControl';
import Select, { SelectChangeEvent } from '@mui/material/Select';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem'
import { getUserProfile, saveUserProfile } from '../Service/TradingApi';
import WaitingDisplay from '../WaitingDisplay';
import { SettingPages } from './Settings';

type MainSettingsProps = {
    User : User,
    SetUser : React.Dispatch<React.SetStateAction<User>>,
    CurSettingsPage : number,
    SetCurSettingsPage : React.Dispatch<React.SetStateAction<number>>,
    SetErrorMsg : React.Dispatch<React.SetStateAction<string>>
}

type ProfileInput = {
    Id : string
    Value : string,
    IsValid : boolean
    ErrorMsg : string
    Label : string
    RegexPattern : string
}

type Profile = {
    Id : number
    FirstName : ProfileInput
    LastName : ProfileInput
    Email : ProfileInput
    AlphaVantageApiKey : ProfileInput
  // The buying power of the user
    FundsAvailable : string
  // The total amount of funds that have been invested by the user since the last reset.
    FundsSpent : string
    Currency : ProfileInput
}

function MainSettings(props : MainSettingsProps) : JSX.Element {

    const initialFormInputValues = {
        Id : 0,
        FirstName : {
                Id : "FirstName",
                Value : "", 
                IsValid : true, 
                ErrorMsg : "The provided first name was not recognized as a valid first name!", 
                Label : "First Name",
                RegexPattern : "^([\\w\\s]{2,50})$"
            },
        LastName : {
                Id : "LastName",
                Value : "", 
                IsValid : true, 
                ErrorMsg : "The provided last name was not recognized as a valid last name!", 
                Label : "Last Name",
                RegexPattern : "^([\\w\\s]{2,50})$"
            },
        Email : {
            Id : "Email",
            Value : "", 
            IsValid : true, 
            ErrorMsg : "The provided email/username was not recognized as a valid email address!", 
            Label : "Email/Username",
            RegexPattern : "^[a-zA-Z\\#\\!\\%\\$\\‘\\&\\+\\*\\–\\/\\=\\?\\^\\_\\`\\.\\{\\|\\}\\~]+@[a-zA-Z0-9\\-\\.]{1,63}$"
        },
        AlphaVantageApiKey : {
            Id : "AlphaVantageApiKey",
            Value : "", 
            IsValid : true, 
            ErrorMsg : "The provided api key is not valid!",
            Label : "Alpha Vantage Api Key",
            RegexPattern : "^[A-Z0-9a-z]{2,50}$"
        },
      // The buying power of the user
        FundsAvailable : "",
      // The total amount of funds that have been invested by the user since the last reset.
        FundsSpent : "",
        Currency : {
            Id : "Currency",
            Value : "", 
            IsValid : true, 
            ErrorMsg : "The provided currency was not recognized as a valid currency!",
            Label : "Currency",
            RegexPattern : "^[A-Z]{3}$"
        }
    }
    
    const navigate = useNavigate();
    const [curProfile, setCurProfile] = useState<Profile>(initialFormInputValues);
    const [isWaiting, setIsWaiting] = useState<JSX.Element>();

    useEffect(() => {
        const newProfileSettings : Profile = {
            Id : props.User.id,
            FirstName : curProfile.FirstName,
            LastName : curProfile.LastName,
            Email : curProfile.Email,
            AlphaVantageApiKey : curProfile.AlphaVantageApiKey,
            Currency : curProfile.Currency,
            FundsAvailable : (props.User.fundsAvailable ? props.User.fundsAvailable : ""),
            FundsSpent : (props.User.fundsSpent ? props.User.fundsSpent : "")
        }

        newProfileSettings.FirstName.Value = (props.User.firstName ? props.User.firstName : "");
        newProfileSettings.LastName.Value = (props.User.lastName ? props.User.lastName : "");
        newProfileSettings.Email.Value = (props.User.email ? props.User.email : "");
        newProfileSettings.Currency.Value = (props.User.currency ? props.User.currency : "");
        newProfileSettings.AlphaVantageApiKey.Value = (props.User.alphaVantageApiKey ? props.User.alphaVantageApiKey : "");
        setCurProfile(newProfileSettings);
    }, [props.User]);

    const onFormChange = (curProfileInput : ProfileInput, val : string | null) => {
        let curValue = ""
        const curObj : Profile = {
            Id : curProfile.Id,
            FirstName : curProfile.FirstName,
            LastName : curProfile.LastName,
            Email : curProfile.Email,
            AlphaVantageApiKey : curProfile.AlphaVantageApiKey,
            Currency : curProfile.Currency,
            FundsAvailable : curProfile.FundsAvailable,
            FundsSpent : curProfile.FundsSpent
        }

        if (val) {
            // Input is undefined
            curValue = val;
        }
        // Validate input
        const curPattern = RegExp(curProfileInput.RegexPattern, "g")
        if (!curPattern.test(curValue)) {
            // The input value is not valid
            curProfileInput.IsValid = false;
        } else {
            // The input value is valid
            curProfileInput.IsValid = true;
        }

        // Setting the current value
        curProfileInput.Value = curValue;
        switch (curProfileInput.Label) {
            case "First Name":
                curObj.FirstName = curProfileInput;
                break;
            case "Last Name":
                curObj.LastName = curProfileInput;
                break;
            case "Email/Username":
                curObj.Email = curProfileInput;
                break;
            case "Alpha Vantage Api Key":
                curObj.AlphaVantageApiKey = curProfileInput;
                break;
            default:
                curObj.Currency = curProfileInput;
        }
        // Setting the current values of the input fields - component is reloaded
        setCurProfile(curObj);
    }

    const saveSettings = async () => {
        setIsWaiting(<WaitingDisplay WaitingText={"Updating profile...."}></WaitingDisplay>);
        // Convert to User object
        const newUserObj : User = {
            id : curProfile.Id,
            firstName : curProfile.FirstName.Value,
            lastName : curProfile.LastName.Value,
            alphaVantageApiKey : curProfile.AlphaVantageApiKey.Value,
            email : curProfile.Email.Value,
            currency : curProfile.Currency.Value,
        } 
        if (props.User.id > 0) {
            await saveUserProfile(newUserObj).then((data) => {
                // Create a new object for the profile state - consisting of old values 
                const newProfileSettings : Profile = {
                    Id : data.id,
                    FirstName : curProfile.FirstName,
                    LastName : curProfile.LastName,
                    Email : curProfile.Email,
                    AlphaVantageApiKey : curProfile.AlphaVantageApiKey,
                    Currency : curProfile.Currency,
                    FundsAvailable : (data.fundsAvailable ? data.fundsAvailable : ""),
                    FundsSpent : (data.fundsSpent ? data.fundsSpent : "")
                }
    
                newProfileSettings.FirstName.Value = (data.firstName ? data.firstName : "");
                newProfileSettings.LastName.Value = (data.lastName ? data.lastName : "");
                newProfileSettings.Email.Value = (data.email ? data.email : "");
                newProfileSettings.Currency.Value = (data.currency ? data.currency : "");
                newProfileSettings.AlphaVantageApiKey.Value = (data.alphaVantageApiKey ? data.alphaVantageApiKey : "");
                setCurProfile(newProfileSettings);
                setIsWaiting(<></>);
            }).catch((error : Error) => {
                setIsWaiting(<></>);
                if (error.message.slice(3) === "401") {
                    navigate("/login");
                }
                props.SetErrorMsg(error.toString());
            });
        }
    }
    
    let inputForm : JSX.Element[] = [];
    let someInputIsNotValid = false;
    const inputValues = [curProfile.FirstName, curProfile.LastName, curProfile.Email, curProfile.AlphaVantageApiKey];
    for (const curValue of inputValues) {
        if (!curValue.IsValid) {
            inputForm.push(<TextField 
                                className="settingsFormInput "
                                key={curValue.Id}
                                id={curValue.Id}
                                label={curValue.Label} 
                                variant="outlined" 
                                value={curValue.Value}
                                helperText={curValue.ErrorMsg}
                                onChange={(e : React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
                                    e.preventDefault();
                                    let inputValue = e.target.value; 
                                    if (!inputValue) {
                                        inputValue = "";
                                    }
                                    onFormChange(curValue, inputValue);
                                }}
                                required
                                error
                            />);
            someInputIsNotValid = true;
        } else {
            inputForm.push(<TextField 
                                className="settingsFormInput"
                                key={curValue.Id}
                                id={curValue.Id}
                                label={curValue.Label} 
                                variant="outlined" 
                                value={curValue.Value}
                                onChange={(e : React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
                                    e.preventDefault();
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
        <FormControl id={curProfile.Currency.Id} className="settingsFormInput" key="Currency">
            <InputLabel id="currency-label">Currency</InputLabel>
            <Select
                labelId="currency-label"
                id="CurrencySelector"
                value={curProfile.Currency.Value}
                label="Currency"
                onChange={(e : SelectChangeEvent) => {
                    e.preventDefault();
                    let inputValue = e.target.value; 
                    if (!inputValue) {
                        inputValue = "";
                    }
                    onFormChange(curProfile.Currency, inputValue);
                }}
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

    inputForm.push(<p key="FundsSpent" id="FundsSpent">Funds Spent: {curProfile.FundsSpent}</p>);
    inputForm.push(<p key="FundsAvailable" id="FundsAvailable">Funds Available: {curProfile.FundsAvailable}</p>);

    return(
        <>
            <h2>Settings</h2>
            <p>
                Change your user and app settings. Make sure to save the changes you make.
            </p>
            <Box className="settingsFormContainer" component="div" sx={{margin: 2}}>
                {inputForm}
                <nav id="SettingsNavigation" className="btn-group">
                    <button className={"btn btn-lg btn-warning " + (someInputIsNotValid ? "disabled" : "")} onClick={(someInputIsNotValid ? () => {} : saveSettings)}>Save Settings</button>
                    <button className='btn btn-lg btn-danger' onClick={() => {props.SetCurSettingsPage(SettingPages.ConfirmReset)}}>Reset profile</button>
                    <button className="btn btn-lg btn-primary" onClick={() => {props.SetCurSettingsPage(SettingPages.PwdChange)}}>Reset password</button>
                </nav>
            </Box>
            {isWaiting}
        </>
    );
}

export type { ProfileInput };
export type { Profile };
export default MainSettings;