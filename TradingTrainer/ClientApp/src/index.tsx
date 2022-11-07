import react from 'react';
import { createRoot } from 'react-dom/client';
import Main from './Main';
import 'bootstrap/dist/css/bootstrap.css';
import './index.css';

const rootContainer : any = document.getElementById("root");
const root = createRoot(rootContainer);

root.render(<Main />);