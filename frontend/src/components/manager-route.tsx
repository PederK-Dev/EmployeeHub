import { Navigate, Outlet } from "react-router";
import { useAuth } from "@/providers/auth-provider";

/** Restricts nested routes to Manager and Admin users; others are redirected to the dashboard. */
export const ManagerRoute = () => {
    const { user } = useAuth();

    if (user?.role !== "Admin" && user?.role !== "Manager") {
        return <Navigate to="/" replace />;
    }

    return <Outlet />;
};
