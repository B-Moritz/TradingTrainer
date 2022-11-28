﻿import React, { Component, useState } from 'react';
import {RouterProvider, createBrowserRouter, useNavigate} from 'react-router-dom';
import LoginForm, {User} from './LoginForm';
import LandingComponent from './LandingComponent';
import PrimaryContainer from './PrimaryContainer';
import ErrorComponent from './ErrorComponent';
import TradingDashboard from './TradingComponents/TradingDashboard';
import RegisterForm from './RegisterForm';
import AppContainer from './AppContainer';
import Settings from './Settings/Settings';


type MainProps = {}


function Main(props: MainProps) : JSX.Element {
    const initialUser : User = {
        id : 0,
        firstName : "",
        lastName : "",
        email : "",
        fundsAvailable : "",
        fundsSpent : "",
        currency : ""
    };
    const [authenticatedUser, setAuthenticatedUser] = useState(initialUser);
    //const [isAuthenticated, setIsAuthenticated] = useState(false);
    // The routing feature is created with information from https://reactrouter.com/en/main/start/tutorial#adding-a-router
    const router = createBrowserRouter([
        {
            path: "/",
            element: <PrimaryContainer />,
            errorElement: <ErrorComponent />,
            children: [
                {
                    path: "/",
                    element: <LandingComponent />,
                },
                {
                    path: "/login",
                    element: <LoginForm 
                                SetUser={setAuthenticatedUser}
                                User={authenticatedUser}
                                //SetIsAuthenticated={setIsAuthenticated}
                            />,
                },
                {
                    path: "/registerForm",
                    element: <RegisterForm />,
                }
            ]
        },
        {
            path: "/tradingDashboard",
            element: <AppContainer 
                        User={authenticatedUser}
                        SetUser={setAuthenticatedUser}
                    />,
            errorElement: <ErrorComponent />,
            children: [
                {
                    path: "/tradingDashboard",
                    element: <TradingDashboard 
                                User={authenticatedUser}
                                SetUser={setAuthenticatedUser}
                            />,
                },
                {
                    path: "/tradingDashboard/settings",
                    element: <Settings 
                                User={authenticatedUser}
                                SetUser={setAuthenticatedUser}
                            />
                }
            ]
        }
    ]);

    return(
        <RouterProvider router={router} />
    );
}

export default Main;