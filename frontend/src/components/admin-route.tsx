import { Navigate, Outlet } from "react-router";
import { useAuth } from "@/providers/auth-provider";

/** Restricts nested routes to Admin users; others are redirected to the dashboard. */
export const AdminRoute = () => {
    const { user } = useAuth();

    if (user?.role !== "Admin") {
        return <Navigate to="/" replace />;
    }

    return <Outlet />;
};
