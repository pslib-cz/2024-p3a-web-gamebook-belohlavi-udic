import { createBrowserRouter, RouterProvider } from "react-router-dom";
import { AuthProvider } from './providers/AuthProvider';
import AppLayout from './pages/AppLayout';
import FrontPage from './pages/FrontPage';
import SignUpPage from './pages/SignUpPage';
import SignInPage from './pages/SignInPage';
import GamePage from './pages/GamePage';
import StartPage from './pages/StartPage';
import NotFoundPage from './pages/NotFound';
import RoomPage from './pages/RoomPage';
import { GameProvider } from './contexts/GameContext';
import { ChallengeProvider } from './contexts/ChallengeContext';

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
                path: "start",
                element: <GameProvider><StartPage /></GameProvider>
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
                element: <GameProvider><GamePage /></GameProvider>
            },
            {
                path: "room/:roomId",
                element: (
                    <GameProvider>
                        <ChallengeProvider>
                            <RoomPage />
                        </ChallengeProvider>
                    </GameProvider>
                )
            },
            {
                path: "*",
                element: <NotFoundPage />
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