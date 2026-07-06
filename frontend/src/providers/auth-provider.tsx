import { createContext, useCallback, useContext, useEffect, useMemo, useState, type PropsWithChildren } from "react";
import { useNavigate } from "react-router";
import { authApi, setUnauthorizedHandler, TOKEN_KEY, USER_KEY } from "@/lib/api";
import type { User } from "@/lib/types";

interface AuthContextValue {
    user: User | null;
    token: string | null;
    isAuthenticated: boolean;
    login: (email: string, password: string) => Promise<void>;
    logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function readStoredUser(): User | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
        return JSON.parse(raw) as User;
    } catch {
        return null;
    }
}

export const AuthProvider = ({ children }: PropsWithChildren) => {
    const navigate = useNavigate();
    const [token, setToken] = useState<string | null>(() => localStorage.getItem(TOKEN_KEY));
    const [user, setUser] = useState<User | null>(() => readStoredUser());

    const logout = useCallback(() => {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
        setToken(null);
        setUser(null);
        navigate("/login", { replace: true });
    }, [navigate]);

    // Let the API layer trigger a logout on any 401.
    useEffect(() => {
        setUnauthorizedHandler(logout);
        return () => setUnauthorizedHandler(null);
    }, [logout]);

    const login = useCallback(async (email: string, password: string) => {
        const result = await authApi.login(email, password);
        localStorage.setItem(TOKEN_KEY, result.token);
        localStorage.setItem(USER_KEY, JSON.stringify(result.user));
        setToken(result.token);
        setUser(result.user);
    }, []);

    const value = useMemo<AuthContextValue>(
        () => ({ user, token, isAuthenticated: !!token, login, logout }),
        [user, token, login, logout],
    );

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error("useAuth must be used within an AuthProvider");
    }
    return context;
};
