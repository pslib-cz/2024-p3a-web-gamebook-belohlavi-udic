import {
    createBrowserRouter,
    RouterProvider,
  } from "react-router-dom";
import './App.css';
import {AuthProvider} from './providers/AuthProvider';
import AppLayout from './pages/AppLayout';
import FrontPage from './pages/FrontPage';
import SignUpPage from './pages/SignUpPage';
import SignInPage from './pages/SignInPage';
import TokenPage from './pages/TokenPage';
import StartPage from "./pages/StartPage";

const router = createBrowserRouter([
    {
        path: "/",
        element: <AppLayout />,
        children: [
            {
                index: true,
                element: <FrontPage />
            },
            {
                path: "sign-up",
                element: <SignUpPage />
            },
            {
                path: "sign-in",
                element: <SignInPage />
            },
            {
                path: "token",
                element: <TokenPage />
            },
            {
                path: "start",
                element: <StartPage />
            }
        ]
    }
]);

function App() {
    return (
        <AuthProvider>
            <RouterProvider router={router} />
        </AuthProvider>
    );
}

export default App;