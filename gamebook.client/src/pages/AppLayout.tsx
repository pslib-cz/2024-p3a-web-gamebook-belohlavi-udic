import { Outlet } from "react-router-dom";
import MainMenu from "../components/MainMenu";
import '../styles/AppLayout.css';

const AppLayout = () => {
    return (
        <div className="app-layout">
            <header className="app-header">
                <MainMenu />
            </header>
            <main className="app-main">
                <Outlet />
            </main>
        </div>
    );
}

export default AppLayout;