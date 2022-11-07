import React, { Component, ReactNode } from 'react';
import {RouterProvider, createBrowserRouter} from 'react-router-dom';
import LoginForm from './LoginForm';
import LandingComponent from './LandingComponent';
import PrimaryContainer from './PrimaryContainer';
import ErrorComponent from './ErrorComponent';
import TradingDashboard from './TradingComponents/TradingDashboard';


class Main extends Component {

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
                    path: "/Login",
                    element: <LoginForm />,
                }
            ]
        },
        {
            path: "/TradingDashboard",
            element: <TradingDashboard />,

        }

        
    ]);

    render() : ReactNode {
        return(<RouterProvider router={this.router} />);
    }
}

export default Main;