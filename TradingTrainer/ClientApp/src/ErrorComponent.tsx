import { useRouteError } from 'react-router-dom';

export default function ErrorComponent() {
    const error : any = useRouteError();
    console.log(error);

    return (
        <div>
            <h1>An error has occured</h1>
            <p>
                <i>{ error.statusText || error.message}</i>
            </p>
        </div>
        );

}
