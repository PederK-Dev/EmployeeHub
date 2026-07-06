import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter, Route, Routes } from "react-router";
import { AppLayout } from "@/components/app-layout";
import { ProtectedRoute } from "@/components/protected-route";
import { Dashboard } from "@/pages/dashboard";
import { Departments } from "@/pages/departments";
import { Employees } from "@/pages/employees";
import { Login } from "@/pages/login";
import { NotFound } from "@/pages/not-found";
import { AuthProvider } from "@/providers/auth-provider";
import { RouteProvider } from "@/providers/router-provider";
import { ThemeProvider } from "@/providers/theme-provider";
import "@/styles/globals.css";

createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <ThemeProvider>
            <BrowserRouter>
                <RouteProvider>
                    <AuthProvider>
                        <Routes>
                            <Route path="/login" element={<Login />} />
                            <Route element={<ProtectedRoute />}>
                                <Route element={<AppLayout />}>
                                    <Route path="/" element={<Dashboard />} />
                                    <Route path="/departments" element={<Departments />} />
                                    <Route path="/employees" element={<Employees />} />
                                </Route>
                            </Route>
                            <Route path="*" element={<NotFound />} />
                        </Routes>
                    </AuthProvider>
                </RouteProvider>
            </BrowserRouter>
        </ThemeProvider>
    </StrictMode>,
);
