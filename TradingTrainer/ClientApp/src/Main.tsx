import React, { Component, ReactNode } from 'react';
import {RouterProvider, createBrowserRouter} from 'react-router-dom';
import LoginForm from './LoginForm';
import LandingComponent from './LandingComponent';
import PrimaryContainer from './PrimaryContainer';
import ErrorComponent from './ErrorComponent';
import TradingDashboard from './TradingComponents/TradingDashboard';
import RegisterForm from './RegisterForm';


class Main extends Component {

    // The routing feature is created with information from https://reactrouter.com/en/main/start/tutorial#adding-a-router
    private router = createBrowserRouter([
        {
            path: "/",
            element: <PrimaryContainer />,
            errorElement: <ErrorComponent />,
            children: [
                {
                    path: "/",
                    element: <LandingComponent />
                },
                {
                    path: "/login",
                    element: <LoginForm />,
                },
                {

                    path: "/registerForm",
                    element: <RegisterForm />
                }

            ]
        },
        {
            path: "/tradingDashboard",
            element: <TradingDashboard />,

        },
       

        
    ]);

    render() : ReactNode {
        return(<RouterProvider router={this.router} />);
    }
}

export default Main;