import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter, Route, Routes } from "react-router";
import { AdminRoute } from "@/components/admin-route";
import { AppLayout } from "@/components/app-layout";
import { ManagerRoute } from "@/components/manager-route";
import { ProtectedRoute } from "@/components/protected-route";
import { Dashboard } from "@/pages/dashboard";
import { Departments } from "@/pages/departments";
import { Employees } from "@/pages/employees";
import { LeaveRequests } from "@/pages/leave-requests";
import { Login } from "@/pages/login";
import { NotFound } from "@/pages/not-found";
import { Positions } from "@/pages/positions";
import { Users } from "@/pages/users";
import { AuthProvider } from "@/providers/auth-provider";
import { RouteProvider } from "@/providers/router-provider";
import { ThemeProvider } from "@/providers/theme-provider";
import { ToastProvider } from "@/providers/toast-provider";
import "@/styles/globals.css";

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <ThemeProvider>
            <BrowserRouter>
                <RouteProvider>
                    <ToastProvider>
                        <AuthProvider>
                            <Routes>
                                <Route path="/login" element={<Login />} />
                                <Route element={<ProtectedRoute />}>
                                    <Route element={<AppLayout />}>
                                        <Route path="/" element={<Dashboard />} />
                                        <Route path="/departments" element={<Departments />} />
                                        <Route path="/positions" element={<Positions />} />
                                        <Route path="/leave" element={<LeaveRequests />} />
                                        <Route element={<ManagerRoute />}>
                                            <Route path="/employees" element={<Employees />} />
                                        </Route>
                                        <Route element={<AdminRoute />}>
                                            <Route path="/users" element={<Users />} />
                                        </Route>
                                    </Route>
                                </Route>
                                <Route path="*" element={<NotFound />} />
                            </Routes>
                        </AuthProvider>
                    </ToastProvider>
                </RouteProvider>
            </BrowserRouter>
        </ThemeProvider>
    </StrictMode>,
);
