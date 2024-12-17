import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { AuthProvider } from './providers/AuthProvider';
import AppLayout from './pages/AppLayout';
import FrontPage from './pages/FrontPage';
import SignUpPage from './pages/SignUpPage';
import SignInPage from './pages/SignInPage';
import GamePage from './pages/GamePage';
import './App.css';

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
                path: "game",
                element: <GamePage />
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