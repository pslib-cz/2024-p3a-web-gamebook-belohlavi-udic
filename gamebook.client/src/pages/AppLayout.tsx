import { Outlet, Link } from "react-router-dom";
import MainMenu from "../components/MainMenu";

const AppLayout = () => {
    return (
        <div>
            <header>
                <h1>My App</h1>
            </header>
            <nav>
                <MainMenu />
            </nav>
            <main>
                <Outlet />
            </main>
        </div>
    );
}
export default AppLayout;